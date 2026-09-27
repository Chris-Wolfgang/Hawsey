using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// The bid rules a UI reads from the engine: <see cref="GameState.MinimumLegalBid"/>,
/// <see cref="GameState.IsNextBidderStuck"/>, and <see cref="GameSession.PlaceHumanBid"/>
/// refusing a bid the engine would reject (#867), instead of throwing.
/// </summary>
public class GameSessionBidRulesTests
{
    /// <summary>East (the first bidder, North dealing) bids <paramref name="amount"/>.</summary>
    private sealed class EastBids(int amount) : PassingAi
    {
        public override BidAction DecideBid(GameState state, PlayerPosition player) =>
            player == PlayerPosition.East ? new BidAction.NumberBid(amount) : BidAction.PassBid.Instance;
    }



    private static async Task<GameSession> AtHumanBidAfterEastBidsAsync(int amount)
    {
        var session = TestGameSessions.Unpaced(ai: new EastBids(amount));
        session.StartNewGame();
        Assert.True(await session.AdvanceAiBiddingAsync());
        return session;
    }



    [Fact]
    public void Before_any_bid_the_minimum_legal_bid_is_the_house_minimum()
    {
        var state = new GameEngine().StartGame(HouseRules.Default, PlayerPosition.North, new Random(1));

        Assert.Equal(HouseRules.Default.MinimumBid, state.MinimumLegalBid);
        Assert.False(state.IsNextBidderStuck);
    }



    [Fact]
    public async Task After_a_bid_the_minimum_legal_bid_is_one_more()
    {
        var session = await AtHumanBidAfterEastBidsAsync(8);

        Assert.Equal(8, session.CurrentState!.HighBid);
        Assert.Equal(9, session.CurrentState.MinimumLegalBid);
    }



    [Fact]
    public async Task PlaceHumanBid_refuses_a_bid_that_does_not_beat_the_high_bid()
    {
        var session = await AtHumanBidAfterEastBidsAsync(8);
        var before = session.CurrentState;

        Assert.False(session.PlaceHumanBid(new BidAction.NumberBid(8)));
        Assert.False(session.PlaceHumanBid(new BidAction.NumberBid(5)));
        Assert.False(session.PlaceHumanBid(new BidAction.NumberBid(BiddingPhase.MaximumBid + 1)));

        Assert.Same(before, session.CurrentState);
        Assert.True(session.PlaceHumanBid(new BidAction.NumberBid(BiddingPhase.MaximumBid)));
    }



    [Fact]
    public void The_dealer_facing_no_bid_is_stuck()
    {
        // North deals; East, South and West pass, so North (the dealer) is next and stuck.
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(1));
        var phase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        foreach (var bidder in new[] { PlayerPosition.East, PlayerPosition.South, PlayerPosition.West })
        {
            Assert.False(state.IsNextBidderStuck);
            state = engine.PlaceBid(state, bidder, BidAction.PassBid.Instance, phase);
        }

        Assert.Equal(PlayerPosition.North, state.NextToAct);
        Assert.True(state.IsNextBidderStuck);
    }



    [Fact]
    public void The_dealer_facing_a_bid_is_not_stuck()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(1));
        var phase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);
        state = engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(7), phase);
        state = engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, phase);
        state = engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, phase);

        Assert.Equal(PlayerPosition.North, state.NextToAct);
        Assert.False(state.IsNextBidderStuck);
    }
}
