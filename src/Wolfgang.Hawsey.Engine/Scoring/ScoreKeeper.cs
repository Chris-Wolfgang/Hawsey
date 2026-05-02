namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Tracks cumulative team scores across rounds and determines the game winner.
/// </summary>
public sealed class ScoreKeeper
{
    private readonly int _pointsToWin;



    /// <summary>
    /// Initializes a new instance of the <see cref="ScoreKeeper"/> class.
    /// </summary>
    /// <param name="pointsToWin">The number of points required to win.</param>
    public ScoreKeeper(int pointsToWin)
    {
        _pointsToWin = pointsToWin;
    }



    /// <summary>
    /// Gets the current North/South score.
    /// </summary>
    public int NorthSouthScore { get; private set; }



    /// <summary>
    /// Gets the current East/West score.
    /// </summary>
    public int EastWestScore { get; private set; }



    /// <summary>
    /// Records a round's scoring result and updates cumulative scores.
    /// </summary>
    /// <param name="roundScore">The scoring result for the round.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roundScore"/> is <c>null</c>.</exception>
    public void RecordRound(RoundScore roundScore)
    {
        if (roundScore == null)
        {
            throw new ArgumentNullException(nameof(roundScore));
        }

        if (roundScore.BiddingTeam == Team.NorthSouth)
        {
            NorthSouthScore += roundScore.BiddingTeamDelta;
            EastWestScore += roundScore.DefendingTeamDelta;
        }
        else
        {
            EastWestScore += roundScore.BiddingTeamDelta;
            NorthSouthScore += roundScore.DefendingTeamDelta;
        }
    }



    /// <summary>
    /// Gets the winning team if the game is over, or <c>null</c> if the game continues.
    /// If both teams reach the target in the same round, the bidding team wins.
    /// </summary>
    /// <param name="biddingTeam">The team that had the bid in the current round.</param>
    /// <returns>The winning team, or <c>null</c> if no team has reached the target.</returns>
    public Team? GetWinner(Team biddingTeam)
    {
        var nsReached = NorthSouthScore >= _pointsToWin;
        var ewReached = EastWestScore >= _pointsToWin;

        if (nsReached && ewReached)
        {
            return biddingTeam;
        }

        if (nsReached)
        {
            return Team.NorthSouth;
        }

        if (ewReached)
        {
            return Team.EastWest;
        }

        return null;
    }
}
