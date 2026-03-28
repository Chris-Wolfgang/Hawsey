namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the trump selection for a round.
/// </summary>
public enum TrumpMode
{
    /// <summary>A specific suit is trump.</summary>
    Suited = 0,

    /// <summary>No trump — Aces are the highest card in all suits.</summary>
    AceHigh = 1
}
