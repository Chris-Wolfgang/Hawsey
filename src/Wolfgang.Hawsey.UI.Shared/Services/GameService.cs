using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.UI.Shared.AI;

namespace Wolfgang.Hawsey.UI.Shared.Services;

/// <summary>
/// Wraps the game engine and manages the game lifecycle for the UI.
/// Human player is always South. AI controls North, East, and West.
/// </summary>
public class GameService
{
    public const PlayerPosition HumanPosition = PlayerPosition.South;

    private readonly GameEngine _engine = new();
    private readonly SimpleAiStrategy _aiStrategy = new();
    private readonly List<(PlayerPosition Player, BidAction Action)> _bidHistory = new();
    private readonly List<string> _aiLog = new();
    private GameState? _state;
    private BiddingPhase? _biddingPhase;
    private Random _random = new();
    private int _highBidAmount;



    public GameState? CurrentState => _state;

    public IReadOnlyList<(PlayerPosition Player, BidAction Action)> BidHistory => _bidHistory;

    public IReadOnlyList<string> AiLog => _aiLog;

    public int HighBidAmount => _highBidAmount;

    public int MinimumLegalBid =>
        Math.Max(_state?.Rules.MinimumBid ?? 6, _highBidAmount + 1);

    public bool IsHumanTurn => _state?.NextToAct == HumanPosition;



    public event Action? StateChanged;
    public event Action<PlayerPosition>? TrickCompleted;
    public event Action? RoundCompleted;
    public event Action<Team>? GameOver;



    /// <summary>
    /// When a trick has just been completed, this holds the trick so the UI can render it
    /// during the post-trick delay. Cleared once the delay elapses.
    /// </summary>
    public TrickResult? RecentlyCompletedTrick { get; private set; }

    /// <summary>
    /// How long to keep the just-completed trick visible before clearing.
    /// </summary>
    public TimeSpan TrickCompletionDisplay { get; set; } = TimeSpan.FromMilliseconds(1500);



    public void StartNewGame(HouseRules? rules = null)
    {
        _random = new Random();
        // Default: must-beat is enforced (you must play a higher card if you can follow suit,
        // even when your partner is winning).
        _state = _engine.StartGame(rules ?? new HouseRules { MustBeat = true }, HumanPosition, _random);
        _biddingPhase = new BiddingPhase(_state.Dealer, _state.Rules.MinimumBid);
        _bidHistory.Clear();
        _highBidAmount = 0;
        _aiLog.Clear();
        LogAi($"New game. Dealer: {_state.Dealer}. Must-beat: {_state.Rules.MustBeat}.");
        StateChanged?.Invoke();
    }



    public async Task<bool> AdvanceAiBiddingAsync()
    {
        if (_state == null || _biddingPhase == null) return false;

        while (_state.Phase == GamePhase.Bidding && !_biddingPhase.IsComplete)
        {
            var nextBidder = _biddingPhase.GetNextBidder();
            if (!nextBidder.HasValue) break;
            if (nextBidder.Value == HumanPosition) return true;

            await Task.Delay(400).ConfigureAwait(false);

            var aiBid = _aiStrategy.DecideBid(_state, nextBidder.Value);
            // Strategy doesn't see the running high; downgrade to Pass if number wouldn't beat it.
            if (aiBid is BidAction.NumberBid n && n.Amount <= _highBidAmount)
            {
                LogAi($"{nextBidder.Value} ideal bid {n.Amount} not above current high {_highBidAmount} — passing.");
                aiBid = BidAction.PassBid.Instance;
            }

            // Stuck-dealer cap: if everyone else passed and this is the dealer, no need
            // to overbid empty competition — bid the minimum.
            var isStuckDealer =
                nextBidder.Value == _state.Dealer
                && _bidHistory.Count == 3
                && _highBidAmount == 0
                && aiBid is BidAction.NumberBid;
            if (isStuckDealer && aiBid is BidAction.NumberBid stuckBid
                && stuckBid.Amount > _state.Rules.MinimumBid)
            {
                LogAi($"{nextBidder.Value} is stuck — capping bid at minimum {_state.Rules.MinimumBid} (would have bid {stuckBid.Amount}).");
                aiBid = new BidAction.NumberBid(_state.Rules.MinimumBid);
            }
            _state = _engine.PlaceBid(_state, nextBidder.Value, aiBid, _biddingPhase);
            RecordBid(nextBidder.Value, aiBid);
            StateChanged?.Invoke();
        }

        return false;
    }



    public void PlaceHumanBid(BidAction action)
    {
        if (_state == null || _biddingPhase == null) return;
        _state = _engine.PlaceBid(_state, HumanPosition, action, _biddingPhase);
        RecordBid(HumanPosition, action);
        StateChanged?.Invoke();
    }



    public void SelectTrump(Suit? trumpSuit)
    {
        if (_state == null) return;
        _state = _engine.SelectTrump(_state, trumpSuit);
        LogAi($"Trump selected: {(trumpSuit?.ToString() ?? "Ace High")}.");
        StateChanged?.Invoke();
    }



    public async Task<bool> HandleTrumpSelectionAsync()
    {
        if (_state == null || _state.Phase != GamePhase.TrumpSelection) return false;
        if (_state.NextToAct == HumanPosition) return true;

        await Task.Delay(500).ConfigureAwait(false);
        var trump = _aiStrategy.DecideTrump(_state, _state.NextToAct!.Value);
        SelectTrump(trump);
        return false;
    }



    public void PerformHawseyExchange(Card[] discard, Card[] fromPartner)
    {
        if (_state == null) return;
        _state = _engine.ExchangeHawseyCards(_state, discard, fromPartner);
        StateChanged?.Invoke();
    }



    public async Task<bool> HandleHawseyExchangeAsync()
    {
        if (_state == null || _state.Phase != GamePhase.HawseyExchange) return false;
        if (_state.HawseyBidder == HumanPosition) return true;

        await Task.Delay(500).ConfigureAwait(false);

        _aiStrategy.DecideHawseyExchange(
            _state,
            _state.HawseyBidder!.Value,
            out var discard,
            out var fromPartner);

        PerformHawseyExchange(discard, fromPartner);
        return false;
    }



    /// <summary>
    /// Stand-in for the future human Hawsey-exchange UI: invokes the AI strategy
    /// on the human's behalf so the round can proceed. Discards the bidder's 2
    /// lowest cards and takes partner's 2 highest.
    /// </summary>
    public void AutoResolveHumanHawseyExchange()
    {
        if (_state == null || _state.Phase != GamePhase.HawseyExchange) return;
        if (_state.HawseyBidder != HumanPosition) return;

        _aiStrategy.DecideHawseyExchange(
            _state,
            HumanPosition,
            out var discard,
            out var fromPartner);

        LogAi($"Human Hawsey exchange auto-resolved (discard {discard[0]}, {discard[1]}; take {fromPartner[0]}, {fromPartner[1]}).");
        PerformHawseyExchange(discard, fromPartner);
    }



    public async Task PlayHumanCardAsync(Card card)
    {
        if (_state == null) return;
        await PlayCardWithCompletionDisplayAsync(HumanPosition, card).ConfigureAwait(false);
    }



    public async Task<bool> AdvanceAiPlaysAsync()
    {
        if (_state == null) return false;

        while (_state.Phase == GamePhase.TrickPlay)
        {
            if (_state.NextToAct == HumanPosition) return true;

            await Task.Delay(400).ConfigureAwait(false);

            var player = _state.NextToAct!.Value;
            var card = _aiStrategy.DecidePlay(_state, player);
            await PlayCardWithCompletionDisplayAsync(player, card).ConfigureAwait(false);
        }

        if (_state.Phase == GamePhase.RoundScoring)
        {
            LogAi($"--- Round complete. Cumulative: US {_state.NorthSouthScore}, THEM {_state.EastWestScore}.");
            RoundCompleted?.Invoke();
            return false;
        }

        if (_state.Phase == GamePhase.GameOver)
        {
            var winner = _state.NorthSouthScore >= _state.Rules.PointsToWin
                ? Team.NorthSouth
                : Team.EastWest;
            LogAi($"=== GAME OVER. Winner: {(winner == Team.NorthSouth ? "US" : "THEM")}. Final: US {_state.NorthSouthScore}, THEM {_state.EastWestScore}.");
            GameOver?.Invoke(winner);
            return false;
        }

        return false;
    }



    public void StartNextRound()
    {
        if (_state == null) return;
        _state = _engine.StartNextRound(_state, _random);
        _biddingPhase = new BiddingPhase(_state.Dealer, _state.Rules.MinimumBid);
        _bidHistory.Clear();
        _highBidAmount = 0;
        LogAi($"--- New round. Dealer: {_state.Dealer}.");
        StateChanged?.Invoke();
    }



    private async Task PlayCardWithCompletionDisplayAsync(PlayerPosition player, Card card)
    {
        var trickCountBefore = _state!.CompletedTricks.Count;
        var trickNumber = trickCountBefore + 1;
        var playSlot = (_state.CurrentTrick?.Plays.Count ?? 0) + 1;

        LogAi($"Trick {trickNumber} play {playSlot}: {player} plays {FormatCard(card)}.");

        _state = _engine.PlayCard(_state, player, card);
        var trickJustCompleted = _state.CompletedTricks.Count > trickCountBefore;

        if (trickJustCompleted)
        {
            RecentlyCompletedTrick = _state.CompletedTricks[_state.CompletedTricks.Count - 1];
            var winningCard = WinningCardOf(RecentlyCompletedTrick);
            var usTricks = CountTricksByTeam(_state.CompletedTricks, Team.NorthSouth);
            var themTricks = _state.CompletedTricks.Count - usTricks;
            LogAi($"  → Trick {trickNumber} won by {RecentlyCompletedTrick.Winner} ({FormatCard(winningCard)}). Tricks: US {usTricks}, THEM {themTricks}.");
            StateChanged?.Invoke();
            TrickCompleted?.Invoke(RecentlyCompletedTrick.Winner);
            await Task.Delay(TrickCompletionDisplay).ConfigureAwait(false);
            RecentlyCompletedTrick = null;
            StateChanged?.Invoke();
        }
        else
        {
            StateChanged?.Invoke();
        }
    }



    private static Card WinningCardOf(TrickResult trick)
    {
        for (var i = 0; i < trick.Cards.Count; i++)
        {
            if (trick.Cards[i].Player == trick.Winner) return trick.Cards[i].Card;
        }
        return trick.Cards[0].Card;
    }



    private static int CountTricksByTeam(IReadOnlyList<TrickResult> tricks, Team team)
    {
        var n = 0;
        for (var i = 0; i < tricks.Count; i++)
        {
            var w = tricks[i].Winner;
            var isUs = w is PlayerPosition.North or PlayerPosition.South;
            if ((isUs && team == Team.NorthSouth) || (!isUs && team == Team.EastWest)) n++;
        }
        return n;
    }



    private static string FormatCard(Card c)
    {
        var rank = c.Rank switch
        {
            Rank.Nine => "9",
            Rank.Ten => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => "?"
        };
        var suit = c.Suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };
        return $"{rank}{suit}";
    }



    private void RecordBid(PlayerPosition player, BidAction action)
    {
        _bidHistory.Add((player, action));
        if (action is BidAction.NumberBid n)
        {
            _highBidAmount = n.Amount;
        }
        var label = action switch
        {
            BidAction.PassBid => "PASS",
            BidAction.NumberBid nb => $"Bid {nb.Amount}",
            BidAction.HawseyBid => "Hawsey",
            _ => action.GetType().Name
        };
        LogAi($"{player} {label}.");
    }



    private void LogAi(string message)
    {
        _aiLog.Insert(0, $"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
        if (_aiLog.Count > 200) _aiLog.RemoveAt(_aiLog.Count - 1);
    }
}
