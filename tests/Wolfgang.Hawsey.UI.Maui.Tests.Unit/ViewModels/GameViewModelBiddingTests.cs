using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// The bid overlay: the prompt (who holds the high bid), the bid buttons (the minimum
/// legal bid up to 11, or only the minimum for the stuck dealer), and whether Pass is
/// offered. All read from the engine's bidding progress.
/// </summary>
public class GameViewModelBiddingTests
{
    /// <summary>One AI seat bids <paramref name="amount"/> whenever it would beat the high bid.</summary>
    private sealed class SeatBids(PlayerPosition seat, int amount) : PassingAi
    {
        public override BidAction DecideBid(GameState state, PlayerPosition player) =>
            player == seat && amount > state.HighBid ? new BidAction.NumberBid(amount) : BidAction.PassBid.Instance;
    }



    private static (GameViewModel Vm, GameSession Session) Table(IPlayerStrategy? ai = null)
    {
        var session = TestGameSessions.Unpaced(ai: ai);
        return (new GameViewModel(session, new ImmediateDispatcher()), session);
    }



    /// <summary>
    /// Passes and plays the first legal card until the human is asked to bid in a round
    /// that <paramref name="dealer"/> deals. The human passes every earlier bid, so an AI
    /// always names trump.
    /// </summary>
    private static void PlayUntilHumanBidsWith(GameViewModel vm, GameSession session, PlayerPosition dealer)
    {
        var steps = 0;

        while (!(vm.IsBiddingVisible && session.CurrentState!.Dealer == dealer))
        {
            Assert.True(++steps < 5_000, $"Never reached a {dealer} deal.");

            Assert.False(vm.IsTrumpPickerVisible);

            if (vm.IsBiddingVisible)
            {
                vm.PlaceBidCommand.Execute("pass");
            }
            else if (vm.IsRoundSummaryVisible)
            {
                vm.NextRoundCommand.Execute(null);
            }
            else
            {
                vm.PlayCardCommand.Execute(vm.HumanCards.First(c => c.IsLegal));
            }
        }
    }



    [Fact]
    public void Before_any_bid_the_overlay_offers_the_minimum_up_to_eleven()
    {
        var (vm, _) = Table();

        vm.NewGameCommand.Execute(null);

        Assert.Equal("No bids yet. Minimum: 6", vm.BidPrompt);
        Assert.Equal(["6", "7", "8", "9", "10", "11"], vm.BidOptions);
        Assert.True(vm.CanPass);
    }



    [Fact]
    public void After_an_opponent_bids_the_overlay_names_them_and_offers_only_higher_bids()
    {
        var (vm, _) = Table(new SeatBids(PlayerPosition.East, 8));

        vm.NewGameCommand.Execute(null);

        Assert.Equal("Current high bid: 8 by East (opponents)", vm.BidPrompt);
        Assert.Equal(["9", "10", "11"], vm.BidOptions);
        Assert.True(vm.CanPass);
    }



    [Fact]
    public void A_bid_that_does_not_beat_the_high_bid_is_ignored()
    {
        var (vm, session) = Table(new SeatBids(PlayerPosition.East, 8));
        vm.NewGameCommand.Execute(null);
        var before = session.CurrentState;

        vm.PlaceBidCommand.Execute("7");

        Assert.Same(before, session.CurrentState);
        Assert.True(vm.IsBiddingVisible);
    }



    [Fact]
    public void A_partners_bid_is_shown_as_the_partners()
    {
        // In the third deal South deals, so North bids before the human.
        var (vm, session) = Table(new SeatBids(PlayerPosition.North, 7));
        vm.NewGameCommand.Execute(null);

        PlayUntilHumanBidsWith(vm, session, PlayerPosition.South);

        Assert.Equal("Current high bid: 7 by your partner", vm.BidPrompt);
        Assert.Equal(["8", "9", "10", "11"], vm.BidOptions);
    }



    [Fact]
    public void The_stuck_dealer_may_only_bid_the_minimum_or_call_Hawsey()
    {
        var (vm, session) = Table();
        vm.NewGameCommand.Execute(null);

        PlayUntilHumanBidsWith(vm, session, PlayerPosition.South);

        Assert.Equal("You're stuck as dealer — bid 6 or call Hawsey.", vm.BidPrompt);
        Assert.Equal(["6"], vm.BidOptions);
        Assert.False(vm.CanPass);

        vm.PlaceBidCommand.Execute("6");

        Assert.True(vm.IsTrumpPickerVisible);
    }
}
