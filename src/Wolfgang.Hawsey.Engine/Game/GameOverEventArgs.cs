
using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.Engine.Game;

/// <summary>
/// Payload for <see cref="GameSession.GameOver"/>.
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
