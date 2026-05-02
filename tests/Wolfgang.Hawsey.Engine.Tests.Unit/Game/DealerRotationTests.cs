using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

public class DealerRotationTests
{
    [Fact]
    public void Current_returns_initial_dealer()
    {
        var rotation = new DealerRotation(PlayerPosition.North);

        Assert.Equal(PlayerPosition.North, rotation.Current);
    }



    [Fact]
    public void Advance_moves_clockwise()
    {
        var rotation = new DealerRotation(PlayerPosition.North);

        rotation.Advance();

        Assert.Equal(PlayerPosition.East, rotation.Current);
    }



    [Fact]
    public void Advance_wraps_around()
    {
        var rotation = new DealerRotation(PlayerPosition.West);

        rotation.Advance();

        Assert.Equal(PlayerPosition.North, rotation.Current);
    }



    [Fact]
    public void Advance_full_cycle()
    {
        var rotation = new DealerRotation(PlayerPosition.North);

        rotation.Advance();
        rotation.Advance();
        rotation.Advance();
        rotation.Advance();

        Assert.Equal(PlayerPosition.North, rotation.Current);
    }
}
