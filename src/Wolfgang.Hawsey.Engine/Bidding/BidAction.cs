namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents an action a player can take during the bidding phase.
/// </summary>
public abstract class BidAction
{
    private BidAction()
    {
    }



    /// <summary>
    /// A pass — the player does not bid.
    /// </summary>
    public sealed class PassBid : BidAction
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PassBid Instance { get; } = new PassBid();
    }



    /// <summary>
    /// A numeric bid for a specific number of tricks.
    /// </summary>
    public sealed class NumberBid : BidAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberBid"/> class.
        /// </summary>
        /// <param name="amount">The number of tricks being bid.</param>
        public NumberBid(int amount)
        {
            Amount = amount;
        }



        /// <summary>
        /// Gets the number of tricks being bid.
        /// </summary>
        public int Amount { get; }
    }



    /// <summary>
    /// A Hawsey bid — the player will attempt to take all 12 tricks alone.
    /// </summary>
    public sealed class HawseyBid : BidAction
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static HawseyBid Instance { get; } = new HawseyBid();
    }
}
