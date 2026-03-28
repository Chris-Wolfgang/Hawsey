namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Tracks which player is the dealer and rotates clockwise after each round.
/// </summary>
public sealed class DealerRotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DealerRotation"/> class.
    /// </summary>
    /// <param name="firstDealer">The first dealer's position.</param>
    public DealerRotation(PlayerPosition firstDealer)
    {
        Current = firstDealer;
    }



    /// <summary>
    /// Gets the current dealer's position.
    /// </summary>
    public PlayerPosition Current { get; private set; }



    /// <summary>
    /// Advances to the next dealer (clockwise).
    /// </summary>
    public void Advance()
    {
        Current = Current.NextClockwise();
    }
}
