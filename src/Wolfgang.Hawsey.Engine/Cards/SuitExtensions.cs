namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Extension methods for <see cref="Suit"/>.
/// </summary>
public static class SuitExtensions
{
    /// <summary>
    /// Gets the same-color partner suit. Hearts pairs with Diamonds, Clubs pairs with Spades.
    /// </summary>
    /// <param name="suit">The suit to find the partner for.</param>
    /// <returns>The same-color partner suit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="suit"/> is not a recognized suit value.</exception>
    public static Suit GetSameColorSuit(this Suit suit)
    {
        return suit switch
        {
            Suit.Hearts => Suit.Diamonds,
            Suit.Diamonds => Suit.Hearts,
            Suit.Clubs => Suit.Spades,
            Suit.Spades => Suit.Clubs,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown suit.")
        };
    }



    /// <summary>
    /// Gets whether the suit is red (Hearts or Diamonds).
    /// </summary>
    /// <param name="suit">The suit to check.</param>
    /// <returns><c>true</c> if the suit is red; otherwise, <c>false</c>.</returns>
    public static bool IsRed(this Suit suit)
    {
        return suit == Suit.Hearts || suit == Suit.Diamonds;
    }



    /// <summary>
    /// Gets whether the suit is black (Clubs or Spades).
    /// </summary>
    /// <param name="suit">The suit to check.</param>
    /// <returns><c>true</c> if the suit is black; otherwise, <c>false</c>.</returns>
    public static bool IsBlack(this Suit suit)
    {
        return suit == Suit.Clubs || suit == Suit.Spades;
    }
}
