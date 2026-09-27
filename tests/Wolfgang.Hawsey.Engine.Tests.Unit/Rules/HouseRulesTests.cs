
using Wolfgang.Hawsey.Engine.Rules;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Rules;

public class HouseRulesTests
{
    [Fact]
    public void Default_has_expected_values()
    {
        var rules = HouseRules.Default;

        Assert.False(rules.MustBeat);
        Assert.False(rules.MustTrump);
        Assert.Equal(6, rules.MinimumBid);
        Assert.Equal(62, rules.PointsToWin);
    }



    [Fact]
    public void Init_properties_can_be_overridden()
    {
        var rules = new HouseRules
        {
            MustBeat = true,
            MustTrump = true,
            MinimumBid = 7,
            PointsToWin = 100
        };

        Assert.True(rules.MustBeat);
        Assert.True(rules.MustTrump);
        Assert.Equal(7, rules.MinimumBid);
        Assert.Equal(100, rules.PointsToWin);
    }



    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public void MinimumBid_when_out_of_range_throws(int minimumBid)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new HouseRules { MinimumBid = minimumBid });

        Assert.Equal(nameof(HouseRules.MinimumBid), ex.ParamName);
    }



    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    public void MinimumBid_at_a_bound_is_accepted(int minimumBid)
    {
        Assert.Equal(minimumBid, new HouseRules { MinimumBid = minimumBid }.MinimumBid);
    }



    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void PointsToWin_when_below_one_throws(int pointsToWin)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new HouseRules { PointsToWin = pointsToWin });

        Assert.Equal(nameof(HouseRules.PointsToWin), ex.ParamName);
    }



    [Fact]
    public void PointsToWin_of_one_is_accepted()
    {
        Assert.Equal(1, new HouseRules { PointsToWin = 1 }.PointsToWin);
    }
}
