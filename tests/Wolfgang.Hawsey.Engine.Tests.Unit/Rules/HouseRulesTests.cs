using Wolfgang.Hawsey.Engine;

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
}
