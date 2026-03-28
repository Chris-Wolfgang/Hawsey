namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the final result of a completed Hawsey game.
/// </summary>
public sealed class GameResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameResult"/> class.
    /// </summary>
    /// <param name="winner">The winning team.</param>
    /// <param name="northSouthScore">The final North/South score.</param>
    /// <param name="eastWestScore">The final East/West score.</param>
    public GameResult(Team winner, int northSouthScore, int eastWestScore)
    {
        Winner = winner;
        NorthSouthScore = northSouthScore;
        EastWestScore = eastWestScore;
    }



    /// <summary>
    /// Gets the winning team.
    /// </summary>
    public Team Winner { get; }



    /// <summary>
    /// Gets the final North/South score.
    /// </summary>
    public int NorthSouthScore { get; }



    /// <summary>
    /// Gets the final East/West score.
    /// </summary>
    public int EastWestScore { get; }
}
