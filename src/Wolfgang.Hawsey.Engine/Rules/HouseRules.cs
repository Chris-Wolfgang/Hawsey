namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Configurable house rules for a Hawsey game.
/// </summary>
public sealed class HouseRules
{
    /// <summary>
    /// Gets the default house rules.
    /// </summary>
    public static HouseRules Default { get; } = new HouseRules();



    /// <summary>
    /// Gets a value indicating whether a player must play a higher card than the current
    /// winner of the trick, even if the current winner is their partner.
    /// Default is <c>false</c>.
    /// </summary>
    public bool MustBeat { get; init; }



    /// <summary>
    /// Gets a value indicating whether a player must play a trump card if they cannot
    /// follow the led suit and have trump cards in their hand.
    /// Default is <c>false</c>.
    /// </summary>
    public bool MustTrump { get; init; }



    /// <summary>
    /// Gets the minimum bid allowed. Default is 6.
    /// </summary>
    public int MinimumBid { get; init; } = 6;



    /// <summary>
    /// Gets the number of points required to win the game. Default is 62.
    /// </summary>
    public int PointsToWin { get; init; } = 62;
}
