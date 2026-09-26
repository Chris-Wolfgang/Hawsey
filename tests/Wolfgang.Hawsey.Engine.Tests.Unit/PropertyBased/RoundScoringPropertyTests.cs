using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Scoring;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// Plays a real first round from a generated seed and dealer, then checks the
/// engine's scoring against the tricks the state recorded. Stryker showed every
/// scoring mutant (team comparison, += / -=, the points-to-win boundary) survived
/// without these, because no test looked at the scores a round produces.
/// </summary>
public class RoundScoringPropertyTests
{
    private static readonly PlayerPosition[] Dealers =
        [PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West];



    private static GameState PlayFirstRound(int seed, int dealer, HouseRules rules)
    {
        var start = new GameEngine().StartGame(rules, Dealers[Math.Abs(dealer % Dealers.Length)], new Random(seed));

        return RoundDriver.PlayOneRound(start, new TestPlayerStrategy());
    }



    [FuzzProperty]
    public void First_round_scores_match_the_tricks_each_team_won(int seed, int dealer)
    {
        var state = PlayFirstRound(seed, dealer, HouseRules.Default);
        var bid = state.BiddingResult!;
        var biddingTeam = bid.Winner.GetTeam();
        var biddingTricks = state.CompletedTricks.Count(t => t.Winner.GetTeam() == biddingTeam);
        var expected = new RoundScore(biddingTeam, bid.BidAmount, biddingTricks, 12 - biddingTricks, bid.IsHawsey);

        var expectedScores = biddingTeam == Team.NorthSouth
            ? (NorthSouth: expected.BiddingTeamDelta, EastWest: expected.DefendingTeamDelta)
            : (NorthSouth: expected.DefendingTeamDelta, EastWest: expected.BiddingTeamDelta);

        Assert.Equal(12, state.CompletedTricks.Count);
        Assert.Equal(expectedScores, (state.NorthSouthScore, state.EastWestScore));
    }



    [FuzzProperty]
    public void First_round_ends_the_game_exactly_when_a_team_reaches_points_to_win(int seed, int dealer, int target)
    {
        var pointsToWin = 1 + Math.Abs(target % 30);
        var state = PlayFirstRound(seed, dealer, new HouseRules { PointsToWin = pointsToWin });

        var reached = state.NorthSouthScore >= pointsToWin || state.EastWestScore >= pointsToWin;

        Assert.Equal(reached ? GamePhase.GameOver : GamePhase.RoundScoring, state.Phase);
    }
}
