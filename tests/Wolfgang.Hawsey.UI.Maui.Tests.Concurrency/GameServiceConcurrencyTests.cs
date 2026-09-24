using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.UI.Maui.Services;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Concurrency;

/// <summary>
/// Coyote systematic tests: each scenario is run under many controlled task
/// interleavings (every <c>Task.Delay</c> and <c>Task.Run</c> becomes a
/// scheduling point), and any exception or failed assertion in any
/// interleaving is a bug, reported with a reproducible schedule.
/// </summary>
[Trait("Category", "Concurrency")]
public class GameServiceConcurrencyTests
{
    /// <summary>Iterations per test; the weekly Coyote workflow raises this.</summary>
    private static uint Iterations =>
        uint.TryParse(Environment.GetEnvironmentVariable("HAWSEY_COYOTE_ITERATIONS"), out var n) && n > 0 ? n : 200;



    /// <summary>
    /// Runs <paramref name="scenario"/> under Coyote. A long scenario (a whole game)
    /// passes a <paramref name="costFactor"/> so it gets a proportionally smaller share
    /// of the iteration budget and stays quick on every PR run.
    /// </summary>
    private static void RunSystematicTest(Func<Task> scenario, uint costFactor = 1)
    {
        var configuration = Configuration
            .Create()
            .WithTestingIterations(Math.Max(10u, Iterations / costFactor))
            .WithMaxSchedulingSteps(5000);
        using var engine = TestingEngine.Create(configuration, scenario);

        engine.Run();

        Assert.True
        (
            engine.TestReport.NumOfFoundBugs == 0,
            string.Join(Environment.NewLine, engine.TestReport.BugReports)
        );
    }



    /// <summary>
    /// Drives a fresh game until it is South's (the human's) turn to play a card,
    /// passing every human bid and naming hearts if asked.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game never reaches South's turn within the step limit.
    /// </exception>
    private static async Task<GameService> ReachHumanCardTurnAsync()
    {
        var service = new GameService();
        service.StartNewGame();

        for (var guard = 0; guard < 200; guard++)
        {
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
                    if (await service.HandleTrumpSelectionAsync())
                    {
                        service.SelectTrump(Suit.Hearts);
                    }

                    break;
                case GamePhase.HawseyExchange:
                    await service.HandleHawseyExchangeAsync();
                    break;
                case GamePhase.TrickPlay:
                    if (await service.AdvanceAiPlaysAsync())
                    {
                        return service;
                    }

                    break;
                default:
                    service.StartNextRound();
                    break;
            }
        }

        throw new InvalidOperationException("Never reached South's turn to play.");
    }



    [Fact]
    public void Human_double_tap_plays_exactly_one_card()
    {
        RunSystematicTest(async () =>
        {
            var service = await ReachHumanCardTurnAsync();
            var handBefore = service.CurrentState!.Hands[GameService.HumanPosition].Count;
            var legal = service.CurrentState.GetLegalPlays();

            // Two taps racing each other, as when the second tap lands before the
            // UI has refreshed the hand.
            var first = Task.Run(() => service.PlayHumanCard(legal[0]));
            var second = Task.Run(() => service.PlayHumanCard(legal[legal.Count - 1]));
            await Task.WhenAll(first, second);

            var handAfter = service.CurrentState.Hands[GameService.HumanPosition].Count;
            Assert.Equal(handBefore - 1, handAfter);
        });
    }



    [Fact]
    public void New_game_during_an_AI_turn_does_not_let_the_old_loop_touch_the_new_game()
    {
        RunSystematicTest(async () =>
        {
            var service = new GameService();
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
                state.Phase != GamePhase.Bidding || state.NextToAct == GameService.HumanPosition,
                $"Bidding stopped at {state.NextToAct} instead of waiting for the human."
            );
        });
    }



    [Fact]
    public void Full_game_reports_every_trick_winner_and_announces_game_over_exactly_once()
    {
        RunSystematicTest(async () =>
        {
            var service = new GameService();
            var wrongTrickWinners = 0;
            var gameOverWinners = new List<Team>();

            // Each event is raised right after the transition that caused it, so the
            // reported winner must be the winner of the trick the state just recorded.
            service.TrickCompleted += (_, e) =>
            {
                var tricks = service.CurrentState!.CompletedTricks;
                if (tricks.Count == 0 || tricks[tricks.Count - 1].Winner != e.Winner)
                {
                    wrongTrickWinners++;
                }
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the game does not finish within the step limit.
    /// </exception>
    private static async Task PlayToGameOverAsync(GameService service)
    {
        for (var guard = 0; guard < 5000; guard++)
        {
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
                    if (await service.HandleTrumpSelectionAsync())
                    {
                        service.SelectTrump(Suit.Hearts);
                    }

                    break;
                case GamePhase.HawseyExchange:
                    await service.HandleHawseyExchangeAsync();
                    break;
                case GamePhase.TrickPlay:
                    if (await service.AdvanceAiPlaysAsync())
                    {
                        service.PlayHumanCard(service.CurrentState!.GetLegalPlays()[0]);
                    }

                    break;
                default:
                    service.StartNextRound();
                    break;
            }
        }

        throw new InvalidOperationException("The game did not finish.");
    }
}
