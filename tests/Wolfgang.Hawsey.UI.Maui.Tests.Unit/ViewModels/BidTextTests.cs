using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

public class BidTextTests
{
    [Fact]
    public void Hawsey_when_read_carries_the_engines_Hawsey_score()
    {
        Assert.Equal($"Hawsey ({GameState.HawseyBidAmount})", BidText.Hawsey);
    }



    [Theory]
    [InlineData(false, 7, "7")]
    [InlineData(true, GameState.HawseyBidAmount, "Hawsey (24)")]
    public void For_when_given_a_bid_writes_it(bool isHawsey, int amount, string expected)
    {
        Assert.Equal(expected, BidText.For(isHawsey, amount));
    }
}
