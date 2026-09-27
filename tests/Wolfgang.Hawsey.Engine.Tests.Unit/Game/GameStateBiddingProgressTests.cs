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



    private (GameState State, BiddingPhase Phase) Deal()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        return (state, new BiddingPhase(state.Dealer, state.Rules.MinimumBid));
    }



    [Fact]
    public void A_new_deal_has_no_high_bid()
    {
        var (state, phase) = Deal();

        Assert.Equal(0, state.HighBid);
        Assert.Null(state.HighBidder);
        Assert.Equal(0, phase.HighestBid);
        Assert.Null(phase.HighestBidder);
    }



    [Fact]
    public void The_high_bid_follows_each_bid_and_survives_passes()
    {
        var (state, phase) = Deal();

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, phase);
        Assert.Equal((0, (PlayerPosition?)null), (state.HighBid, state.HighBidder));

        state = _engine.PlaceBid(state, PlayerPosition.South, new BidAction.NumberBid(7), phase);
        Assert.Equal((7, (PlayerPosition?)PlayerPosition.South), (state.HighBid, state.HighBidder));

        state = _engine.PlaceBid(state, PlayerPosition.West, new BidAction.NumberBid(9), phase);
        Assert.Equal((9, (PlayerPosition?)PlayerPosition.West), (state.HighBid, state.HighBidder));
        Assert.Equal((9, (PlayerPosition?)PlayerPosition.West), (phase.HighestBid, phase.HighestBidder));
    }



    [Fact]
    public void When_bidding_ends_the_result_holds_the_winning_bid()
    {
        var (state, phase) = Deal();
        state = _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(8), phase);
        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, phase);
        state = _engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, phase);

        state = _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, phase);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.Equal(0, state.HighBid);
        Assert.Null(state.HighBidder);
        Assert.Equal(8, state.BiddingResult!.BidAmount);
        Assert.Equal(PlayerPosition.East, state.BiddingResult.Winner);
    }



    [Fact]
    public void After_a_Hawsey_bid_the_bidding_phase_names_the_Hawsey_bidder()
    {
        var (state, phase) = Deal();
        state = _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(7), phase);

        _engine.PlaceBid(state, PlayerPosition.South, BidAction.HawseyBid.Instance, phase);

        Assert.Equal(PlayerPosition.South, phase.HighestBidder);
    }



    [Fact]
    public void The_next_deal_starts_with_no_high_bid()
    {
        var scored = RoundDriver.PlayOneRound(Deal().State, new TestPlayerStrategy());
        Assert.Equal(GamePhase.RoundScoring, scored.Phase);

        var next = _engine.StartNextRound(scored, new Random(7));

        Assert.Equal(0, next.HighBid);
        Assert.Null(next.HighBidder);
    }



    [FuzzProperty]
    public void The_state_always_reports_the_bidding_phases_high_bid(bool eastBids, bool southBids, bool westBids)
    {
        var (state, phase) = Deal();
        bool[] bids = [eastBids, southBids, westBids];

        for (var i = 0; i < bids.Length; i++)
        {
            var bidder = phase.GetNextBidder()!.Value;
            BidAction action = bids[i]
                ? new BidAction.NumberBid(Math.Max(state.Rules.MinimumBid, state.HighBid + 1))
                : BidAction.PassBid.Instance;

            state = _engine.PlaceBid(state, bidder, action, phase);

            Assert.Equal(GamePhase.Bidding, state.Phase);
            Assert.Equal(phase.HighestBid, state.HighBid);
            Assert.Equal(phase.HighestBidder, state.HighBidder);
        }
    }
}
