using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.Services;

/// <summary>
/// Payload for <see cref="GameService.GameOver"/>.
/// </summary>
public sealed class GameOverEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameOverEventArgs"/> class.
    /// </summary>
    /// <param name="winner">The team that won the game.</param>
    public GameOverEventArgs(Team winner)
    {
        Winner = winner;
    }

    /// <summary>Gets the team that won the game.</summary>
    public Team Winner { get; }
}
