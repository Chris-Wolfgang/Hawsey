using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameSession"/>'s events and rules handling: one
/// <see cref="GameSession.StateChanged"/> per move (a UI redraws on it), the trick
/// winner it reports, the rules a game starts with, and the AI lines
/// <see cref="SimpleAiStrategy"/> never takes (an AI Hawsey bid, found with
/// an injected strategy).
/// </summary>
public class GameSessionEventTests
{
    private static int CountStateChanges(GameSession session, Action move)
    {
        var count = 0;
        EventHandler handler = (_, _) => count++;
        session.StateChanged += handler;
        move();
        session.StateChanged -= handler;
        return count;
    }



    private static async Task<(int Changes, bool Result)> CountStateChangesAsync(GameSession session, Func<Task<bool>> move)
    {
        var count = 0;
        EventHandler handler = (_, _) => count++;
        session.StateChanged += handler;
        var result = await move();
        session.StateChanged -= handler;
        return (count, result);
    }



    [Fact]
    public void StartNewGame_without_rules_uses_the_default_rules()
    {
        var session = TestGameSessions.Unpaced();

        session.StartNewGame();

        Assert.Same(HouseRules.Default, session.CurrentState!.Rules);
    }



    [Fact]
    public void StartNewGame_with_rules_uses_them()
    {
        var session = TestGameSessions.Unpaced();
        var rules = new HouseRules { PointsToWin = 30 };

        session.StartNewGame(rules);

        Assert.Same(rules, session.CurrentState!.Rules);
    }



    [Fact]
    public async Task Every_move_raises_StateChanged_exactly_once()
    {
        var session = TestGameSessions.Unpaced();

        Assert.Equal(1, CountStateChanges(session, () => session.StartNewGame()));

        // East (the first bidder) is the only AI seat before the human.
        Assert.Equal((1, true), await CountStateChangesAsync(session, session.AdvanceAiBiddingAsync));
        Assert.Equal(1, CountStateChanges(session, () => Assert.True(session.PlaceHumanBid(new BidAction.NumberBid(7)))));

        // West and North pass; the human won the bid and names trump.
        Assert.Equal((2, false), await CountStateChangesAsync(session, session.AdvanceAiBiddingAsync));
        Assert.Equal(1, CountStateChanges(session, () => Assert.True(session.SelectTrump(Suit.Spades))));

        // South leads.
        Assert.Equal(1, CountStateChanges(session, () => Assert.True(session.PlayHumanCard(session.CurrentState!.GetLegalPlays()[0]))));

        // West, North and East complete the trick; the human leads the next one
        // only if South won, so count the AI plays rather than assume who leads.
        var aiPlays = 0;
        EventHandler counter = (_, _) => aiPlays++;
        session.StateChanged += counter;
        await session.AdvanceAiPlaysAsync();
        session.StateChanged -= counter;
        Assert.InRange(aiPlays, 3, 6);
    }



    [Fact]
    public async Task AI_trump_the_human_Hawsey_exchange_and_the_next_deal_each_raise_StateChanged_once()
    {
        // Everyone passes: the AI dealer (North) is stuck and names trump.
        var stuck = TestGameSessions.Unpaced();
        stuck.StartNewGame();
        Assert.True(await stuck.AdvanceAiBiddingAsync());
        Assert.True(stuck.PlaceHumanBid(BidAction.PassBid.Instance));
        await stuck.AdvanceAiBiddingAsync();
        Assert.Equal((1, false), await CountStateChangesAsync(stuck, stuck.HandleTrumpSelectionAsync));

        // The human calls Hawsey, names trump and exchanges.
        var hawsey = TestGameSessions.Unpaced();
        hawsey.StartNewGame();
        Assert.True(await hawsey.AdvanceAiBiddingAsync());
        Assert.True(hawsey.PlaceHumanBid(BidAction.HawseyBid.Instance));
        Assert.True(hawsey.SelectTrump(Suit.Clubs));
        var hand = hawsey.CurrentState!.Hands[PlayerPosition.South];
        var partner = hawsey.CurrentState.Hands[PlayerPosition.North];
        Assert.Equal(1, CountStateChanges(hawsey, () => Assert.True(hawsey.PerformHawseyExchange([hand[0], hand[1]], [partner[0], partner[1]]))));

        // A scored round: StartNextRound deals once.
        var round = TestGameSessions.Unpaced();
        round.StartNewGame();
        while (round.CurrentState!.Phase != GamePhase.RoundScoring)
        {
            switch (round.CurrentState.Phase)
            {
                case GamePhase.Bidding:
                    if (await round.AdvanceAiBiddingAsync())
                    {
                        round.PlaceHumanBid(BidAction.PassBid.Instance);
                    }

                    break;

                case GamePhase.TrumpSelection:
                    await round.HandleTrumpSelectionAsync();
                    break;

                default:
                    if (await round.AdvanceAiPlaysAsync())
                    {
                        round.PlayHumanCard(round.CurrentState.GetLegalPlays()[0]);
                    }

                    break;
            }
        }

        Assert.NotEmpty(round.Bids);
        Assert.Equal(1, CountStateChanges(round, round.StartNextRound));
        Assert.Equal(GamePhase.Bidding, round.CurrentState.Phase);
        // The next deal starts its own bidding: last round's bids are gone.
        Assert.Empty(round.Bids);
    }



    [Fact]
    public async Task TrickCompleted_reports_the_player_who_won_the_trick()
    {
        var session = TestGameSessions.Unpaced();
        var winners = new List<PlayerPosition>();
        session.TrickCompleted += (_, e) => winners.Add(e.Winner);

        session.StartNewGame();
        Assert.True(await session.AdvanceAiBiddingAsync());
        Assert.True(session.PlaceHumanBid(BidAction.PassBid.Instance));
        await session.AdvanceAiBiddingAsync();
        await session.HandleTrumpSelectionAsync();

        // A whole round, so the winners include seats other than North, the enum's
        // default value, and an event that never set its winner would be caught.
        while (session.CurrentState!.Phase == GamePhase.TrickPlay)
        {
            if (await session.AdvanceAiPlaysAsync())
            {
                Assert.True(session.PlayHumanCard(session.CurrentState!.GetLegalPlays()[0]));
            }
        }

        Assert.Equal(session.CurrentState!.CompletedTricks.Select(t => t.Winner), winners);
        Assert.Contains(winners, w => w != PlayerPosition.North);
    }



    [Fact]
    public async Task An_AI_Hawsey_bid_leads_to_the_AI_exchange()
    {
        // East, the first bidder, calls Hawsey and names hearts; the session makes
        // East's exchange itself.
        var ai = new TestPlayerStrategy(new Queue<BidAction>([BidAction.HawseyBid.Instance]));
        var session = TestGameSessions.Unpaced(ai: ai);
        session.StartNewGame();

        Assert.False(await session.AdvanceAiBiddingAsync());
        Assert.False(await session.HandleTrumpSelectionAsync());
        Assert.Equal(GamePhase.HawseyExchange, session.CurrentState!.Phase);
        Assert.Equal(PlayerPosition.East, session.CurrentState.HawseyBidder);

        var before = session.CurrentState.Hands[PlayerPosition.East].ToList();

        Assert.Equal((1, false), await CountStateChangesAsync(session, session.HandleHawseyExchangeAsync));

        Assert.Equal(GamePhase.TrickPlay, session.CurrentState.Phase);
        Assert.NotEqual(before, session.CurrentState.Hands[PlayerPosition.East]);
        Assert.False(await session.HandleHawseyExchangeAsync());
    }



    [Fact]
    public async Task Whole_games_against_the_real_AI_see_competitive_bids_and_finish()
    {
        // The real SimpleAiStrategy bids on hand strength. The human passes every bid and
        // names hearts when stuck, so each hand's bidding is the AI's. Across these
        // seeded games the AI makes number bids, and every game reaches game over.
        var aiBids = 0;

        for (var seed = 1; seed <= 3; seed++)
        {
            var session = TestGameSessions.Unpaced(seed, ai: new SimpleAiStrategy());
            session.StateChanged += (_, _) =>
            {
                if (session.CurrentState is { Phase: GamePhase.Bidding, HighBid: > 0 })
                {
                    aiBids++;
                }
            };
            session.StartNewGame();
            var steps = 0;

            while (session.CurrentState!.Phase != GamePhase.GameOver)
            {
                Assert.True(++steps < 20_000, "The game did not finish.");

                switch (session.CurrentState.Phase)
                {
                    case GamePhase.Bidding:
                        if (await session.AdvanceAiBiddingAsync())
                        {
                            Assert.True(session.PlaceHumanBid(BidAction.PassBid.Instance));
                        }

                        break;

                    case GamePhase.TrumpSelection:
                        // In these seeded games the human is never the stuck dealer (an
                        // AI always bids), and the AI never calls Hawsey, so an AI always
                        // names trump.
                        Assert.False(await session.HandleTrumpSelectionAsync());
                        break;

                    case GamePhase.RoundScoring:
                        session.StartNextRound();
                        break;

                    default:
                        if (await session.AdvanceAiPlaysAsync())
                        {
                            Assert.True(session.PlayHumanCard(session.CurrentState.GetLegalPlays()[0]));
                        }

                        break;
                }
            }

            Assert.NotNull(session.CurrentState.Winner);
        }

        Assert.True(aiBids > 0, "The AI never made a number bid.");
    }



    [Fact]
    public async Task A_game_can_end_on_an_AI_card_and_the_session_announces_it()
    {
        // Seed 1, a target of 12, the human passing and playing its first legal card:
        // the game ends on an AI's card, so AdvanceAiPlaysAsync raises GameOver.
        var session = TestGameSessions.Unpaced(seed: 1);

        Assert.True(await EndsOnAnAiCard(session));
        Assert.Equal(GamePhase.GameOver, session.CurrentState!.Phase);
    }



    private static async Task<bool> EndsOnAnAiCard(GameSession session)
    {
        // The handler only counts; the loop reads the count around each AI call to see
        // whether that call is the one that ended the game.
        var gameOvers = 0;
        var endedByAi = false;
        session.GameOver += (_, _) => gameOvers++;
        session.StartNewGame(new HouseRules { PointsToWin = 12 });

        while (session.CurrentState!.Phase != GamePhase.GameOver)
        {
            switch (session.CurrentState.Phase)
            {
                case GamePhase.Bidding:
                    if (await session.AdvanceAiBiddingAsync())
                    {
                        session.PlaceHumanBid(BidAction.PassBid.Instance);
                    }

                    break;

                case GamePhase.TrumpSelection:
                    // The game ends before the human deals, so an AI always names trump.
                    Assert.False(await session.HandleTrumpSelectionAsync());
                    break;

                case GamePhase.RoundScoring:
                    session.StartNextRound();
                    break;

                default:
                    var before = gameOvers;
                    var humanPlays = await session.AdvanceAiPlaysAsync();
                    endedByAi |= gameOvers > before;

                    if (humanPlays)
                    {
                        session.PlayHumanCard(session.CurrentState.GetLegalPlays()[0]);
                    }

                    break;
            }
        }

        return endedByAi;
    }
}
