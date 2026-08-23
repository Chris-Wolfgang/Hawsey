namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Convenience wrapper that runs a full Hawsey game to completion
/// using <see cref="IPlayerStrategy"/> for player decisions.
/// </summary>
public sealed class GameRunner
{
    private readonly GameEngine _engine;



    /// <summary>
    /// Initializes a new instance of the <see cref="GameRunner"/> class.
    /// </summary>
    public GameRunner()
    {
        _engine = new GameEngine();
    }



    /// <summary>
    /// Runs a complete game of Hawsey from start to finish.
    /// </summary>
    /// <param name="strategy">The strategy to use for all player decisions.</param>
    /// <param name="rules">The house rules.</param>
    /// <param name="firstDealer">The first dealer's position.</param>
    /// <param name="random">The random number generator.</param>
    /// <returns>The final game state after a team has won.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/>, <paramref name="rules"/>, or <paramref name="random"/> is <c>null</c>.</exception>
    public GameState RunGame
    (
        IPlayerStrategy strategy,
        HouseRules rules,
        PlayerPosition firstDealer,
        Random random
    )
    {
        if (strategy == null)
        {
            throw new ArgumentNullException(nameof(strategy));
        }

        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var state = _engine.StartGame(rules, firstDealer, random);

        while (state.Phase != GamePhase.GameOver)
        {
            state = AdvanceState(state, strategy, random);
        }

        return state;
    }



    private GameState AdvanceState(GameState state, IPlayerStrategy strategy, Random random)
    {
        switch (state.Phase)
        {
            case GamePhase.Bidding:
                return RunBidding(state, strategy);

            case GamePhase.TrumpSelection:
                var trump = strategy.DecideTrump(state, state.NextToAct!.Value);
                return _engine.SelectTrump(state, trump);

            case GamePhase.HawseyExchange:
                strategy.DecideHawseyExchange
                (
                    state,
                    state.HawseyBidder!.Value,
                    out var discard,
                    out var fromPartner
                );
                return _engine.ExchangeHawseyCards(state, discard, fromPartner);

            case GamePhase.TrickPlay:
                var card = strategy.DecidePlay(state, state.NextToAct!.Value);
                // Second `.Value` doesn't need `!` — flow analysis carries the
                // non-null proof from the preceding dereference on this arm.
                return _engine.PlayCard(state, state.NextToAct.Value, card);

            case GamePhase.RoundScoring:
                return _engine.StartNextRound(state, random);

            default:
                throw new InvalidOperationException($"Unexpected game phase: {state.Phase}");
        }
    }



    private GameState RunBidding(GameState state, IPlayerStrategy strategy)
    {
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (!biddingPhase.IsComplete)
        {
            var bidder = biddingPhase.GetNextBidder()!.Value;
            var action = strategy.DecideBid(state, bidder);
            state = _engine.PlaceBid(state, bidder, action, biddingPhase);
        }

        return state;
    }
}
