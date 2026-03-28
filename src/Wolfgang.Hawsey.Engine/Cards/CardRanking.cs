namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Provides methods to determine the effective suit and rank of a card
/// given the current trump context. This is the single source of truth
/// for left bower behavior.
/// </summary>
public static class CardRanking
{
    /// <summary>
    /// The effective rank value for the right bower (Jack of trump suit).
    /// </summary>
    public const int RightBowerRank = 16;

    /// <summary>
    /// The effective rank value for the left bower (Jack of same-color suit as trump).
    /// </summary>
    public const int LeftBowerRank = 15;



    /// <summary>
    /// Gets the effective suit of a card given the trump suit.
    /// The left bower (Jack of the same-color suit as trump) is considered
    /// part of the trump suit.
    /// </summary>
    /// <param name="card">The card to evaluate.</param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <returns>The effective suit of the card.</returns>
    public static Suit GetEffectiveSuit(Card card, Suit? trumpSuit)
    {
        if (trumpSuit.HasValue && IsLeftBower(card, trumpSuit.Value))
        {
            return trumpSuit.Value;
        }

        return card.Suit;
    }



    /// <summary>
    /// Gets the effective rank of a card as an integer for comparison purposes.
    /// Higher values beat lower values within the same effective suit.
    /// </summary>
    /// <param name="card">The card to evaluate.</param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <returns>An integer representing the card's effective rank.</returns>
    public static int GetEffectiveRank(Card card, Suit? trumpSuit)
    {
        if (trumpSuit.HasValue)
        {
            if (IsRightBower(card, trumpSuit.Value))
            {
                return RightBowerRank;
            }

            if (IsLeftBower(card, trumpSuit.Value))
            {
                return LeftBowerRank;
            }
        }

        return (int)card.Rank;
    }



    /// <summary>
    /// Determines whether a card is the right bower (Jack of the trump suit).
    /// </summary>
    /// <param name="card">The card to check.</param>
    /// <param name="trumpSuit">The trump suit.</param>
    /// <returns><c>true</c> if the card is the right bower; otherwise, <c>false</c>.</returns>
    public static bool IsRightBower(Card card, Suit trumpSuit)
    {
        return card.Rank == Rank.Jack && card.Suit == trumpSuit;
    }



    /// <summary>
    /// Determines whether a card is the left bower (Jack of the same-color suit as trump).
    /// </summary>
    /// <param name="card">The card to check.</param>
    /// <param name="trumpSuit">The trump suit.</param>
    /// <returns><c>true</c> if the card is the left bower; otherwise, <c>false</c>.</returns>
    public static bool IsLeftBower(Card card, Suit trumpSuit)
    {
        return card.Rank == Rank.Jack && card.Suit == trumpSuit.GetSameColorSuit();
    }



    /// <summary>
    /// Determines whether a card is effectively a trump card (including the left bower).
    /// </summary>
    /// <param name="card">The card to check.</param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <returns><c>true</c> if the card is trump; otherwise, <c>false</c>.</returns>
    public static bool IsTrump(Card card, Suit? trumpSuit)
    {
        if (!trumpSuit.HasValue)
        {
            return false;
        }

        return GetEffectiveSuit(card, trumpSuit) == trumpSuit.Value;
    }
}
