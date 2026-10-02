using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Concurrency;

/// <summary>
/// Coyote systematic tests: each scenario is run under many controlled task
/// interleavings (every <c>Task.Delay</c> and <c>Task.Run</c> becomes a
/// scheduling point), and any exception or failed assertion in any
/// interleaving is a bug, reported with a reproducible schedule.
/// </summary>
[Trait("Category", "Concurrency")]
public class GameSessionConcurrencyTests
{
    /// <summary>Iterations per test; the weekly Coyote workflow raises this.</summary>
    private static uint Iterations =>
        uint.TryParse(Environment.GetEnvironmentVariable("HAWSEY_COYOTE_ITERATIONS"), out var n) && n > 0 ? n : 200;



    /// <summary>
    /// Runs <paramref name="scenario"/> under Coyote. A long scenario (a whole game)
    /// passes a <paramref name="costFactor"/> so it gets a proportionally smaller share
    /// of the iteration budget and stays quick on every PR run.
    /// </summary>
    /// <remarks>
    /// The iterations run in batches, each in a new <see cref="TestingEngine"/>. An engine
    /// keeps data for every iteration it explores until it is disposed (about 0.5 MB per
    /// whole game), so one engine for the monthly soak's budget ran a GitHub runner out of
    /// memory (#943). Measured with a forced collection after each disposed engine, the live
    /// heap stays flat from batch to batch: the growth is the engine's, not a leak.
    /// </remarks>
    private static void RunSystematicTest(Func<Task> scenario, uint costFactor = 1)
    {
        const uint IterationsPerEngine = 5000;

        var batch = Math.Max(1u, IterationsPerEngine / costFactor);
        var remaining = Math.Max(10u, Iterations / costFactor);

        while (remaining > 0)
        {
            var iterations = Math.Min(batch, remaining);
            remaining -= iterations;

            var configuration = Configuration
                .Create()
                .WithTestingIterations(iterations)
                .WithMaxSchedulingSteps(5000);
            using var engine = TestingEngine.Create(configuration, scenario);

            engine.Run();

            Assert.True
            (
                engine.TestReport.NumOfFoundBugs == 0,
                string.Join(Environment.NewLine, engine.TestReport.BugReports)
            );
        }
    }



    /// <summary>
    /// Drives a fresh game until it is South's (the human's) turn to play a card and
    /// South's card will not complete the trick, passing every human bid and naming
    /// hearts if asked. The AI bids, so the leader varies: when South plays last and
    /// wins, South leads again, and a second tap would be a legitimate move rather than
    /// a stale one. Such turns are played (first legal card) and skipped.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game never reaches South's turn within the step limit.
    /// </exception>
    private static async Task<GameSession> ReachHumanCardTurnAsync()
    {
        var service = new GameSession();
        service.StartNewGame();

        // Every line here runs on a passing run (the test assembly is held to 100% line
        // coverage): the step limit is an assertion inside the loop rather than a throw
        // after it, and the phases that can't occur are left to the default case.
        var steps = 0;

        while (true)
        {
            Assert.True(++steps <= 200, "Never reached South's turn to play.");

            // Deals the next round if the last one has just been scored, and does nothing
            // otherwise. Called on every step, not in a case of its own: a round only
            // ends here in the rare schedule where South never gets an early turn, and a
            // line that runs only in some schedules would make coverage flaky.
            service.StartNextRound();
            var state = service.CurrentState!;

            switch (state.Phase)
            {
                case GamePhase.Bidding:
                    if (await service.AdvanceAiBiddingAsync())
                    {
                        service.PlaceHumanBid(BidAction.PassBid.Instance);
                    }

                    break;
                case GamePhase.TrumpSelection:
                    // South passed, and North deals the first round, so an AI seat
                    // names trump: South never has to.
                    Assert.False(await service.HandleTrumpSelectionAsync());
                    break;
                case GamePhase.TrickPlay:
                    if (await service.AdvanceAiPlaysAsync())
                    {
                        // Four-card tricks: the AI never calls Hawsey.
                        if (service.CurrentState!.CurrentTrick!.Plays.Count < 3)
                        {
                            return service;
                        }

                        service.PlayHumanCard(service.CurrentState.GetLegalPlays()[0]);
                    }

                    break;

                // No Hawsey exchange (the AI never bids Hawsey and South always passes),
                // and a scored round is dealt at the top of the loop.
            }
        }
    }



    [Fact]
    public void Human_double_tap_plays_exactly_one_card()
    {
        RunSystematicTest(async () =>
        {
            var service = await ReachHumanCardTurnAsync();
            var handBefore = service.CurrentState!.Hands[GameSession.HumanPosition].Count;
            var legal = service.CurrentState.GetLegalPlays();

            // Two taps racing each other, as when the second tap lands before the
            // UI has refreshed the hand.
            var first = Task.Run(() => service.PlayHumanCard(legal[0]));
            var second = Task.Run(() => service.PlayHumanCard(legal[legal.Count - 1]));
            await Task.WhenAll(first, second);

            var handAfter = service.CurrentState.Hands[GameSession.HumanPosition].Count;
            Assert.Equal(handBefore - 1, handAfter);
        });
    }



    [Fact]
    public void New_game_during_an_AI_turn_does_not_let_the_old_loop_touch_the_new_game()
    {
        RunSystematicTest(async () =>
        {
            var service = new GameSession();
            service.StartNewGame();

            // Scenario: the first game's AI bidding loop is sleeping when New Game is
            // pressed, and the new game starts its own loop. The two loops must never
            // advance the same bidding round.
            var oldLoop = Task.Run(service.AdvanceAiBiddingAsync);
            var restart = Task.Run(async () =>
            {
                service.StartNewGame();
                await service.AdvanceAiBiddingAsync();
            });

            await Task.WhenAll(oldLoop, restart);

            var state = service.CurrentState!;
            Assert.True
            (
                state.Phase != GamePhase.Bidding || state.NextToAct == GameSession.HumanPosition,
                $"Bidding stopped at {state.NextToAct} instead of waiting for the human."
            );
        });
    }



    [Fact]
    public void Full_game_reports_every_trick_winner_and_announces_game_over_exactly_once()
    {
        RunSystematicTest(async () =>
        {
            var service = new GameSession();
            var wrongTrickWinners = 0;
            var gameOverWinners = new List<Team>();

            // Each event is raised right after the transition that caused it, so the
            // reported winner must be the winner of the trick the state just recorded.
            service.TrickCompleted += (_, e) =>
            {
                var tricks = service.CurrentState!.CompletedTricks;
                wrongTrickWinners += tricks.Count == 0 || tricks[tricks.Count - 1].Winner != e.Winner ? 1 : 0;
            };
            service.GameOver += (_, e) => gameOverWinners.Add(e.Winner);

            service.StartNewGame();
            await PlayToGameOverAsync(service);

            var state = service.CurrentState!;
            Assert.Equal(0, wrongTrickWinners);
            var winner = Assert.Single(gameOverWinners);
            var winnerScore = winner == Team.NorthSouth ? state.NorthSouthScore : state.EastWestScore;
            Assert.True(winnerScore >= state.Rules.PointsToWin, $"{winner} was announced with {winnerScore} points.");
        }, costFactor: 20);
    }



    /// <summary>
    /// Plays the human seat automatically (passes every bid, names hearts, plays the
    /// first legal card) until the game is over.
    /// </summary>
    private static async Task PlayToGameOverAsync(GameSession service)
    {
        var steps = 0;

        while (true)
        {
            Assert.True(++steps <= 5000, "The game did not finish.");
            var state = service.CurrentState!;

            switch (state.Phase)
            {
                case GamePhase.GameOver:
                    return;
                case GamePhase.Bidding:
                    if (await service.AdvanceAiBiddingAsync())
                    {
                        service.PlaceHumanBid(BidAction.PassBid.Instance);
                    }

                    break;
                case GamePhase.TrumpSelection:
                    // South names hearts only when stuck as dealer, which some games never
                    // reach: one statement, so the coverage of this line doesn't depend on
                    // the schedule Coyote explores.
                    _ = await service.HandleTrumpSelectionAsync() && service.SelectTrump(Suit.Hearts);
                    break;
                case GamePhase.TrickPlay:
                    if (await service.AdvanceAiPlaysAsync())
                    {
                        service.PlayHumanCard(service.CurrentState!.GetLegalPlays()[0]);
                    }

                    break;
                default:
                    // Round scored. (No Hawsey exchange: the AI never bids Hawsey and
                    // South always passes.)
                    service.StartNextRound();
                    break;
            }
        }
    }
}
