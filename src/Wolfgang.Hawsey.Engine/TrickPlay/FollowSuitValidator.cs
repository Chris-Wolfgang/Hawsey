namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Validates which cards in a player's hand are legal to play in the current trick,
/// taking into account follow-suit rules, trump rules, and house rules.
/// </summary>
public static class FollowSuitValidator
{
    /// <summary>
    /// Gets the list of cards from the hand that are legal to play.
    /// </summary>
    /// <param name="hand">The cards currently in the player's hand.</param>
    /// <param name="ledSuit">
    /// The effective suit of the card that was led, or <c>null</c> if this player is leading.
    /// </param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <param name="rules">The house rules in effect.</param>
    /// <param name="currentWinningCard">
    /// The card currently winning the trick, or <c>null</c> if this player is leading.
    /// </param>
    /// <returns>A list of legal cards to play.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hand"/> or <paramref name="rules"/> is <c>null</c>.</exception>
    public static IReadOnlyList<Card> GetLegalPlays
    (
        IReadOnlyList<Card> hand,
        Suit? ledSuit,
        Suit? trumpSuit,
        HouseRules rules,
        Card? currentWinningCard
    )
    {
        if (hand == null)
        {
            throw new ArgumentNullException(nameof(hand));
        }

        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (hand.Count == 0)
        {
            return Array.Empty<Card>();
        }

        // Leading: all cards are legal
        if (!ledSuit.HasValue)
        {
            return hand;
        }

        // Find cards that follow the led suit (using effective suit for bower logic)
        var followSuitCards = GetCardsOfEffectiveSuit(hand, ledSuit.Value, trumpSuit);

        if (followSuitCards.Count > 0)
        {
            // Must follow suit — optionally must beat
            if (rules.MustBeat && currentWinningCard.HasValue)
            {
                return ApplyMustBeat(followSuitCards, currentWinningCard.Value, trumpSuit, ledSuit.Value);
            }

            return followSuitCards;
        }

        // Void in led suit
        if (rules.MustTrump && trumpSuit.HasValue)
        {
            var trumpCards = GetCardsOfEffectiveSuit(hand, trumpSuit.Value, trumpSuit);

            if (trumpCards.Count > 0)
            {
                if (rules.MustBeat && currentWinningCard.HasValue)
                {
                    return ApplyMustBeat(trumpCards, currentWinningCard.Value, trumpSuit, ledSuit.Value);
                }

                return trumpCards;
            }
        }

        // Can play anything
        return hand;
    }



    /// <summary>
    /// Filters cards to only those that beat the current winning card.
    /// If no card can beat the winner, returns the full set of candidates.
    /// </summary>
    private static IReadOnlyList<Card> ApplyMustBeat
    (
        IReadOnlyList<Card> candidates,
        Card currentWinner,
        Suit? trumpSuit,
        Suit ledSuit
    )
    {
        var comparer = new CardComparer(trumpSuit, ledSuit);

        var beatingCards = new List<Card>();

        for (var i = 0; i < candidates.Count; i++)
        {
            if (comparer.Compare(candidates[i], currentWinner) > 0)
            {
                beatingCards.Add(candidates[i]);
            }
        }

        return beatingCards.Count > 0 ? beatingCards : candidates;
    }



    /// <summary>
    /// Gets all cards from the hand whose effective suit matches the target suit.
    /// </summary>
    private static IReadOnlyList<Card> GetCardsOfEffectiveSuit
    (
        IReadOnlyList<Card> hand,
        Suit targetSuit,
        Suit? trumpSuit
    )
    {
        var result = new List<Card>();

        for (var i = 0; i < hand.Count; i++)
        {
            if (CardRanking.GetEffectiveSuit(hand[i], trumpSuit) == targetSuit)
            {
                result.Add(hand[i]);
            }
        }

        return result;
    }
}
