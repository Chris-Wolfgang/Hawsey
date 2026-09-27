using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.UI.Maui.Threading;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;

/// <summary>
/// Runs dispatched work at once, on the calling thread.
/// </summary>
internal sealed class ImmediateDispatcher : IUiDispatcher
{
    public int PostCount { get; private set; }



    public void Post(Action action)
    {
        PostCount++;
        action();
    }
}



/// <summary>
/// A <see cref="GameSession"/> with no pauses between AI moves and a seeded deal, so
/// a command runs the game forward synchronously and repeatably.
/// </summary>
internal static class TestGameSessions
{
    public static GameSession Unpaced(int seed = 42) =>
        new(aiPacing: false, randomFactory: () => new Random(seed), aiStrategy: new SimpleAiStrategy());
}
