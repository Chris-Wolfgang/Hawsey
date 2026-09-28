using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;
using Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameState.Winner"/> (#862): the team that reached
/// <see cref="HouseRules.PointsToWin"/>, and the bidding team when both reached it in
/// the same round.
/// </summary>
public class GameStateWinnerTests
{
    private readonly GameEngine _engine = new();



    private GameState PlayFirstRound(int seed, int pointsToWin) =>
        RoundDriver.PlayOneRound
        (
            _engine.StartGame(new HouseRules { PointsToWin = pointsToWin }, PlayerPosition.North, new Random(seed)),
            new TestPlayerStrategy()
        );



    [Fact]
    public void Winner_is_null_while_the_game_continues()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(1));
        Assert.Null(state.Winner);

        var scored = PlayFirstRound(seed: 1, pointsToWin: 100);
        Assert.Equal(GamePhase.RoundScoring, scored.Phase);
        Assert.Null(scored.Winner);
    }



    [FuzzProperty]
    public void Winner_follows_the_points_to_win_rule(int seed, int target)
    {
        var state = PlayFirstRound(seed, pointsToWin: 1 + Math.Abs(target % 30));
        var northSouth = state.NorthSouthScore >= state.Rules.PointsToWin;
        var eastWest = state.EastWestScore >= state.Rules.PointsToWin;

        Team? expected = (northSouth, eastWest) switch
        {
            (true, true) => state.BiddingResult!.Winner.GetTeam(),
            (true, false) => Team.NorthSouth,
            (false, true) => Team.EastWest,
            _ => null,
        };

        Assert.Equal(expected, state.Winner);
    }



    [Fact]
    public void When_both_teams_reach_the_target_in_one_round_the_bidding_team_wins()
    {
        // With a target of 1, both teams reach it whenever the bidding team makes its
        // bid and the defenders take a trick. Search a fixed range of seeds for a round
        // where the stuck dealer's team is East/West, so North/South is NOT the answer,
        // which is what the old UI rule always said (#862).
        var state = Enumerable
            .Range(0, 500)
            .Select
            (
                seed => RoundDriver.PlayOneRound
                (
                    _engine.StartGame(new HouseRules { PointsToWin = 1 }, PlayerPosition.East, new Random(seed)),
                    new TestPlayerStrategy()
                )
            )
            .First(s => s.NorthSouthScore >= 1 && s.EastWestScore >= 1);

        Assert.Equal(GamePhase.GameOver, state.Phase);
        Assert.Equal(Team.EastWest, state.BiddingResult!.Winner.GetTeam());
        Assert.Equal(Team.EastWest, state.Winner);
    }



    private static GameState Scored(GamePhase phase, int northSouth, int eastWest) =>
        new
        (
            phase,
            PlayerPosition.North,
            new Dictionary<PlayerPosition, List<Card>>(),
            trumpSuit: null,
            TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks: [],
            currentTrick: null,
            northSouth,
            eastWest,
            HouseRules.Default,
            nextToAct: null,
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );



    [Fact]
    public void Winner_when_East_West_finishes_exactly_on_the_target_is_East_West()
    {
        Assert.Equal(Team.EastWest, Scored(GamePhase.GameOver, northSouth: 10, eastWest: HouseRules.Default.PointsToWin).Winner);
    }



    [Fact]
    public void Winner_before_game_over_is_null_even_past_the_target()
    {
        Assert.Null(Scored(GamePhase.RoundScoring, northSouth: HouseRules.Default.PointsToWin + 5, eastWest: 0).Winner);
    }
}
