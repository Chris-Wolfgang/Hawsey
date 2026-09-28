using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// A command whose move throws reports the failure in the status line; it used to be
/// lost with the command's discarded task.
/// </summary>
public class GameViewModelCommandErrorTests
{
    /// <summary>An AI that fails the moment it has to bid.</summary>
    private sealed class FailingAi : PassingAi
    {
        public override BidAction DecideBid(GameState state, PlayerPosition player) =>
            throw new InvalidOperationException("The AI failed.");
    }



    [Fact]
    public void A_command_that_throws_reports_the_failure_in_the_status()
    {
        var vm = new GameViewModel(TestGameSessions.Unpaced(ai: new FailingAi()), new ImmediateDispatcher());

        vm.NewGameCommand.Execute(null);

        Assert.Equal("Something went wrong: The AI failed.", vm.StatusMessage);
        Assert.False(vm.IsBiddingVisible);
    }
}
