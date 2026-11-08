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
/// legal bid up to the engine's maximum, or only the minimum for the stuck dealer), and whether Pass is
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



    /// <summary>The bids from <paramref name="lowest"/> up to the engine's maximum, as the panel lists them.</summary>
    private static string[] BidsFrom(int lowest) =>
        Enumerable
            .Range(lowest, GameState.MaximumBid - lowest + 1)
            .Select(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();



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
    public void BidOptions_when_nobody_has_bid_runs_from_the_minimum_to_the_engines_maximum()
    {
        var (vm, _) = Table();

        vm.NewGameCommand.Execute(null);

        Assert.Equal("No bids yet. Minimum: 6", vm.BidPrompt);
        Assert.Equal(BidsFrom(6), vm.BidOptions);
        Assert.True(vm.CanPass);
    }



    [Fact]
    public void BidPrompt_when_an_opponent_has_bid_names_them_and_offers_only_higher_bids()
    {
        var (vm, _) = Table(new SeatBids(PlayerPosition.East, 8));

        vm.NewGameCommand.Execute(null);

        Assert.Equal("Current high bid: 8 by East (opponents)", vm.BidPrompt);
        Assert.Equal(BidsFrom(9), vm.BidOptions);
        Assert.True(vm.CanPass);
    }



    [Fact]
    public void PlaceBidCommand_when_the_bid_does_not_beat_the_high_bid_is_ignored()
    {
        var (vm, session) = Table(new SeatBids(PlayerPosition.East, 8));
        vm.NewGameCommand.Execute(null);
        var before = session.CurrentState;

        vm.PlaceBidCommand.Execute("7");

        Assert.Same(before, session.CurrentState);
        Assert.True(vm.IsBiddingVisible);
    }



    [Fact]
    public void BidPrompt_when_the_partner_has_bid_names_the_partner()
    {
        // In the third deal South deals, so North bids before the human.
        var (vm, session) = Table(new SeatBids(PlayerPosition.North, 7));
        vm.NewGameCommand.Execute(null);

        PlayUntilHumanBidsWith(vm, session, PlayerPosition.South);

        Assert.Equal("Current high bid: 7 by your partner", vm.BidPrompt);
        Assert.Equal(BidsFrom(8), vm.BidOptions);
    }



    [Fact]
    public void BidOptions_when_the_human_is_the_stuck_dealer_offers_only_the_minimum_or_Hawsey()
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
