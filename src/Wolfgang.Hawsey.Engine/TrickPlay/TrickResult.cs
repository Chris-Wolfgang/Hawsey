using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.Engine.TrickPlay;

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
    /// <exception cref="ArgumentNullException"><paramref name="cards"/> is <see langword="null"/>.</exception>
    public TrickResult(PlayerPosition winner, IReadOnlyList<PlayedCard> cards)
    {
        Winner = winner;
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
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
