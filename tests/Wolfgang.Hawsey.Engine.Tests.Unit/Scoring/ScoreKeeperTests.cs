using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Scoring;

public class ScoreKeeperTests
{
    [Fact]
    public void RecordRound_when_ns_bids_and_makes_updates_correctly()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 7, 8, 4, isHawsey: false));

        Assert.Equal(8, keeper.NorthSouthScore);
        Assert.Equal(4, keeper.EastWestScore);
    }



    [Fact]
    public void RecordRound_when_ew_bids_and_makes_updates_correctly()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.EastWest, 7, 8, 4, isHawsey: false));

        Assert.Equal(8, keeper.EastWestScore);
        Assert.Equal(4, keeper.NorthSouthScore);
    }



    [Fact]
    public void RecordRound_when_bid_missed_goes_negative()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 7, 5, 7, isHawsey: false));

        Assert.Equal(-7, keeper.NorthSouthScore);
        Assert.Equal(7, keeper.EastWestScore);
    }



    [Fact]
    public void RecordRound_accumulates_across_rounds()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 6, 8, 4, isHawsey: false));
        keeper.RecordRound(new RoundScore(Team.EastWest, 7, 9, 3, isHawsey: false));

        Assert.Equal(8 + 3, keeper.NorthSouthScore);
        Assert.Equal(4 + 9, keeper.EastWestScore);
    }



    [Fact]
    public void GetWinner_when_no_team_at_target_returns_null()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 6, 8, 4, isHawsey: false));

        Assert.Null(keeper.GetWinner(Team.NorthSouth));
    }



    [Fact]
    public void GetWinner_when_ns_reaches_target_returns_ns()
    {
        var keeper = new ScoreKeeper(10);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 6, 10, 2, isHawsey: false));

        Assert.Equal(Team.NorthSouth, keeper.GetWinner(Team.NorthSouth));
    }



    [Fact]
    public void GetWinner_when_ew_reaches_target_returns_ew()
    {
        var keeper = new ScoreKeeper(10);

        keeper.RecordRound(new RoundScore(Team.EastWest, 6, 10, 2, isHawsey: false));

        Assert.Equal(Team.EastWest, keeper.GetWinner(Team.EastWest));
    }



    [Fact]
    public void GetWinner_when_both_reach_target_bidding_team_wins()
    {
        var keeper = new ScoreKeeper(10);

        // Both teams at 8
        keeper.RecordRound(new RoundScore(Team.NorthSouth, 6, 8, 8, isHawsey: false));

        // NS bids 6, makes 8 (NS->16, EW->12) — both above 10
        keeper.RecordRound(new RoundScore(Team.NorthSouth, 6, 8, 4, isHawsey: false));

        Assert.Equal(Team.NorthSouth, keeper.GetWinner(Team.NorthSouth));
    }



    [Fact]
    public void Scores_can_go_negative()
    {
        var keeper = new ScoreKeeper(62);

        keeper.RecordRound(new RoundScore(Team.NorthSouth, 10, 3, 9, isHawsey: false));

        Assert.Equal(-10, keeper.NorthSouthScore);
    }



    [Fact]
    public void RecordRound_when_null_throws()
    {
        var keeper = new ScoreKeeper(62);

        Assert.Throws<ArgumentNullException>(() => keeper.RecordRound(null!));
    }



    [Fact]
    public void GetWinner_when_defending_team_reaches_target_through_tricks()
    {
        var keeper = new ScoreKeeper(10);

        // EW bids but NS (defending) scores enough tricks to win
        keeper.RecordRound(new RoundScore(Team.EastWest, 6, 3, 9, isHawsey: false));

        // NS scored 9, EW scored 3
        keeper.RecordRound(new RoundScore(Team.EastWest, 6, 6, 6, isHawsey: false));

        // NS: 9+6=15, EW: 3+6=9 — only NS reached
        Assert.Equal(Team.NorthSouth, keeper.GetWinner(Team.EastWest));
    }
}
