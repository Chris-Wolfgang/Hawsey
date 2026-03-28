namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents a player's hand of cards. Cards can be added and removed
/// as the game progresses.
/// </summary>
public sealed class PlayerHand
{
    private readonly List<Card> _cards;



    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerHand"/> class with the specified cards.
    /// </summary>
    /// <param name="cards">The initial cards in the hand.</param>
    public PlayerHand(IEnumerable<Card> cards)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        _cards = new List<Card>(cards);
    }



    /// <summary>
    /// Gets the current cards in the hand.
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards;



    /// <summary>
    /// Removes one instance of the specified card from the hand.
    /// </summary>
    /// <param name="card">The card to remove.</param>
    /// <exception cref="InvalidOperationException">The card is not in the hand.</exception>
    public void Remove(Card card)
    {
        var index = _cards.IndexOf(card);

        if (index < 0)
        {
            throw new InvalidOperationException($"Card {card} is not in the hand.");
        }

        _cards.RemoveAt(index);
    }



    /// <summary>
    /// Adds cards to the hand.
    /// </summary>
    /// <param name="cards">The cards to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cards"/> is <c>null</c>.</exception>
    public void Add(IEnumerable<Card> cards)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        _cards.AddRange(cards);
    }



    /// <summary>
    /// Determines whether the hand contains the specified card.
    /// </summary>
    /// <param name="card">The card to look for.</param>
    /// <returns><c>true</c> if the hand contains the card; otherwise, <c>false</c>.</returns>
    public bool Contains(Card card) => _cards.Contains(card);
}
