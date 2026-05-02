namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Interface for player decision-making strategies.
/// Implement this for AI players, UI-driven players, or network players.
/// </summary>
public interface IPlayerStrategy
{
    /// <summary>
    /// Decides what bid to place.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="player">The player making the decision.</param>
    /// <returns>The bid action to take.</returns>
    BidAction DecideBid(GameState state, PlayerPosition player);



    /// <summary>
    /// Decides which trump suit to pick, or <c>null</c> for Ace high.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="player">The player making the decision.</param>
    /// <returns>The trump suit, or <c>null</c> for Ace high.</returns>
    Suit? DecideTrump(GameState state, PlayerPosition player);



    /// <summary>
    /// Decides which card to play.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="player">The player making the decision.</param>
    /// <returns>The card to play.</returns>
    Card DecidePlay(GameState state, PlayerPosition player);



    /// <summary>
    /// Decides which cards to discard and which cards the partner gives
    /// during a Hawsey exchange.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="bidder">The Hawsey bidder.</param>
    /// <param name="cardsToDiscard">Output: the 2 cards the bidder discards.</param>
    /// <param name="cardsFromPartner">Output: the 2 cards the partner gives.</param>
    void DecideHawseyExchange
    (
        GameState state,
        PlayerPosition bidder,
        out Card[] cardsToDiscard,
        out Card[] cardsFromPartner
    );
}
