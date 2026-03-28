namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Computes the scoring result for a single round of Hawsey.
/// </summary>
public sealed class RoundScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundScore"/> class.
    /// </summary>
    /// <param name="biddingTeam">The team that won the bid.</param>
    /// <param name="bidAmount">The winning bid amount.</param>
    /// <param name="biddingTeamTricks">The number of tricks taken by the bidding team.</param>
    /// <param name="defendingTeamTricks">The number of tricks taken by the defending team.</param>
    /// <param name="isHawsey">Whether this was a Hawsey round.</param>
    public RoundScore
    (
        Team biddingTeam,
        int bidAmount,
        int biddingTeamTricks,
        int defendingTeamTricks,
        bool isHawsey
    )
    {
        BiddingTeam = biddingTeam;
        BidAmount = bidAmount;
        BiddingTeamTricks = biddingTeamTricks;
        DefendingTeamTricks = defendingTeamTricks;
        IsHawsey = isHawsey;
    }



    /// <summary>
    /// Gets the team that won the bid.
    /// </summary>
    public Team BiddingTeam { get; }



    /// <summary>
    /// Gets the winning bid amount.
    /// </summary>
    public int BidAmount { get; }



    /// <summary>
    /// Gets the number of tricks taken by the bidding team.
    /// </summary>
    public int BiddingTeamTricks { get; }



    /// <summary>
    /// Gets the number of tricks taken by the defending team.
    /// </summary>
    public int DefendingTeamTricks { get; }



    /// <summary>
    /// Gets a value indicating whether this was a Hawsey round.
    /// </summary>
    public bool IsHawsey { get; }



    /// <summary>
    /// Gets the point change for the bidding team.
    /// </summary>
    public int BiddingTeamDelta
    {
        get
        {
            if (IsHawsey)
            {
                return BiddingTeamTricks == 12 ? 24 : -24;
            }

            return BiddingTeamTricks >= BidAmount
                ? BiddingTeamTricks
                : -BidAmount;
        }
    }



    /// <summary>
    /// Gets the point change for the defending team.
    /// </summary>
    public int DefendingTeamDelta => DefendingTeamTricks;
}
