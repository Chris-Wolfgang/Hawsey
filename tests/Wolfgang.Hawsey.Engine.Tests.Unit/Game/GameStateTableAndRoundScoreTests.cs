using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// What the engine tells a UI to show: <see cref="GameState.TableCards"/> and
/// <see cref="GameState.TableWinningCard"/> (the trick on the table, including the one
/// just won), and <see cref="GameState.RoundScore"/> (the round just scored).
/// </summary>
public class GameStateTableAndRoundScoreTests
{
    private readonly GameEngine _engine = new();
    private readonly SimpleAiStrategy _ai = new();



    private GameState Deal() => _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(3));



    /// <summary>Bids and names trump with the AI, stopping at the first lead.</summary>
    private GameState AtFirstLead()
    {
        var state = Deal();

        while (state.Phase != GamePhase.TrickPlay)
        {
            var player = state.NextToAct!.Value;
            state = state.Phase == GamePhase.Bidding
                ? _engine.PlaceBid(state, player, _ai.DecideBid(state, player))
                : _engine.SelectTrump(state, _ai.DecideTrump(state, player));
        }

        return state;
    }



    private GameState Play(GameState state)
    {
        var player = state.NextToAct!.Value;
        return _engine.PlayCard(state, player, _ai.DecidePlay(state, player));
    }



    [Fact]
    public void Before_the_first_lead_the_table_is_empty()
    {
        var state = AtFirstLead();

        Assert.Empty(state.TableCards);
        Assert.Null(state.TableWinningCard);
        Assert.Null(state.RoundScore);
    }



    [Fact]
    public void During_a_trick_the_table_shows_its_cards_and_current_winner()
    {
        var state = Play(Play(AtFirstLead()));

        Assert.Equal(state.CurrentTrick!.Plays, state.TableCards);
        Assert.Equal(state.CurrentTrick.GetCurrentWinner(), state.TableWinningCard);
    }



    [Fact]
    public void After_a_trick_is_won_the_table_shows_it_until_the_next_lead()
    {
        var state = AtFirstLead();

        while (state.CompletedTricks.Count == 0)
        {
            state = Play(state);
        }

        var won = state.CompletedTricks[0];
        Assert.Empty(state.CurrentTrick!.Plays);
        Assert.Equal(won.Cards, state.TableCards);
        Assert.Equal(won.Cards.Single(p => p.Player == won.Winner).Card, state.TableWinningCard);

        state = Play(state);

        Assert.Single(state.TableCards);
    }



    [Fact]
    public void A_scored_round_carries_its_score_and_shows_the_last_trick()
    {
        var dealt = Deal();

        var scored = RoundDriver.PlayOneRound(dealt, _ai);

        var score = scored.RoundScore!;
        Assert.Equal(GamePhase.RoundScoring, scored.Phase);
        Assert.Null(scored.CurrentTrick);
        Assert.Equal(scored.CompletedTricks[scored.CompletedTricks.Count - 1].Cards, scored.TableCards);
        Assert.Equal(scored.BiddingResult!.Winner.GetTeam(), score.BiddingTeam);
        Assert.Equal(dealt.NorthSouthScore + score.DeltaFor(Team.NorthSouth), scored.NorthSouthScore);
        Assert.Equal(dealt.EastWestScore + score.DeltaFor(Team.EastWest), scored.EastWestScore);
        Assert.Equal
        (
            scored.CompletedTricks.Count(t => t.Winner.GetTeam() == Team.NorthSouth),
            score.TricksFor(Team.NorthSouth)
        );
        Assert.Equal
        (
            scored.CompletedTricks.Count(t => t.Winner.GetTeam() == Team.EastWest),
            score.TricksFor(Team.EastWest)
        );
    }



    [Fact]
    public void The_next_deal_has_no_round_score_and_an_empty_table()
    {
        var next = _engine.StartNextRound(RoundDriver.PlayOneRound(Deal(), _ai), new Random(4));

        Assert.Null(next.RoundScore);
        Assert.Empty(next.TableCards);
        Assert.Null(next.TableWinningCard);
    }
}
