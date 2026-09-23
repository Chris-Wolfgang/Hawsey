using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.UI.Maui.AI;

namespace Wolfgang.Hawsey.UI.Maui.Services;

/// <summary>
/// Wraps the game engine and manages the game lifecycle for the UI.
/// Human player is always South. AI controls North, East, and West.
/// </summary>
/// <remarks>
/// Thread safety: the AI loops resume on thread-pool threads after their delays
/// while human moves arrive on the UI thread, and the engine's
/// <see cref="GameEngine.PlayCard"/> mutates the trick shared with the incoming
/// state. So every transition of <c>_state</c> runs under <c>_sync</c>, and
/// each one re-checks, inside the lock, that it is still the right phase and
/// player's turn. A stale human move (a double-tap, a tap during an AI turn)
/// is ignored and reported as <c>false</c>. <c>_generation</c> increments on
/// every new game, so an AI loop that wakes up after New Game stops instead of
/// playing into the new game. Events are raised outside the lock.
/// </remarks>
public class GameService
{
    public const PlayerPosition HumanPosition = PlayerPosition.South;

    private readonly GameEngine _engine = new();
    private readonly SimpleAiStrategy _aiStrategy = new();
    private readonly object _sync = new();
    private GameState? _state;
    private BiddingPhase? _biddingPhase;
    private Random _random = new();
    private int _generation;



    public GameState? CurrentState => Volatile.Read(ref _state);



    public event EventHandler? StateChanged;



    public event EventHandler<TrickCompletedEventArgs>? TrickCompleted;



    public event EventHandler? RoundCompleted;



    public event EventHandler<GameOverEventArgs>? GameOver;



    public bool IsHumanTurn => CurrentState?.NextToAct == HumanPosition;



    public void StartNewGame(HouseRules? rules = null)
    {
        lock (_sync)
        {
            // S2245: System.Random is fine here — this seeds a card-dealer/PRNG for
            // gameplay, not anything security-sensitive (no keys, no tokens, no
            // secrets). Cryptographically strong RNG would add cost and dependency
            // for zero user-facing benefit in a bridge card game.
#pragma warning disable S2245
            _random = new Random();
#pragma warning restore S2245
            var state = _engine.StartGame(rules ?? HouseRules.Default, PlayerPosition.North, _random);
            _biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);
            _generation++;
            Volatile.Write(ref _state, state);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
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
                if (!IsBiddingOpen(generation))
                {
                    return false;
                }

                var next = _biddingPhase!.GetNextBidder();

                if (!next.HasValue)
                {
                    return false;
                }

                if (next.Value == HumanPosition)
                {
                    return true;
                }

                bidder = next.Value;
            }

            await Task.Delay(400).ConfigureAwait(false);

            lock (_sync)
            {
                // Another loop, a new game or a human move may have moved on while we slept.
                if (!IsBiddingOpen(generation) || _biddingPhase!.GetNextBidder() != bidder)
                {
                    return false;
                }

                var aiBid = _aiStrategy.DecideBid(_state!, bidder);
                Volatile.Write(ref _state, _engine.PlaceBid(_state!, bidder, aiBid, _biddingPhase));
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }



    /// <summary>
    /// Places the human's bid. Returns false, and changes nothing, when it is
    /// not the human's turn to bid.
    /// </summary>
    public bool PlaceHumanBid(BidAction action)
    {
        lock (_sync)
        {
            if (!IsBiddingOpen(_generation) || _biddingPhase!.GetNextBidder() != HumanPosition)
            {
                return false;
            }

            Volatile.Write(ref _state, _engine.PlaceBid(_state!, HumanPosition, action, _biddingPhase));
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

        await Task.Delay(500).ConfigureAwait(false);

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
    /// Applies a Hawsey exchange. Returns false, and changes nothing, outside
    /// the Hawsey exchange phase.
    /// </summary>
    public bool PerformHawseyExchange(Card[] discard, Card[] fromPartner)
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.HawseyExchange })
            {
                return false;
            }

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

        await Task.Delay(500).ConfigureAwait(false);

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
    /// not the human's turn or the card is not a legal play.
    /// </summary>
    public bool PlayHumanCard(Card card)
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.TrickPlay, NextToAct: HumanPosition } || !_state.GetLegalPlays().Contains(card))
            {
                return false;
            }

            Volatile.Write(ref _state, _engine.PlayCard(_state, HumanPosition, card));
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }



    /// <summary>
    /// Advances AI plays. Returns true if human needs to play.
    /// </summary>
    public async Task<bool> AdvanceAiPlaysAsync()
    {
        var generation = CurrentGeneration();

        while (true)
        {
            PlayerPosition player;

            lock (_sync)
            {
                if (generation != _generation || _state is not { Phase: GamePhase.TrickPlay, NextToAct: { } next })
                {
                    return false;
                }

                if (next == HumanPosition)
                {
                    return true;
                }

                player = next;
            }

            await Task.Delay(400).ConfigureAwait(false);

            var outcome = PlayAiCard(generation, player);

            if (outcome == null)
            {
                return false;
            }

            StateChanged?.Invoke(this, EventArgs.Empty);

            if (outcome.TrickCompleted != null)
            {
                TrickCompleted?.Invoke(this, outcome.TrickCompleted);
                await Task.Delay(800).ConfigureAwait(false);
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



    public void StartNextRound()
    {
        lock (_sync)
        {
            if (_state is not { Phase: GamePhase.RoundScoring })
            {
                return;
            }

            var state = _engine.StartNextRound(_state, _random);
            _biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);
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
    private AiPlayOutcome? PlayAiCard(int generation, PlayerPosition player)
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

            var trickCompleted = state.CompletedTricks.Count > 0 && (state.CurrentTrick == null || state.CurrentTrick.Plays.Count == 0)
                ? new TrickCompletedEventArgs(state.CompletedTricks[state.CompletedTricks.Count - 1].Winner)
                : null;
            var winner = state.NorthSouthScore >= state.Rules.PointsToWin ? Team.NorthSouth : Team.EastWest;
            var gameOver = state.Phase == GamePhase.GameOver ? new GameOverEventArgs(winner) : null;

            return new AiPlayOutcome(trickCompleted, state.Phase == GamePhase.RoundScoring, gameOver);
        }
    }



    private int CurrentGeneration()
    {
        lock (_sync)
        {
            return _generation;
        }
    }



    private bool IsBiddingOpen(int generation) =>
        generation == _generation
        && _state is { Phase: GamePhase.Bidding }
        && _biddingPhase is { IsComplete: false };



    private sealed record AiPlayOutcome(TrickCompletedEventArgs? TrickCompleted, bool RoundCompleted, GameOverEventArgs? GameOver);
}
