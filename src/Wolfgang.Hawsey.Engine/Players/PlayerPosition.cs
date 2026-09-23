namespace Wolfgang.Hawsey.Engine.Players;

/// <summary>
/// Represents a player's position at the table.
/// Partners sit across: North/South and East/West.
/// </summary>
public enum PlayerPosition
{
    /// <summary>North position.</summary>
    North = 0,

    /// <summary>East position — clockwise from North.</summary>
    East = 1,

    /// <summary>South position — across from North.</summary>
    South = 2,

    /// <summary>West position — clockwise from South.</summary>
    West = 3
}
