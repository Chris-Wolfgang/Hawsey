using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.UI.Maui.AI;

namespace Wolfgang.Hawsey.UI.Maui.Services;

/// <summary>
/// Wraps the game engine and manages the game lifecycle for the UI.
/// Human player is always South. AI controls North, East, and West.
/// </summary>
public class GameService
{
    public const PlayerPosition HumanPosition = PlayerPosition.South;

    private readonly GameEngine _engine = new();
    private readonly SimpleAiStrategy _aiStrategy = new();
    private GameState? _state;
    private BiddingPhase? _biddingPhase;
    private Random _random = new();



    public GameState? CurrentState => _state;



    public event Action? StateChanged;



    public event Action<PlayerPosition>? TrickCompleted;



    public event Action? RoundCompleted;



    public event Action<Team>? GameOver;



    public bool IsHumanTurn => _state?.NextToAct == HumanPosition;



    public void StartNewGame(HouseRules? rules = null)
    {
        _random = new Random();
        _state = _engine.StartGame(rules ?? HouseRules.Default, PlayerPosition.North, _random);
        _biddingPhase = new BiddingPhase(_state.Dealer, _state.Rules.MinimumBid);
        StateChanged?.Invoke();
    }



    /// <summary>
    /// Advances AI bidding. Returns true if human needs to bid.
    /// </summary>
    public async Task<bool> AdvanceAiBiddingAsync()
    {
        if (_state == null || _biddingPhase == null)
        {
            return false;
        }

        while (_state.Phase == GamePhase.Bidding && !_biddingPhase.IsComplete)
        {
            var nextBidder = _biddingPhase.GetNextBidder();

            if (!nextBidder.HasValue)
            {
                break;
            }

            if (nextBidder.Value == HumanPosition)
            {
                return true;
            }

            await Task.Delay(400).ConfigureAwait(false);

            var aiBid = _aiStrategy.DecideBid(_state, nextBidder.Value);
            _state = _engine.PlaceBid(_state, nextBidder.Value, aiBid, _biddingPhase);
            StateChanged?.Invoke();
        }

        return false;
    }



    public void PlaceHumanBid(BidAction action)
    {
        if (_state == null || _biddingPhase == null)
        {
            return;
        }

        _state = _engine.PlaceBid(_state, HumanPosition, action, _biddingPhase);
        StateChanged?.Invoke();
    }



    public void SelectTrump(Suit? trumpSuit)
    {
        if (_state == null)
        {
            return;
        }

        _state = _engine.SelectTrump(_state, trumpSuit);
        StateChanged?.Invoke();
    }



    /// <summary>
    /// Returns true if human needs to select trump.
    /// </summary>
    public async Task<bool> HandleTrumpSelectionAsync()
    {
        if (_state == null || _state.Phase != GamePhase.TrumpSelection)
        {
            return false;
        }

        if (_state.NextToAct == HumanPosition)
        {
            return true;
        }

        await Task.Delay(500).ConfigureAwait(false);

        var trump = _aiStrategy.DecideTrump(_state, _state.NextToAct!.Value);
        SelectTrump(trump);

        return false;
    }



    public void PerformHawseyExchange(Card[] discard, Card[] fromPartner)
    {
        if (_state == null)
        {
            return;
        }

        _state = _engine.ExchangeHawseyCards(_state, discard, fromPartner);
        StateChanged?.Invoke();
    }



    /// <summary>
    /// Returns true if human needs to handle exchange.
    /// </summary>
    public async Task<bool> HandleHawseyExchangeAsync()
    {
        if (_state == null || _state.Phase != GamePhase.HawseyExchange)
        {
            return false;
        }

        if (_state.HawseyBidder == HumanPosition)
        {
            return true;
        }

        await Task.Delay(500).ConfigureAwait(false);

        _aiStrategy.DecideHawseyExchange(
            _state,
            _state.HawseyBidder!.Value,
            out var discard,
            out var fromPartner);

        PerformHawseyExchange(discard, fromPartner);

        return false;
    }



    public void PlayHumanCard(Card card)
    {
        if (_state == null)
        {
            return;
        }

        _state = _engine.PlayCard(_state, HumanPosition, card);
        StateChanged?.Invoke();
    }



    /// <summary>
    /// Advances AI plays. Returns true if human needs to play.
    /// </summary>
    public async Task<bool> AdvanceAiPlaysAsync()
    {
        if (_state == null)
        {
            return false;
        }

        while (_state.Phase == GamePhase.TrickPlay)
        {
            if (_state.NextToAct == HumanPosition)
            {
                return true;
            }

            await Task.Delay(400).ConfigureAwait(false);

            var player = _state.NextToAct!.Value;
            var card = _aiStrategy.DecidePlay(_state, player);
            _state = _engine.PlayCard(_state, player, card);
            StateChanged?.Invoke();

            // Check if trick just completed
            if (_state.CurrentTrick != null && _state.CurrentTrick.Plays.Count == 0 &&
                _state.CompletedTricks.Count > 0)
            {
                var lastTrick = _state.CompletedTricks[_state.CompletedTricks.Count - 1];
                TrickCompleted?.Invoke(lastTrick.Winner);
                await Task.Delay(800).ConfigureAwait(false);
            }
        }

        if (_state.Phase == GamePhase.RoundScoring)
        {
            RoundCompleted?.Invoke();
            return false;
        }

        if (_state.Phase == GamePhase.GameOver)
        {
            var winner = _state.NorthSouthScore >= _state.Rules.PointsToWin
                ? Team.NorthSouth
                : Team.EastWest;
            GameOver?.Invoke(winner);
            return false;
        }

        return false;
    }



    public void StartNextRound()
    {
        if (_state == null)
        {
            return;
        }

        _state = _engine.StartNextRound(_state, _random);
        _biddingPhase = new BiddingPhase(_state.Dealer, _state.Rules.MinimumBid);
        StateChanged?.Invoke();
    }
}
