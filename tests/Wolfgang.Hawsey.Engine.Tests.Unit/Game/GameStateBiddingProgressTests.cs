using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;
using Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameState.HighBid"/> and <see cref="GameState.HighBidder"/>: the bidding
/// progress a UI shows ("current high bid: 8 by West") and a strategy needs to make a
/// legal bid (#863). North deals, so East, South, West and North bid in that order.
/// </summary>
public class GameStateBiddingProgressTests
{
    private readonly GameEngine _engine = new();



    private GameState Deal() => _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));



    [Fact]
    public void A_new_deal_has_no_high_bid()
    {
        var state = Deal();

        Assert.Equal(0, state.HighBid);
        Assert.Null(state.HighBidder);
    }



    [Fact]
    public void The_high_bid_follows_each_bid_and_survives_passes()
    {
        var state = Deal();

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance);
        Assert.Equal((0, (PlayerPosition?)null), (state.HighBid, state.HighBidder));

        state = _engine.PlaceBid(state, PlayerPosition.South, new BidAction.NumberBid(7));
        Assert.Equal((7, (PlayerPosition?)PlayerPosition.South), (state.HighBid, state.HighBidder));

        state = _engine.PlaceBid(state, PlayerPosition.West, new BidAction.NumberBid(9));
        Assert.Equal((9, (PlayerPosition?)PlayerPosition.West), (state.HighBid, state.HighBidder));
    }



    [Fact]
    public void When_bidding_ends_the_result_holds_the_winning_bid()
    {
        var state = Deal();
        state = _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(8));
        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance);
        state = _engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance);

        state = _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.Equal(0, state.HighBid);
        Assert.Null(state.HighBidder);
        Assert.Equal(8, state.BiddingResult!.BidAmount);
        Assert.Equal(PlayerPosition.East, state.BiddingResult.Winner);
    }



    [Fact]
    public void A_Hawsey_bid_ends_the_bidding_with_the_Hawsey_bidder_as_winner()
    {
        var state = Deal();
        state = _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(7));

        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.HawseyBid.Instance);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.Equal(PlayerPosition.South, state.BiddingResult!.Winner);
        Assert.True(state.BiddingResult.IsHawsey);
        Assert.Equal(PlayerPosition.South, state.HawseyBidder);
    }



    [Fact]
    public void The_next_deal_starts_with_no_high_bid()
    {
        var scored = RoundDriver.PlayOneRound(Deal(), new TestPlayerStrategy());
        Assert.Equal(GamePhase.RoundScoring, scored.Phase);

        var next = _engine.StartNextRound(scored, new Random(7));

        Assert.Equal(0, next.HighBid);
        Assert.Null(next.HighBidder);
    }



    [FuzzProperty]
    public void The_state_always_reports_the_last_number_bid_as_the_high_bid(bool eastBids, bool southBids, bool westBids)
    {
        var state = Deal();
        bool[] bids = [eastBids, southBids, westBids];
        var expectedBid = 0;
        PlayerPosition? expectedBidder = null;

        for (var i = 0; i < bids.Length; i++)
        {
            var bidder = state.NextToAct!.Value;
            BidAction action = bids[i]
                ? new BidAction.NumberBid(state.MinimumLegalBid)
                : BidAction.PassBid.Instance;
            (expectedBid, expectedBidder) = bids[i] ? (state.MinimumLegalBid, bidder) : (expectedBid, expectedBidder);

            state = _engine.PlaceBid(state, bidder, action);

            Assert.Equal(GamePhase.Bidding, state.Phase);
            Assert.Equal(expectedBid, state.HighBid);
            Assert.Equal(expectedBidder, state.HighBidder);
        }
    }
}
