using Wolfgang.Hawsey.Engine;

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
}
