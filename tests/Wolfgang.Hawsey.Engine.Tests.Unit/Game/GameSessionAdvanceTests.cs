using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameSession.AdvanceAsync"/>: the loop every UI runs after each human
/// move. It plays the AI seats until it must wait, and says what for.
/// </summary>
public class GameSessionAdvanceTests
{
    private readonly GameSession _session = TestGameSessions.Unpaced();



    [Fact]
    public async Task Before_a_game_starts_it_waits_for_nothing()
    {
        Assert.Equal(WaitingFor.Nothing, await _session.AdvanceAsync());
    }



    [Fact]
    public async Task A_new_game_waits_for_the_humans_bid()
    {
        _session.StartNewGame();

        Assert.Equal(WaitingFor.HumanBid, await _session.AdvanceAsync());
        Assert.Equal(GameSession.HumanPosition, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public async Task When_the_human_wins_the_bid_it_waits_for_trump()
    {
        _session.StartNewGame();
        await _session.AdvanceAsync();
        Assert.True(_session.PlaceHumanBid(new BidAction.NumberBid(8)));

        Assert.Equal(WaitingFor.HumanTrump, await _session.AdvanceAsync());
    }



    [Fact]
    public async Task After_a_human_Hawsey_bid_and_trump_it_waits_for_the_exchange()
    {
        _session.StartNewGame();
        await _session.AdvanceAsync();
        Assert.True(_session.PlaceHumanBid(BidAction.HawseyBid.Instance));
        Assert.Equal(WaitingFor.HumanTrump, await _session.AdvanceAsync());
        Assert.True(_session.SelectTrump(Suit.Spades));

        Assert.Equal(WaitingFor.HumanHawseyExchange, await _session.AdvanceAsync());
    }



    [Fact]
    public async Task A_round_asks_for_every_card_but_the_forced_last_one_then_waits_for_the_next_round()
    {
        _session.StartNewGame();
        await _session.AdvanceAsync();
        Assert.True(_session.PlaceHumanBid(BidAction.PassBid.Instance));

        var cardsAskedFor = 0;
        WaitingFor waiting;

        while ((waiting = await _session.AdvanceAsync()) == WaitingFor.HumanCard)
        {
            // Never asked for the last card: there is no choice to make.
            Assert.True(_session.CurrentState!.Hands[GameSession.HumanPosition].Count > 1);
            Assert.True(_session.PlayHumanCard(_session.CurrentState.GetLegalPlays()[0]));
            cardsAskedFor++;
        }

        Assert.Equal(WaitingFor.NextRound, waiting);
        Assert.Equal(11, cardsAskedFor);
        Assert.Equal(GamePhase.RoundScoring, _session.CurrentState!.Phase);
        Assert.Empty(_session.CurrentState.Hands[GameSession.HumanPosition]);
    }



    [Fact]
    public async Task At_game_over_it_waits_for_nothing_more_but_says_so()
    {
        // A one-point target: the first round ends the game.
        _session.StartNewGame(new HouseRules { PointsToWin = 1 });
        await _session.AdvanceAsync();
        Assert.True(_session.PlaceHumanBid(BidAction.PassBid.Instance));

        WaitingFor waiting;

        while ((waiting = await _session.AdvanceAsync()) == WaitingFor.HumanCard)
        {
            Assert.True(_session.PlayHumanCard(_session.CurrentState!.GetLegalPlays()[0]));
        }

        Assert.Equal(WaitingFor.GameOver, waiting);
        Assert.Equal(GamePhase.GameOver, _session.CurrentState!.Phase);
    }



    [Fact]
    public async Task A_new_game_during_an_AI_pause_leaves_the_old_advance_waiting_for_nothing()
    {
        var paced = TestGameSessions.Paced();
        paced.StartNewGame();

        var advancing = paced.AdvanceAsync();
        Assert.False(advancing.IsCompleted, "The AI step should be paused.");
        paced.StartNewGame();

        Assert.Equal(WaitingFor.Nothing, await advancing);
        Assert.Equal(PlayerPosition.East, paced.CurrentState!.NextToAct);
    }
}
