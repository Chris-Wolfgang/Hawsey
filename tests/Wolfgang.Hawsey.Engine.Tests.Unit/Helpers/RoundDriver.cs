using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

/// <summary>
/// Plays one round through <see cref="GameEngine"/> the way <see cref="GameRunner"/>
/// does, but stops when the round is scored, so a test can inspect the scored state
/// before <see cref="GameEngine.StartNextRound"/> resets it.
/// </summary>
internal static class RoundDriver
{
    public static GameState PlayOneRound(GameState state, IPlayerStrategy strategy)
    {
        var engine = new GameEngine();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (state.Phase is not (GamePhase.RoundScoring or GamePhase.GameOver))
        {
            if (state.Phase == GamePhase.Bidding)
            {
                var bidder = biddingPhase.GetNextBidder()!.Value;
                state = engine.PlaceBid(state, bidder, strategy.DecideBid(state, bidder), biddingPhase);
            }
            else if (state.Phase == GamePhase.TrumpSelection)
            {
                state = engine.SelectTrump(state, strategy.DecideTrump(state, state.NextToAct!.Value));
            }
            else if (state.Phase == GamePhase.HawseyExchange)
            {
                strategy.DecideHawseyExchange(state, state.HawseyBidder!.Value, out var discard, out var fromPartner);
                state = engine.ExchangeHawseyCards(state, discard, fromPartner);
            }
            else
            {
                var player = state.NextToAct!.Value;
                state = engine.PlayCard(state, player, strategy.DecidePlay(state, player));
            }
        }

        return state;
    }
}
