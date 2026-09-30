using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameSession.TableCards"/> and <see cref="GameSession.TableWinningCard"/>:
/// what the table shows. When the human wins a trick and is to lead, the won trick
/// shows for a moment and then clears, so the human leads to an empty table.
/// </summary>
public class GameSessionTableTests
{
    /// <summary>
    /// With seed 25, West bids 6 and leads the first trick; North and East play low; the
    /// human plays last and the jack of diamonds wins the trick.
    /// </summary>
    private const int HumanWinsTheFirstTrickSeed = 25;

    private static readonly Card WinningJack = new(Rank.Jack, Suit.Diamonds);



    /// <summary>West opens the bidding at 6; everyone else passes.</summary>
    private sealed class WestBidsSix : PassingAi
    {
        public override BidAction DecideBid(GameState state, PlayerPosition player) =>
            player == PlayerPosition.West && state.HighBid == 0 ? new BidAction.NumberBid(6) : BidAction.PassBid.Instance;
    }



    /// <summary>Plays to the human's last card of the first trick, and plays the winning jack.</summary>
    private static async Task HumanWinsTheFirstTrickAsync(GameSession session)
    {
        session.StartNewGame();
        Assert.Equal(WaitingFor.HumanBid, await session.AdvanceAsync());
        Assert.True(session.PlaceHumanBid(BidAction.PassBid.Instance));
        Assert.Equal(WaitingFor.HumanCard, await session.AdvanceAsync());
        Assert.Equal(3, session.CurrentState!.CurrentTrick!.Plays.Count);
        Assert.True(session.PlayHumanCard(WinningJack));
        Assert.Equal(PlayerPosition.South, session.CurrentState.CompletedTricks[0].Winner);
    }



    [Fact]
    public void Before_a_game_the_table_is_empty()
    {
        var session = TestGameSessions.Unpaced();

        Assert.Empty(session.TableCards);
        Assert.Null(session.TableWinningCard);
    }



    [Fact]
    public async Task During_a_trick_the_table_is_the_trick()
    {
        var session = TestGameSessions.Unpaced(HumanWinsTheFirstTrickSeed, new WestBidsSix());
        session.StartNewGame();
        await session.AdvanceAsync();
        session.PlaceHumanBid(BidAction.PassBid.Instance);
        await session.AdvanceAsync();

        var state = session.CurrentState!;
        Assert.Equal(state.TableCards, session.TableCards);
        Assert.Equal(3, session.TableCards.Count);
        Assert.Equal(state.TableWinningCard, session.TableWinningCard);
    }



    [Fact]
    public async Task A_trick_the_human_wins_shows_then_clears_before_the_human_leads()
    {
        var session = TestGameSessions.Unpaced(HumanWinsTheFirstTrickSeed, new WestBidsSix());
        await HumanWinsTheFirstTrickAsync(session);

        // The won trick shows until the session's loop runs.
        Assert.Equal(4, session.TableCards.Count);
        Assert.Equal(WinningJack, session.TableWinningCard);

        var changes = 0;
        session.StateChanged += (_, _) => changes++;

        Assert.Equal(WaitingFor.HumanCard, await session.AdvanceAsync());

        Assert.Equal(1, changes);
        Assert.Empty(session.TableCards);
        Assert.Null(session.TableWinningCard);
        Assert.Equal(4, session.CurrentState!.TableCards.Count);
        Assert.Equal(GameSession.HumanPosition, session.CurrentState.NextToAct);

        // The lead shows on the cleared table.
        var lead = session.CurrentState.GetLegalPlays()[0];
        Assert.True(session.PlayHumanCard(lead));

        Assert.Equal(lead, Assert.Single(session.TableCards).Card);
    }



    [Fact]
    public async Task Whenever_the_human_leads_after_a_trick_the_table_is_empty()
    {
        // The AI seats always pass, so the human, as the stuck dealer, also names trump.
        var session = TestGameSessions.Unpaced();
        session.StartNewGame();
        var leadsAfterATrick = 0;
        var steps = 0;
        WaitingFor waiting;

        while ((waiting = await session.AdvanceAsync()) != WaitingFor.GameOver)
        {
            Assert.True(++steps < 5_000, "The game did not finish.");
            var state = session.CurrentState!;

            switch (waiting)
            {
                case WaitingFor.HumanBid:
                    // The stuck dealer may not pass.
                    session.PlaceHumanBid
                    (
                        state.IsNextBidderStuck ? new BidAction.NumberBid(state.MinimumLegalBid) : BidAction.PassBid.Instance
                    );
                    break;

                case WaitingFor.HumanTrump:
                    session.SelectTrump(Suit.Spades);
                    break;

                case WaitingFor.NextRound:
                    session.StartNextRound();
                    Assert.Empty(session.TableCards);
                    break;

                default:
                    if (state.CurrentTrick is not { Plays.Count: > 0 } && state.CompletedTricks.Count > 0)
                    {
                        Assert.Empty(session.TableCards);
                        leadsAfterATrick++;
                    }

                    session.PlayHumanCard(state.GetLegalPlays()[0]);
                    break;
            }
        }

        Assert.True(leadsAfterATrick > 0, "The human never won a trick.");
    }



    [Fact]
    public async Task A_new_game_during_the_pause_before_clearing_stops_the_loop()
    {
        var session = TestGameSessions.Paced(HumanWinsTheFirstTrickSeed, new WestBidsSix());
        await HumanWinsTheFirstTrickAsync(session);

        var playing = session.AdvanceAiPlaysAsync();
        Assert.False(playing.IsCompleted, "The loop should be paused before it clears the table.");
        var changes = 0;
        session.StateChanged += (_, _) => changes++;
        session.StartNewGame();

        Assert.False(await playing);

        Assert.Equal(1, changes);
        Assert.Equal(GamePhase.Bidding, session.CurrentState!.Phase);
    }



    [Fact]
    public async Task A_lead_during_the_pause_before_clearing_is_answered_instead()
    {
        var session = TestGameSessions.Paced(HumanWinsTheFirstTrickSeed, new WestBidsSix());
        await HumanWinsTheFirstTrickAsync(session);

        var playing = session.AdvanceAiPlaysAsync();
        Assert.False(playing.IsCompleted, "The loop should be paused before it clears the table.");
        var lead = session.CurrentState!.GetLegalPlays()[0];
        Assert.True(session.PlayHumanCard(lead));

        await playing;

        // The loop did not clear the lead away: it played the AI seats' answers to it.
        var state = session.CurrentState!;
        var secondTrick = state.CompletedTricks.Count > 1 ? state.CompletedTricks[1].Cards : state.CurrentTrick!.Plays;
        Assert.Equal(lead, secondTrick[0].Card);
        Assert.True(secondTrick.Count > 1);
    }
}
