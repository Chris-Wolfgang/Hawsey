namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the outcome of a bidding phase.
/// </summary>
public sealed class BiddingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BiddingResult"/> class.
    /// </summary>
    /// <param name="winner">The player who won the bid.</param>
    /// <param name="bidAmount">The winning bid amount.</param>
    /// <param name="isHawsey">Whether the winning bid was a Hawsey call.</param>
    /// <param name="isStuck">Whether the dealer was stuck with the minimum bid.</param>
    public BiddingResult(PlayerPosition winner, int bidAmount, bool isHawsey, bool isStuck)
    {
        Winner = winner;
        BidAmount = bidAmount;
        IsHawsey = isHawsey;
        IsStuck = isStuck;
    }



    /// <summary>
    /// Gets the player who won the bid.
    /// </summary>
    public PlayerPosition Winner { get; }



    /// <summary>
    /// Gets the winning bid amount.
    /// </summary>
    public int BidAmount { get; }



    /// <summary>
    /// Gets a value indicating whether the winning bid was a Hawsey call.
    /// </summary>
    public bool IsHawsey { get; }



    /// <summary>
    /// Gets a value indicating whether the dealer was stuck with the minimum bid
    /// because no one else bid.
    /// </summary>
    public bool IsStuck { get; }
}
