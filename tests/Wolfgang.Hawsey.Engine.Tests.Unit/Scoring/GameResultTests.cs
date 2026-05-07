using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Scoring;

public class GameResultTests
{
    [Fact]
    public void Constructor_when_called_assigns_winner()
    {
        var result = new GameResult(Team.NorthSouth, northSouthScore: 24, eastWestScore: 18);

        Assert.Equal(Team.NorthSouth, result.Winner);
    }



    [Fact]
    public void Constructor_when_called_assigns_north_south_score()
    {
        var result = new GameResult(Team.NorthSouth, northSouthScore: 24, eastWestScore: 18);

        Assert.Equal(24, result.NorthSouthScore);
    }



    [Fact]
    public void Constructor_when_called_assigns_east_west_score()
    {
        var result = new GameResult(Team.NorthSouth, northSouthScore: 24, eastWestScore: 18);

        Assert.Equal(18, result.EastWestScore);
    }



    [Fact]
    public void Constructor_when_east_west_wins_returns_east_west_winner()
    {
        var result = new GameResult(Team.EastWest, northSouthScore: 11, eastWestScore: 24);

        Assert.Equal(Team.EastWest, result.Winner);
        Assert.Equal(11, result.NorthSouthScore);
        Assert.Equal(24, result.EastWestScore);
    }



    [Fact]
    public void Constructor_when_scores_are_negative_assigns_them_as_given()
    {
        var result = new GameResult(Team.NorthSouth, northSouthScore: 5, eastWestScore: -7);

        Assert.Equal(5, result.NorthSouthScore);
        Assert.Equal(-7, result.EastWestScore);
    }
}
