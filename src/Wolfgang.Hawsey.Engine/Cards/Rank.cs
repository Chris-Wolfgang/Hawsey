namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the ranks in a pinochle deck (9 through Ace).
/// </summary>
public enum Rank
{
    /// <summary>Nine — lowest rank.</summary>
    Nine = 9,

    /// <summary>Ten.</summary>
    Ten = 10,

    /// <summary>Jack.</summary>
    Jack = 11,

    /// <summary>Queen.</summary>
    Queen = 12,

    /// <summary>King.</summary>
    King = 13,

    /// <summary>Ace — highest rank in non-trump suits and Ace high mode.</summary>
    Ace = 14
}
