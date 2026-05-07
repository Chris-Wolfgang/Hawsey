using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.Services;

/// <summary>
/// Payload for <see cref="GameService.TrickCompleted"/>.
/// </summary>
public sealed class TrickCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrickCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="winner">The player who won the trick.</param>
    public TrickCompletedEventArgs(PlayerPosition winner)
    {
        Winner = winner;
    }

    /// <summary>Gets the player who won the trick.</summary>
    public PlayerPosition Winner { get; }
}
