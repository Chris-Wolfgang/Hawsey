using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
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



    private static void RunSystematicTest(Func<Task> scenario)
    {
        var configuration = Configuration
            .Create()
            .WithTestingIterations(Iterations)
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
}
