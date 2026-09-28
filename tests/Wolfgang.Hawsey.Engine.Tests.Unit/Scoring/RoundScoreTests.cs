
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Scoring;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Scoring;

public class RoundScoreTests
{
    [Fact]
    public void BiddingTeamDelta_when_bid_made_returns_tricks_won()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 7, biddingTeamTricks: 8, defendingTeamTricks: 4, isHawsey: false);

        Assert.Equal(8, score.BiddingTeamDelta);
    }



    [Fact]
    public void BiddingTeamDelta_when_bid_exactly_met_returns_tricks_won()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 7, biddingTeamTricks: 7, defendingTeamTricks: 5, isHawsey: false);

        Assert.Equal(7, score.BiddingTeamDelta);
    }



    [Fact]
    public void BiddingTeamDelta_when_bid_missed_returns_negative_bid()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 7, biddingTeamTricks: 6, defendingTeamTricks: 6, isHawsey: false);

        Assert.Equal(-7, score.BiddingTeamDelta);
    }



    [Fact]
    public void BiddingTeamDelta_when_hawsey_made_returns_24()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 24, biddingTeamTricks: 12, defendingTeamTricks: 0, isHawsey: true);

        Assert.Equal(24, score.BiddingTeamDelta);
    }



    [Fact]
    public void BiddingTeamDelta_when_hawsey_missed_returns_negative_24()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 24, biddingTeamTricks: 11, defendingTeamTricks: 1, isHawsey: true);

        Assert.Equal(-24, score.BiddingTeamDelta);
    }



    [Fact]
    public void DefendingTeamDelta_always_returns_tricks_won()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 7, biddingTeamTricks: 6, defendingTeamTricks: 6, isHawsey: false);

        Assert.Equal(6, score.DefendingTeamDelta);
    }



    [Fact]
    public void DefendingTeamDelta_when_hawsey_made_returns_zero()
    {
        var score = new RoundScore(Team.NorthSouth, bidAmount: 24, biddingTeamTricks: 12, defendingTeamTricks: 0, isHawsey: true);

        Assert.Equal(0, score.DefendingTeamDelta);
    }



    [Fact]
    public void DeltaFor_and_TricksFor_give_each_team_its_own_side_of_the_round()
    {
        // East/West bid 8 and took 5: set, -8; North/South took the other 7.
        var score = new RoundScore(Team.EastWest, 8, 5, 7, false);

        Assert.Equal(-8, score.DeltaFor(Team.EastWest));
        Assert.Equal(7, score.DeltaFor(Team.NorthSouth));
        Assert.Equal(5, score.TricksFor(Team.EastWest));
        Assert.Equal(7, score.TricksFor(Team.NorthSouth));
    }
}
