using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Bidding;

public class BidActionTests
{
    private static BidAction.NumberBid? NoBid() => null;



    [Fact]
    public void NumberBid_Equals_when_amounts_match_returns_true()
    {
        var bid = new BidAction.NumberBid(8);
        object same = new BidAction.NumberBid(8);

        Assert.True(bid.Equals(new BidAction.NumberBid(8)));
        Assert.True(bid.Equals(same));
        Assert.Equal(new BidAction.NumberBid(8).GetHashCode(), bid.GetHashCode());
    }



    [Fact]
    public void NumberBid_Equals_when_amounts_differ_or_other_is_not_a_number_bid_returns_false()
    {
        var bid = new BidAction.NumberBid(8);

        Assert.False(bid.Equals(new BidAction.NumberBid(9)));
        Assert.False(bid.Equals(NoBid()));
        Assert.False(bid.Equals(BidAction.PassBid.Instance));
    }



    [Fact]
    public void PlacedBid_when_same_player_and_amount_are_equal()
    {
        Assert.Equal
        (
            new PlacedBid(PlayerPosition.North, new BidAction.NumberBid(8)),
            new PlacedBid(PlayerPosition.North, new BidAction.NumberBid(8))
        );
    }
}
