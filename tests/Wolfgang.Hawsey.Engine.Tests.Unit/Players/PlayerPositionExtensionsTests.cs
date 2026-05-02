using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Players;

public class PlayerPositionExtensionsTests
{
    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.North)]
    public void NextClockwise_returns_correct_position(PlayerPosition input, PlayerPosition expected)
    {
        Assert.Equal(expected, input.NextClockwise());
    }



    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, PlayerPosition.North)]
    [InlineData(PlayerPosition.East, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.East)]
    public void Partner_returns_opposite_position(PlayerPosition input, PlayerPosition expected)
    {
        Assert.Equal(expected, input.Partner());
    }



    [Theory]
    [InlineData(PlayerPosition.North, Team.NorthSouth)]
    [InlineData(PlayerPosition.South, Team.NorthSouth)]
    [InlineData(PlayerPosition.East, Team.EastWest)]
    [InlineData(PlayerPosition.West, Team.EastWest)]
    public void GetTeam_returns_correct_team(PlayerPosition position, Team expected)
    {
        Assert.Equal(expected, position.GetTeam());
    }
}
