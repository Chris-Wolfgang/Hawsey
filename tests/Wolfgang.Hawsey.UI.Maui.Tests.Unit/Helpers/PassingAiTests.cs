using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;

/// <summary>
/// The view-model tests lean on <see cref="PassingAi"/> to keep the bidding in the
/// human's hands, so pin that it only overrides the bid.
/// </summary>
public class PassingAiTests
{
    [Fact]
    public void PassingAi_passes_every_bid_and_otherwise_decides_like_the_real_AI()
    {
        var state = new GameEngine().StartGame(HouseRules.Default, PlayerPosition.North, new Random(3));
        var real = new SimpleAiStrategy();
        var passing = new PassingAi();

        Assert.IsType<BidAction.PassBid>(passing.DecideBid(state, PlayerPosition.East));
        Assert.Equal(real.DecideTrump(state, PlayerPosition.East), passing.DecideTrump(state, PlayerPosition.East));

        real.DecideHawseyExchange(state, PlayerPosition.East, out var realDiscard, out var realFromPartner);
        passing.DecideHawseyExchange(state, PlayerPosition.East, out var discard, out var fromPartner);

        Assert.Equal(realDiscard, discard);
        Assert.Equal(realFromPartner, fromPartner);
    }
}
