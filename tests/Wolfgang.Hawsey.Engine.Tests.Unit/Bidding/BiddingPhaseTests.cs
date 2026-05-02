using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Bidding;

public class BiddingPhaseTests
{
    [Fact]
    public void GetNextBidder_starts_left_of_dealer()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        Assert.Equal(PlayerPosition.East, phase.GetNextBidder());
    }



    [Fact]
    public void GetNextBidder_progresses_clockwise()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, BidAction.PassBid.Instance);
        Assert.Equal(PlayerPosition.South, phase.GetNextBidder());

        phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance);
        Assert.Equal(PlayerPosition.West, phase.GetNextBidder());

        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        Assert.Equal(PlayerPosition.North, phase.GetNextBidder());
    }



    [Fact]
    public void PlaceBid_when_all_pass_dealer_is_stuck()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.North, BidAction.PassBid.Instance);

        var result = phase.GetResult();

        Assert.Equal(PlayerPosition.North, result.Winner);
        Assert.Equal(6, result.BidAmount);
        Assert.True(result.IsStuck);
        Assert.False(result.IsHawsey);
    }



    [Fact]
    public void PlaceBid_when_highest_bidder_wins()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(6));
        phase.PlaceBid(PlayerPosition.South, new BidAction.NumberBid(7));
        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.North, BidAction.PassBid.Instance);

        var result = phase.GetResult();

        Assert.Equal(PlayerPosition.South, result.Winner);
        Assert.Equal(7, result.BidAmount);
        Assert.False(result.IsStuck);
        Assert.False(result.IsHawsey);
    }



    [Fact]
    public void PlaceBid_when_hawsey_stops_bidding_immediately()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, BidAction.HawseyBid.Instance);

        Assert.True(phase.IsComplete);
        Assert.Null(phase.GetNextBidder());

        var result = phase.GetResult();

        Assert.Equal(PlayerPosition.East, result.Winner);
        Assert.Equal(24, result.BidAmount);
        Assert.True(result.IsHawsey);
        Assert.False(result.IsStuck);
    }



    [Fact]
    public void PlaceBid_when_hawsey_after_bids_stops_immediately()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(6));
        phase.PlaceBid(PlayerPosition.South, BidAction.HawseyBid.Instance);

        Assert.True(phase.IsComplete);

        var result = phase.GetResult();

        Assert.Equal(PlayerPosition.South, result.Winner);
        Assert.True(result.IsHawsey);
    }



    [Fact]
    public void PlaceBid_when_bid_below_minimum_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        Assert.Throws<InvalidOperationException>
        (
            () => phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(5))
        );
    }



    [Fact]
    public void PlaceBid_when_bid_does_not_beat_previous_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(7));

        Assert.Throws<InvalidOperationException>
        (
            () => phase.PlaceBid(PlayerPosition.South, new BidAction.NumberBid(7))
        );
    }



    [Fact]
    public void PlaceBid_when_wrong_player_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        Assert.Throws<InvalidOperationException>
        (
            () => phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance)
        );
    }



    [Fact]
    public void PlaceBid_when_bidding_complete_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, BidAction.HawseyBid.Instance);

        Assert.Throws<InvalidOperationException>
        (
            () => phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance)
        );
    }



    [Fact]
    public void PlaceBid_when_null_action_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        Assert.Throws<ArgumentNullException>
        (
            () => phase.PlaceBid(PlayerPosition.East, null!)
        );
    }



    [Fact]
    public void GetResult_when_bidding_not_complete_throws()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        Assert.Throws<InvalidOperationException>(() => phase.GetResult());
    }



    [Fact]
    public void PlaceBid_when_dealer_bids_last_and_wins()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(6));
        phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.North, new BidAction.NumberBid(7));

        var result = phase.GetResult();

        Assert.Equal(PlayerPosition.North, result.Winner);
        Assert.Equal(7, result.BidAmount);
    }



    [Fact]
    public void PlaceBid_when_custom_minimum_bid_respected()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 8);

        Assert.Throws<InvalidOperationException>
        (
            () => phase.PlaceBid(PlayerPosition.East, new BidAction.NumberBid(7))
        );
    }



    [Fact]
    public void PlaceBid_when_all_pass_stuck_at_custom_minimum()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 8);

        phase.PlaceBid(PlayerPosition.East, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.North, BidAction.PassBid.Instance);

        var result = phase.GetResult();

        Assert.Equal(8, result.BidAmount);
        Assert.True(result.IsStuck);
    }



    [Fact]
    public void GetNextBidder_when_complete_returns_null()
    {
        var phase = new BiddingPhase(PlayerPosition.North, 6);

        phase.PlaceBid(PlayerPosition.East, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.South, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.West, BidAction.PassBid.Instance);
        phase.PlaceBid(PlayerPosition.North, BidAction.PassBid.Instance);

        Assert.Null(phase.GetNextBidder());
    }
}
