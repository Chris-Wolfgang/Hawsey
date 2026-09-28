using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.Engine.Rules;

/// <summary>
/// Configurable house rules for a Hawsey game.
/// </summary>
public sealed class HouseRules
{
    private readonly int _minimumBid = 6;
    private readonly int _pointsToWin = 62;



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
    /// Gets the minimum bid allowed, from 1 to <see cref="GameState.MaximumBid"/>. Default is 6.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is below 1 or above <see cref="GameState.MaximumBid"/>.
    /// </exception>
    public int MinimumBid
    {
        get => _minimumBid;
        init
        {
            if (value < 1 || value > GameState.MaximumBid)
            {
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    value,
                    $"The minimum bid must be from 1 to {GameState.MaximumBid}."
                );
            }

            _minimumBid = value;
        }
    }



    /// <summary>
    /// Gets the number of points required to win the game, at least 1. Default is 62.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is below 1.</exception>
    public int PointsToWin
    {
        get => _pointsToWin;
        init
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    value,
                    "The points to win must be at least 1."
                );
            }

            _pointsToWin = value;
        }
    }
}
