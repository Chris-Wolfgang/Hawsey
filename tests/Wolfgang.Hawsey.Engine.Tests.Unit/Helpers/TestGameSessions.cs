using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

/// <summary>
/// Game sessions for tests, with a seeded deal so games repeat.
/// </summary>
internal static class TestGameSessions
{
    /// <summary>
    /// No pauses between AI moves: every AI loop completes before its task is returned.
    /// </summary>
    public static GameSession Unpaced(int seed = 42, IPlayerStrategy? ai = null) =>
        new(aiPacing: false, randomFactory: () => new Random(seed), aiStrategy: ai ?? new SimpleAiStrategy());



    /// <summary>
    /// Paced like the app (real pauses between AI moves). For tests that start a New
    /// Game while an AI loop is paused.
    /// </summary>
    public static GameSession Paced(int seed = 42, IPlayerStrategy? ai = null) =>
        new(aiPacing: true, randomFactory: () => new Random(seed), aiStrategy: ai ?? new SimpleAiStrategy());
}
