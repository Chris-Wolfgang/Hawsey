using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Scoring;
using Wolfgang.Hawsey.Engine.TrickPlay;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// The round summary panel reads every label, colour and number from the engine's
/// <see cref="RoundScore"/> and <see cref="GameState.Winner"/>.
/// </summary>
public class RoundSummaryViewModelTests
{
    private static GameState Scored(RoundScore? score, int us, int them, GamePhase phase = GamePhase.RoundScoring) =>
        new
        (
            phase: phase,
            dealer: PlayerPosition.North,
            hands: new Dictionary<PlayerPosition, List<Card>>(),
            trumpSuit: null,
            trumpMode: TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks: new List<TrickResult>(),
            currentTrick: null,
            northSouthScore: us,
            eastWestScore: them,
            rules: HouseRules.Default,
            nextToAct: null,
            tricksPlayedInRound: GameState.TricksPerRound,
            isHawseyRound: score?.IsHawsey ?? false,
            hawseyBidder: null,
            roundScore: score
        );



    [Fact]
    public void From_when_the_human_team_makes_its_bid_shows_it_in_blue_and_green()
    {
        var summary = RoundSummaryViewModel.From(Scored(new RoundScore(Team.NorthSouth, 7, 8, 4, isHawsey: false), us: 8, them: 4), isGameOver: false);

        Assert.False(summary.IsGameOver);
        Assert.Equal("Round complete", summary.Title);
        Assert.False(summary.IsTitleUs);
        Assert.Equal("US", summary.BidTeamLabel);
        Assert.True(summary.IsBidTeamUs);
        Assert.Equal(TableColors.UsBlue, summary.BidTeamColor);
        Assert.Equal("7", summary.BidText);
        Assert.True(summary.Made);
        Assert.Equal("made it", summary.ResultText);
        Assert.Equal(TableColors.Positive, summary.ResultColor);
        Assert.Equal(8, summary.UsTricks);
        Assert.Equal(4, summary.ThemTricks);
        Assert.Equal(8, summary.UsDelta);
        Assert.Equal(4, summary.ThemDelta);
        Assert.Equal("+8", summary.UsDeltaText);
        Assert.Equal("+4", summary.ThemDeltaText);
        Assert.Equal(TableColors.Positive, summary.UsDeltaColor);
        Assert.Equal(TableColors.Positive, summary.ThemDeltaColor);
        Assert.Equal(8, summary.UsTotal);
        Assert.Equal(4, summary.ThemTotal);
    }



    [Fact]
    public void From_when_the_opponents_go_set_shows_the_loss_in_red()
    {
        var summary = RoundSummaryViewModel.From(Scored(new RoundScore(Team.EastWest, 9, 7, 5, isHawsey: false), us: 5, them: -9), isGameOver: false);

        Assert.Equal("THEM", summary.BidTeamLabel);
        Assert.False(summary.IsBidTeamUs);
        Assert.Equal(TableColors.ThemRed, summary.BidTeamColor);
        Assert.False(summary.Made);
        Assert.Equal("went set", summary.ResultText);
        Assert.Equal(TableColors.ThemRed, summary.ResultColor);
        Assert.Equal(5, summary.UsTricks);
        Assert.Equal(7, summary.ThemTricks);
        Assert.Equal("+5", summary.UsDeltaText);
        Assert.Equal("-9", summary.ThemDeltaText);
        Assert.Equal(TableColors.Positive, summary.UsDeltaColor);
        Assert.Equal(TableColors.ThemRed, summary.ThemDeltaColor);
    }



    [Fact]
    public void From_when_the_round_was_a_made_Hawsey_shows_the_Hawsey_bid_and_a_zero_for_the_defence()
    {
        var summary = RoundSummaryViewModel.From
        (
            Scored(new RoundScore(Team.NorthSouth, GameState.HawseyBidAmount, GameState.TricksPerRound, 0, isHawsey: true), us: 24, them: 0),
            isGameOver: false
        );

        Assert.Equal($"Hawsey ({GameState.HawseyBidAmount})", summary.BidText);
        Assert.True(summary.Made);
        Assert.Equal($"+{GameState.HawseyBidAmount}", summary.UsDeltaText);
        Assert.Equal("0", summary.ThemDeltaText);
        Assert.Equal(TableColors.Positive, summary.ThemDeltaColor);
    }



    [Theory]
    [InlineData(true, "US wins the game!")]
    [InlineData(false, "THEM wins the game!")]
    public void From_when_the_game_is_over_names_the_winner(bool usWin, string title)
    {
        var target = HouseRules.Default.PointsToWin;
        var state = Scored
        (
            new RoundScore(Team.NorthSouth, 7, usWin ? 8 : 4, usWin ? 4 : 8, isHawsey: false),
            us: usWin ? target : 0,
            them: usWin ? 0 : target,
            GamePhase.GameOver
        );

        var summary = RoundSummaryViewModel.From(state, isGameOver: true);

        Assert.True(summary.IsGameOver);
        Assert.Equal(title, summary.Title);
        Assert.Equal(usWin, summary.IsTitleUs);
    }



    [Fact]
    public void From_when_no_round_has_been_scored_shows_zeros()
    {
        var summary = RoundSummaryViewModel.From(Scored(null, us: 0, them: 0), isGameOver: false);

        Assert.Equal("US", summary.BidTeamLabel);
        Assert.Equal("0", summary.BidText);
        Assert.False(summary.Made);
        Assert.Equal(0, summary.UsTricks);
        Assert.Equal("0", summary.UsDeltaText);
    }



    [Fact]
    public void From_when_the_state_is_null_throws()
    {
        Assert.Equal("state", Assert.Throws<ArgumentNullException>(() => RoundSummaryViewModel.From(null!, isGameOver: false)).ParamName);
    }
}
