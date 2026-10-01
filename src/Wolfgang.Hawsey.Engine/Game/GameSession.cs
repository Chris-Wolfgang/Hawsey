using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Game;

/// <summary>
/// A live game between one human, always South, and three <see cref="SimpleAiStrategy"/>
/// seats. It is what every Hawsey UI drives: the UI calls the human's moves and the
/// AI-advance methods, and redraws on <see cref="StateChanged"/>. The AI moves are
/// paced so a person can follow them.
/// </summary>
/// <remarks>
/// Thread safety: the AI loops resume on thread-pool threads after their delays
/// while human moves arrive on the UI thread, so two moves can race for the same
/// turn. So every transition of <c>_state</c> runs under <c>_sync</c>, and
/// each one re-checks, inside the lock, that it is still the right phase and
/// player's turn. A stale human move (a double-tap, a tap during an AI turn)
/// is ignored and reported as <c>false</c>. <c>_generation</c> increments on
/// every new game, so an AI loop that wakes up after New Game stops instead of
/// playing into the new game. Events are raised outside the lock.
/// </remarks>
public class GameSession
{
    /// <summary>
    /// The seat the human plays. The AI plays the other three.
    /// </summary>
    public const PlayerPosition HumanPosition = PlayerPosition.South;


    private readonly GameEngine _engine = new();
    private readonly bool _aiPacing;
    private readonly Func<Random> _randomFactory;
    private readonly IPlayerStrategy _aiStrategy;
    // MA0158 (use System.Threading.Lock) does not apply here: the Coyote concurrency tests
    // (tests/Wolfgang.Hawsey.UI.Maui.Tests.Concurrency) control lock ordering by rewriting
    // Monitor, which `lock (object)` compiles to. Coyote 1.7.11 cannot see
    // System.Threading.Lock: with it, both tests fail with "Potential deadlock or hang
    // detected" (verified). Keep the Monitor-based lock while the tests rely on Coyote.
#pragma warning disable MA0158
    private readonly object _sync = new();
#pragma warning restore MA0158
    private GameState? _state;
    private readonly List<PlacedBid> _bids = new();
    private Random _random;
    private int _generation;

    // The state whose won trick has been cleared from the table, so the human leads to
    // an empty table. Any later state (the next lead, a new round, a new game) is a
    // different instance, so the table shows again without resetting this.
    private GameState? _clearedTable;



    /// <summary>
    /// Initializes a new instance of the <see cref="GameSession"/> class, with the AI
    /// seats paced so a person can follow their moves, and a fresh random deal for
    /// each game.
    /// </summary>
    public GameSession()
        : this(aiPacing: true, randomFactory: NewRandom, aiStrategy: new SimpleAiStrategy())
    {
    }



    /// <summary>
    /// Test seam: <paramref name="aiPacing"/> <see langword="false"/> removes the pauses
    /// between AI moves, <paramref name="randomFactory"/> supplies each game's
    /// <see cref="Random"/>, and <paramref name="aiStrategy"/> plays the three AI seats,
    /// so a test can play whole games quickly and repeatably, including lines
    /// <see cref="SimpleAiStrategy"/> never chooses (an AI Hawsey bid).
    /// </summary>
    internal GameSession(bool aiPacing, Func<Random> randomFactory, IPlayerStrategy aiStrategy)
    {
        _aiPacing = aiPacing;
        _randomFactory = randomFactory;
        _aiStrategy = aiStrategy;
        _random = randomFactory();
    }



    /// <summary>
    /// Gets the current game state, or <see langword="null"/> before the first game.
    /// </summary>
    public GameState? CurrentState => Volatile.Read(ref _state);



    /// <summary>
    /// Raised after every change to <see cref="CurrentState"/>. May be raised on a
    /// background thread.
    /// </summary>
    public event EventHandler? StateChanged;



    /// <summary>
    /// Raised when a trick is complete, with the player who won it.
    /// </summary>
    public event EventHandler<TrickCompletedEventArgs>? TrickCompleted;



    /// <summary>
    /// Raised when a round has been scored and the game goes on.
    /// </summary>
    public event EventHandler? RoundCompleted;



    /// <summary>
    /// Raised when a team reaches the points needed to win.
    /// </summary>
    public event EventHandler<GameOverEventArgs>? GameOver;



    /// <summary>
    /// Gets the bids made so far in the current round, in bidding order: a snapshot, so
    /// a UI can show each seat's bid. Empty before the first bid of each deal.
    /// </summary>
    public IReadOnlyList<PlacedBid> Bids
    {
        get
        {
            lock (_sync)
            {
                return _bids.ToArray();
            }
        }
    }



    /// <summary>
    /// Gets the cards to show on the table. This is <see cref="GameState.TableCards"/>, but
    /// when the human wins a trick and is to lead, the won trick shows only for a moment:
    /// the table then clears (with <see cref="StateChanged"/>) so the human leads to an empty
    /// table. After an AI wins, its lead replaces the trick anyway.
    /// </summary>
    public IReadOnlyList<PlayedCard> TableCards => IsTableCleared(out var state) ? Array.Empty<PlayedCard>() : state?.TableCards ?? Array.Empty<PlayedCard>();



    /// <summary>
    /// Gets the card winning the trick in <see cref="TableCards"/>, or <see langword="null"/>
    /// when the table is empty.
    /// </summary>
    public Card? TableWinningCard => IsTableCleared(out var state) ? null : state?.TableWinningCard;



    /// <summary>
    /// Gets a value indicating whether the human is the next to act.
    /// </summary>
    public bool IsHumanTurn => CurrentState?.NextToAct == HumanPosition;



    /// <summary>
    /// Deals a new game with North dealing. Any AI loop still running for the
    /// previous game stops at its next step.
    /// </summary>
    /// <param name="rules">The house rules, or <see langword="null"/> for <see cref="HouseRules.Default"/>.</param>
    public void StartNewGame(HouseRules? rules = null)
    {
        lock (_sync)
        {
            _random = _randomFactory();
            var state = _engine.StartGame(rules ?? HouseRules.Default, PlayerPosition.North, _random);
            _bids.Clear();
            _generation++;
            Volatile.Write(ref _state, state);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }



    /// <summary>
    /// Plays the AI seats' moves, phase after phase, until the game needs something
    /// from outside: a move from the human, or the UI's go-ahead for the next round.
    /// This is the loop every UI runs after each human move, so they all drive the game
    /// the same way. The human's last card of a round is played here too, since there
    /// is no choice to make.
    /// </summary>
    /// <returns>
    /// What the session is waiting for; <see cref="WaitingFor.Nothing"/> when no game has
    /// started, or a newer move (such as a New Game) took over during an AI pause.
    /// </returns>
    public async Task<WaitingFor> AdvanceAsync()
    {
        var generation = CurrentGeneration();

        while (true)
        {
            var state = CurrentState;

            // A New Game during one of the AI pauses below: that game has its own loop.
            if (state == null || generation != CurrentGeneration())
            {
                return WaitingFor.Nothing;
            }

            switch (state.Phase)
            {
                case GamePhase.Bidding:
                    if (await AdvanceAiBiddingAsync().ConfigureAwait(false))
                    {
                        return WaitingFor.HumanBid;
                    }

                    break;

                case GamePhase.TrumpSelection:
                    if (await HandleTrumpSelectionAsync().ConfigureAwait(false))
                    {
                        return WaitingFor.HumanTrump;
                    }

                    break;

                case GamePhase.HawseyExchange:
                    if (await HandleHawseyExchangeAsync().ConfigureAwait(false))
                    {
                        return WaitingFor.HumanHawseyExchange;
                    }

                    break;

                case GamePhase.TrickPlay:
                    if (await AdvanceAiPlaysAsync().ConfigureAwait(false) && !PlayForcedLastCard())
                    {
                        return WaitingFor.HumanCard;
                    }

                    break;

                case GamePhase.RoundScoring:
                    return WaitingFor.NextRound;

                default:
                    return WaitingFor.GameOver;
            }
        }
    }



    /// <summary>
    /// Plays the human's card when it is the only one left in hand. Returns
    /// <see langword="true"/> when it did.
    /// </summary>
    private bool PlayForcedLastCard()
    {
        var hand = CurrentState?.Hands[HumanPosition];

        return hand is { Count: 1 } && PlayHumanCard(hand[0]);
    }



    /// <summary>
    /// Advances AI bidding. Returns true if human needs to bid.
    /// </summary>
    public async Task<bool> AdvanceAiBiddingAsync()
    {
        var generation = CurrentGeneration();

        while (true)
        {
            PlayerPosition bidder;

            lock (_sync)
            {
                if (!TryGetOpenBidding(generation, out var open))
                {
                    return false;
                }

                if (open.NextToAct == HumanPosition)
                {
                    return true;
                }

                // Bidding is open, so someone is next to act.
                bidder = open.NextToAct!.Value;
            }

            await PauseAsync(400).ConfigureAwait(false);

            lock (_sync)
            {
                // Another loop, a new game or a human move may have moved on while we slept.
                if (!TryGetOpenBidding(generation, out var state) || state.NextToAct != bidder)
                {
                    return false;
                }

                var aiBid = _aiStrategy.DecideBid(state, bidder);
                Volatile.Write(ref _state, _engine.PlaceBid(state, bidder, aiBid));
                _bids.Add(new PlacedBid(bidder, aiBid));
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }



    /// <summary>
    /// Places the human's bid. Returns false, and changes nothing, when it is
    /// not the human's turn to bid, or a number bid is below
    /// <see cref="GameState.MinimumLegalBid"/> or above <see cref="GameState.MaximumBid"/>.
    /// </summary>
    public bool PlaceHumanBid(BidAction action)
    {
        lock (_sync)
        {
            if (!TryGetOpenBidding(_generation, out var state) || state.NextToAct != HumanPosition)
            {
                return false;
            }

            // A number bid must beat the high bid, and at least meet the minimum (#867).
            if (action is BidAction.NumberBid { Amount: var amount } && (amount < state.MinimumLegalBid || amount > GameState.MaximumBid))
            {
                return false;
            }

            Volatile.Write(ref _state, _engine.PlaceBid(state, HumanPosition, action));
            _bids.Add(new PlacedBid(HumanPosition, action));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }



    /// <summary>
    /// Names trump for the human. Returns false, and changes nothing, when the
    /// human is not the one naming trump.
    /// </summary>
    public bool SelectTrump(Suit? trumpSuit)
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.TrumpSelection, NextToAct: HumanPosition })
            {
                return false;
            }

            Volatile.Write(ref _state, _engine.SelectTrump(_state, trumpSuit));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }



    /// <summary>
    /// Returns true if human needs to select trump.
    /// </summary>
    public async Task<bool> HandleTrumpSelectionAsync()
    {
        var generation = CurrentGeneration();
        PlayerPosition picker;

        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.TrumpSelection, NextToAct: { } next })
            {
                return false;
            }

            if (next == HumanPosition)
            {
                return true;
            }

            picker = next;
        }

        await PauseAsync(500).ConfigureAwait(false);

        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.TrumpSelection } || _state.NextToAct != picker)
            {
                return false;
            }

            var trump = _aiStrategy.DecideTrump(_state, picker);
            Volatile.Write(ref _state, _engine.SelectTrump(_state, trump));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return false;
    }



    /// <summary>
    /// Applies the human's Hawsey exchange. Returns false, and changes nothing,
    /// unless the game is in the Hawsey exchange phase with the human as bidder.
    /// </summary>
    public bool PerformHawseyExchange(Card[] discard, Card[] fromPartner)
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.HawseyExchange, HawseyBidder: HumanPosition })
            {
                return false;
            }

            Volatile.Write(ref _state, _engine.ExchangeHawseyCards(_state, discard, fromPartner));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }



    /// <summary>
    /// Applies the human's Hawsey exchange from the two cards the human discards. The
    /// partner gives their two best cards for the trump named, chosen by the same rule
    /// the AI uses. Returns false, and changes nothing, unless the game is in the Hawsey
    /// exchange phase with the human as bidder and <paramref name="discard"/> is two
    /// cards the human holds.
    /// </summary>
    /// <param name="discard">The two cards the human discards.</param>
    /// <returns><see langword="true"/> when the exchange was made.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="discard"/> is <see langword="null"/>.</exception>
    public bool PerformHumanHawseyExchange(Card[] discard)
    {
        if (discard == null)
        {
            throw new ArgumentNullException(nameof(discard));
        }

        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.HawseyExchange, HawseyBidder: HumanPosition }
                || discard.Length != 2
                || !Holds(_state.Hands[HumanPosition], discard))
            {
                return false;
            }

            _aiStrategy.DecideHawseyExchange(_state, HumanPosition, out _, out var fromPartner);
            Volatile.Write(ref _state, _engine.ExchangeHawseyCards(_state, discard, fromPartner));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }



    /// <summary>
    /// Returns true if human needs to handle exchange.
    /// </summary>
    public async Task<bool> HandleHawseyExchangeAsync()
    {
        var generation = CurrentGeneration();
        PlayerPosition bidder;

        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.HawseyExchange, HawseyBidder: { } hawseyBidder })
            {
                return false;
            }

            if (hawseyBidder == HumanPosition)
            {
                return true;
            }

            bidder = hawseyBidder;
        }

        await PauseAsync(500).ConfigureAwait(false);

        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.HawseyExchange } || _state.HawseyBidder != bidder)
            {
                return false;
            }

            _aiStrategy.DecideHawseyExchange
            (
                _state,
                bidder,
                out var discard,
                out var fromPartner
            );

            Volatile.Write(ref _state, _engine.ExchangeHawseyCards(_state, discard, fromPartner));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return false;
    }



    /// <summary>
    /// Plays the human's card. Returns false, and changes nothing, when it is
    /// not the human's turn or the card is not a legal play. When the card
    /// completes a trick, a round or the game, the matching events are raised,
    /// exactly as for an AI play.
    /// </summary>
    public bool PlayHumanCard(Card card)
    {
        PlayOutcome outcome;

        lock (_sync)
        {
            // One legality check, inside the engine; an illegal or out-of-turn card is refused.
            if (_state is null || _engine.TryPlayCard(_state, HumanPosition, card) is not { } state)
            {
                return false;
            }

            Volatile.Write(ref _state, state);
            outcome = OutcomeOf(state);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);

        if (outcome.TrickCompleted != null)
        {
            TrickCompleted?.Invoke(this, outcome.TrickCompleted);
        }

        if (outcome.RoundCompleted)
        {
            RoundCompleted?.Invoke(this, EventArgs.Empty);
        }

        if (outcome.GameOver != null)
        {
            GameOver?.Invoke(this, outcome.GameOver);
        }

        return true;
    }



    /// <summary>
    /// Advances AI plays. Returns true if human needs to play.
    /// </summary>
    public async Task<bool> AdvanceAiPlaysAsync()
    {
        var generation = CurrentGeneration();
        var pausedAfterTrick = false;

        while (true)
        {
            var step = NextPlayStep(generation);

            // Stop: false; the human to play: true.
            if (step.Kind is PlayStepKind.Stop or PlayStepKind.HumanPlays)
            {
                return step.Kind == PlayStepKind.HumanPlays;
            }

            if (step.Kind == PlayStepKind.ClearWonTrick)
            {
                // Then look again: the human may have played, or a New Game started, meanwhile.
                await ClearWonTrickAsync(generation, step.State!, pausedAfterTrick).ConfigureAwait(false);
                continue;
            }

            var player = step.Player;

            await PauseAsync(400).ConfigureAwait(false);

            var outcome = PlayAiCard(generation, player);

            if (outcome == null)
            {
                return false;
            }

            StateChanged?.Invoke(this, EventArgs.Empty);

            if (outcome.TrickCompleted != null)
            {
                TrickCompleted?.Invoke(this, outcome.TrickCompleted);
                await PauseAsync(800).ConfigureAwait(false);
                pausedAfterTrick = true;

                // New Game during the pause: the finished game's round or game end
                // must not be announced into the new game.
                if (generation != CurrentGeneration())
                {
                    return false;
                }
            }

            if (outcome.RoundCompleted)
            {
                RoundCompleted?.Invoke(this, EventArgs.Empty);
                return false;
            }

            if (outcome.GameOver != null)
            {
                GameOver?.Invoke(this, outcome.GameOver);
                return false;
            }
        }
    }



    private enum PlayStepKind
    {
        /// <summary>The game moved on, or it is not trick play: the loop stops.</summary>
        Stop,

        /// <summary>The human is to play, with nothing to clear from the table.</summary>
        HumanPlays,

        /// <summary>The human won the trick on the table and leads next: clear it first.</summary>
        ClearWonTrick,

        /// <summary>An AI seat is to play.</summary>
        AiPlays,
    }



    private readonly record struct PlayStep(PlayStepKind Kind, PlayerPosition Player = default, GameState? State = null);



    /// <summary>What the AI play loop does next, read under the lock.</summary>
    private PlayStep NextPlayStep(int generation)
    {
        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.TrickPlay, NextToAct: { } next } state)
            {
                return new PlayStep(PlayStepKind.Stop);
            }

            if (next != HumanPosition)
            {
                return new PlayStep(PlayStepKind.AiPlays, next);
            }

            var wonTrickOnTable = state.CurrentTrick is not { Plays.Count: > 0 }
                && state.CompletedTricks.Count > 0
                && !ReferenceEquals(state, _clearedTable);

            return wonTrickOnTable
                ? new PlayStep(PlayStepKind.ClearWonTrick, next, state)
                : new PlayStep(PlayStepKind.HumanPlays);
        }
    }



    /// <summary>
    /// The human won the trick on the table and leads next: after the pause that lets the
    /// player see the trick (skipped when the AI loop has just paused after it), clear
    /// the table. Nothing is cleared if the game moved on during the pause.
    /// </summary>
    private async Task ClearWonTrickAsync(int generation, GameState wonTrick, bool alreadyPaused)
    {
        if (!alreadyPaused)
        {
            await PauseAsync(800).ConfigureAwait(false);
        }

        lock (_sync)
        {
            if (generation != _generation || !ReferenceEquals(_state, wonTrick))
            {
                return;
            }

            _clearedTable = wonTrick;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }



    private bool IsTableCleared(out GameState? state)
    {
        lock (_sync)
        {
            state = _state;
            return state != null && ReferenceEquals(state, _clearedTable);
        }
    }



    /// <summary>
    /// Deals the next round after a round has been scored. Does nothing in any
    /// other phase.
    /// </summary>
    public void StartNextRound()
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.RoundScoring })
            {
                return;
            }

            var state = _engine.StartNextRound(_state, _random);
            _bids.Clear();
            Volatile.Write(ref _state, state);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }



    /// <summary>
    /// Plays <paramref name="player"/>'s AI card under the lock. Returns null when
    /// the game moved on while the AI was "thinking" (new game, or no longer that
    /// player's turn); otherwise what the move completed, so the caller can raise
    /// the events outside the lock. Only the loop that made the transition reports
    /// it, so a round or game end is announced exactly once.
    /// </summary>
    private PlayOutcome? PlayAiCard(int generation, PlayerPosition player)
    {
        lock (_sync)
        {
            if (generation != _generation || _state is not { Phase: GamePhase.TrickPlay } || _state.NextToAct != player)
            {
                return null;
            }

            var card = _aiStrategy.DecidePlay(_state, player);
            var state = _engine.PlayCard(_state, player, card);
            Volatile.Write(ref _state, state);
            return OutcomeOf(state);
        }
    }



    /// <summary>
    /// What a card play completed, read from the state it produced: the trick
    /// winner when a trick just closed, whether the round ended, and the winning
    /// team when the game ended. Shared by the AI and human play paths so both
    /// raise the same events.
    /// </summary>
    private static PlayOutcome OutcomeOf(GameState state)
    {
        var trickCompleted = state.CompletedTricks.Count > 0 && (state.CurrentTrick == null || state.CurrentTrick.Plays.Count == 0)
            ? new TrickCompletedEventArgs(state.CompletedTricks[state.CompletedTricks.Count - 1].Winner)
            : null;
        // GameState decides the winner, including the bidding-team rule when both
        // teams reach the target in the same round (#862).
        var gameOver = state.Winner is { } winner ? new GameOverEventArgs(winner) : null;

        return new PlayOutcome(trickCompleted, state.Phase == GamePhase.RoundScoring, gameOver);
    }



    /// <summary>
    /// Pauses between a scored round and the next deal, so the player can read the
    /// result. Returns at once when pacing is off.
    /// </summary>
    /// <returns>A task that completes when the pause is over.</returns>
    public Task PauseBeforeNextRoundAsync() => PauseAsync(1500);



    // S2245: System.Random is fine here. It deals cards for a game, which is not
    // security-sensitive (no keys, tokens or secrets).
#pragma warning disable S2245
    private static Random NewRandom() => new();
#pragma warning restore S2245



    /// <summary>
    /// The pause before an AI move, so a person can follow the play. Skipped when
    /// pacing is off (tests).
    /// </summary>
    private Task PauseAsync(int milliseconds) =>
        _aiPacing ? Task.Delay(milliseconds) : Task.CompletedTask;



    /// <summary>
    /// Whether <paramref name="hand"/> holds every card in <paramref name="cards"/>,
    /// counting copies (a pinochle deck has two of each card).
    /// </summary>
    private static bool Holds(IReadOnlyList<Card> hand, Card[] cards)
    {
        var remaining = hand.ToList();

        foreach (var card in cards)
        {
            if (!remaining.Remove(card))
            {
                return false;
            }
        }

        return true;
    }



    private int CurrentGeneration()
    {
        lock (_sync)
        {
            return _generation;
        }
    }



    /// <summary>
    /// Whether bidding is still open in game <paramref name="generation"/>, and if so
    /// the state to act on. A Try-method rather than
    /// <c>[MemberNotNullWhen]</c>: that attribute is not in netstandard2.0, and a second
    /// PolySharp polyfill makes the engine build non-reproducible (see the engine's
    /// .csproj).
    /// </summary>
    private bool TryGetOpenBidding(int generation, out GameState state)
    {
        if (generation == _generation && _state is { Phase: GamePhase.Bidding } openState)
        {
            state = openState;
            return true;
        }

        state = null!;
        return false;
    }



    private sealed record PlayOutcome(TrickCompletedEventArgs? TrickCompleted, bool RoundCompleted, GameOverEventArgs? GameOver);
}
