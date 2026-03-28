namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Extension methods for <see cref="PlayerPosition"/>.
/// </summary>
public static class PlayerPositionExtensions
{
    /// <summary>
    /// Gets the next player position clockwise around the table.
    /// </summary>
    /// <param name="position">The current player position.</param>
    /// <returns>The next clockwise position.</returns>
    public static PlayerPosition NextClockwise(this PlayerPosition position)
    {
        return (PlayerPosition)(((int)position + 1) % 4);
    }



    /// <summary>
    /// Gets the partner's position (directly across the table).
    /// </summary>
    /// <param name="position">The current player position.</param>
    /// <returns>The partner's position.</returns>
    public static PlayerPosition Partner(this PlayerPosition position)
    {
        return (PlayerPosition)(((int)position + 2) % 4);
    }



    /// <summary>
    /// Gets the team that this player position belongs to.
    /// </summary>
    /// <param name="position">The player position.</param>
    /// <returns>The team the player belongs to.</returns>
    public static Team GetTeam(this PlayerPosition position)
    {
        return position == PlayerPosition.North || position == PlayerPosition.South
            ? Team.NorthSouth
            : Team.EastWest;
    }
}
