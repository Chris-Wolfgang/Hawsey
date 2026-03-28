namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the outcome of a completed trick.
/// </summary>
public sealed class TrickResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrickResult"/> class.
    /// </summary>
    /// <param name="winner">The player who won the trick.</param>
    /// <param name="cards">The cards that were played in the trick.</param>
    public TrickResult(PlayerPosition winner, IReadOnlyList<PlayedCard> cards)
    {
        Winner = winner;
        Cards = cards;
    }



    /// <summary>
    /// Gets the player who won the trick.
    /// </summary>
    public PlayerPosition Winner { get; }



    /// <summary>
    /// Gets the cards that were played in the trick, in order.
    /// </summary>
    public IReadOnlyList<PlayedCard> Cards { get; }
}
