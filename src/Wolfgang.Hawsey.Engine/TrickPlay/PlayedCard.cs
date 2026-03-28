namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents a card that has been played in a trick, along with
/// who played it and the order in which it was played.
/// </summary>
public sealed class PlayedCard
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayedCard"/> class.
    /// </summary>
    /// <param name="card">The card that was played.</param>
    /// <param name="player">The player who played the card.</param>
    /// <param name="playOrder">The order in which the card was played (0-based).</param>
    public PlayedCard(Card card, PlayerPosition player, int playOrder)
    {
        Card = card;
        Player = player;
        PlayOrder = playOrder;
    }



    /// <summary>
    /// Gets the card that was played.
    /// </summary>
    public Card Card { get; }



    /// <summary>
    /// Gets the player who played the card.
    /// </summary>
    public PlayerPosition Player { get; }



    /// <summary>
    /// Gets the order in which the card was played (0-based).
    /// </summary>
    public int PlayOrder { get; }
}
