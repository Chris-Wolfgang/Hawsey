namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Compares two cards in the context of a trick, taking into account
/// the trump suit and the suit that was led.
/// </summary>
public sealed class CardComparer : IComparer<Card>
{
    private readonly Suit? _trumpSuit;
    private readonly Suit _ledSuit;



    /// <summary>
    /// Initializes a new instance of the <see cref="CardComparer"/> class.
    /// </summary>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <param name="ledSuit">The effective suit of the card that was led.</param>
    public CardComparer(Suit? trumpSuit, Suit ledSuit)
    {
        _trumpSuit = trumpSuit;
        _ledSuit = ledSuit;
    }



    /// <summary>
    /// Compares two cards and returns a value indicating which card wins
    /// in the context of the current trick.
    /// </summary>
    /// <param name="x">The first card to compare.</param>
    /// <param name="y">The second card to compare.</param>
    /// <returns>
    /// A positive value if <paramref name="x"/> beats <paramref name="y"/>;
    /// a negative value if <paramref name="y"/> beats <paramref name="x"/>;
    /// zero if the cards are of equal strength.
    /// </returns>
    public int Compare(Card x, Card y)
    {
        var xCategory = GetCategory(x);
        var yCategory = GetCategory(y);

        if (xCategory != yCategory)
        {
            return xCategory.CompareTo(yCategory);
        }

        return CardRanking.GetEffectiveRank(x, _trumpSuit)
            .CompareTo(CardRanking.GetEffectiveRank(y, _trumpSuit));
    }



    /// <summary>
    /// Gets the category of a card for comparison purposes.
    /// Trump = 2 (highest), Led suit = 1, Off-suit = 0 (lowest).
    /// </summary>
    private int GetCategory(Card card)
    {
        var effectiveSuit = CardRanking.GetEffectiveSuit(card, _trumpSuit);

        if (_trumpSuit.HasValue && effectiveSuit == _trumpSuit.Value)
        {
            return 2;
        }

        if (effectiveSuit == _ledSuit)
        {
            return 1;
        }

        return 0;
    }
}
