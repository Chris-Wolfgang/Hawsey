namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Factory for creating and shuffling a pinochle deck.
/// </summary>
public static class Deck
{
    /// <summary>
    /// The total number of cards in a pinochle deck.
    /// </summary>
    public const int CardCount = 48;

    private static readonly Rank[] PinochleRanks = new[]
    {
        Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace
    };

    private static readonly Suit[] AllSuits = new[]
    {
        Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades
    };



    /// <summary>
    /// Creates a standard pinochle deck of 48 cards (each rank/suit combination appears twice).
    /// </summary>
    /// <returns>A list of 48 cards in a deterministic order.</returns>
    public static IReadOnlyList<Card> CreatePinochleDeck()
    {
        var cards = new List<Card>(CardCount);

        for (var copy = 0; copy < 2; copy++)
        {
            foreach (var suit in AllSuits)
            {
                foreach (var rank in PinochleRanks)
                {
                    cards.Add(new Card(rank, suit));
                }
            }
        }

        return cards;
    }



    /// <summary>
    /// Shuffles a collection of cards using the Fisher-Yates algorithm.
    /// </summary>
    /// <param name="cards">The cards to shuffle.</param>
    /// <param name="random">The random number generator to use for shuffling.</param>
    /// <returns>A new list containing the cards in shuffled order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cards"/> or <paramref name="random"/> is <c>null</c>.</exception>
    public static IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> cards, Random random)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var shuffled = new Card[cards.Count];

        for (var i = 0; i < cards.Count; i++)
        {
            shuffled[i] = cards[i];
        }

        for (var i = shuffled.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        return shuffled;
    }
}
