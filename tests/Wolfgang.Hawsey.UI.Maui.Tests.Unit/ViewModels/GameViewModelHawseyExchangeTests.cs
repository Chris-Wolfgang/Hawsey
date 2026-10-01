using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// The human's Hawsey exchange: after bidding Hawsey and naming trump, the human taps
/// two cards to discard and the partner gives their two best.
/// </summary>
public class GameViewModelHawseyExchangeTests
{
    private readonly GameSession _session = TestGameSessions.Unpaced();
    private readonly ImmediateDispatcher _dispatcher = new();
    private readonly GameViewModel _vm;



    public GameViewModelHawseyExchangeTests()
    {
        _vm = new GameViewModel(_session, _dispatcher);
    }



    private void ReachTheExchange()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("hawsey");
        _vm.SelectTrumpCommand.Execute("spades");
    }



    private void Tap(int index) => _vm.PlayCardCommand.Execute(_vm.HumanCards[index]);



    [Fact]
    public void Naming_trump_after_a_Hawsey_bid_opens_the_exchange_with_nothing_picked()
    {
        ReachTheExchange();

        Assert.Equal(GamePhase.HawseyExchange, _session.CurrentState!.Phase);
        Assert.True(_vm.IsHawseyExchangeVisible);
        Assert.Equal("Hawsey! Pick 2 cards to discard.", _vm.StatusMessage);
        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
        Assert.All(_vm.HumanCards, c => Assert.True(c.IsInteractive));
        Assert.All(_vm.HumanCards, c => Assert.False(c.IsSelected));
        Assert.False(_vm.ConfirmHawseyExchangeCommand.CanExecute(null));
    }



    [Fact]
    public void Tapping_picks_at_most_two_cards_and_a_second_tap_unpicks()
    {
        ReachTheExchange();

        Tap(0);
        Tap(1);
        Tap(2);

        Assert.Equal("Selected: 2 / 2", _vm.ExchangeSelectionText);
        Assert.Equal([0, 1], Enumerable.Range(0, 12).Where(i => _vm.HumanCards[i].IsSelected));
        Assert.True(_vm.ConfirmHawseyExchangeCommand.CanExecute(null));

        Tap(0);

        Assert.Equal("Selected: 1 / 2", _vm.ExchangeSelectionText);
        Assert.Equal([1], Enumerable.Range(0, 12).Where(i => _vm.HumanCards[i].IsSelected));
        Assert.False(_vm.ConfirmHawseyExchangeCommand.CanExecute(null));
    }



    [Fact]
    public void Tapping_a_card_no_longer_in_the_hand_picks_nothing()
    {
        ReachTheExchange();

        _vm.PlayCardCommand.Execute(new CardViewModel(new Card(Rank.Ace, Suit.Spades), isLegal: true));

        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
    }



    [Fact]
    public void Confirming_with_fewer_than_two_picked_does_nothing()
    {
        ReachTheExchange();
        Tap(0);

        _vm.ConfirmHawseyExchangeCommand.Execute(null);

        Assert.Equal(GamePhase.HawseyExchange, _session.CurrentState!.Phase);
        Assert.True(_vm.IsHawseyExchangeVisible);
    }



    [Fact]
    public void Confirming_discards_the_picks_takes_the_partners_best_and_plays_on()
    {
        ReachTheExchange();
        var state = _session.CurrentState!;
        var hand = state.Hands[GameSession.HumanPosition];
        new SimpleAiStrategy().DecideHawseyExchange(state, GameSession.HumanPosition, out _, out var partnersBest);
        Tap(0);
        Tap(1);

        _vm.ConfirmHawseyExchangeCommand.Execute(null);

        var after = _session.CurrentState!;
        var newHand = after.Hands[GameSession.HumanPosition];
        Assert.False(_vm.IsHawseyExchangeVisible);
        Assert.Equal(GamePhase.TrickPlay, after.Phase);
        Assert.All(partnersBest, c => Assert.Contains(c, newHand));
        Assert.Equal(hand.Count, newHand.Count);
        Assert.All(_vm.HumanCards, c => Assert.False(c.IsSelected));
        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
        Assert.Equal("Your turn. Play a card.", _vm.StatusMessage);
    }



    [Fact]
    public void A_new_game_during_the_exchange_closes_it_and_drops_the_picks()
    {
        ReachTheExchange();
        Tap(0);

        _vm.NewGameCommand.Execute(null);

        Assert.False(_vm.IsHawseyExchangeVisible);
        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
        Assert.True(_vm.IsBiddingVisible);
    }
}
