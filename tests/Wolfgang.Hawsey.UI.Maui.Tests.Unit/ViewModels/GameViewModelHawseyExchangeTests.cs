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
    public void SelectTrumpCommand_when_the_human_bid_Hawsey_opens_the_exchange_with_nothing_picked()
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
    public void PlayCardCommand_when_exchanging_picks_at_most_two_cards_and_a_second_tap_unpicks()
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
    public void PlayCardCommand_when_exchanging_and_the_card_is_not_in_the_hand_picks_nothing()
    {
        ReachTheExchange();

        _vm.PlayCardCommand.Execute(new CardViewModel(new Card(Rank.Ace, Suit.Spades), isLegal: true));

        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
    }



    [Fact]
    public void ConfirmHawseyExchangeCommand_when_fewer_than_two_are_picked_does_nothing()
    {
        ReachTheExchange();
        Tap(0);

        _vm.ConfirmHawseyExchangeCommand.Execute(null);

        Assert.Equal(GamePhase.HawseyExchange, _session.CurrentState!.Phase);
        Assert.True(_vm.IsHawseyExchangeVisible);
    }



    [Fact]
    public void ConfirmHawseyExchangeCommand_when_two_are_picked_discards_them_takes_the_partners_best_and_plays_on()
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



    [Theory]
    [InlineData(3, 3, 7, "the picked card itself is dragged right")]
    [InlineData(5, 5, 3, "the picked card itself is dragged left")]
    [InlineData(5, 3, 8, "another card is dragged across it, left to right")]
    [InlineData(5, 9, 3, "another card is dragged across it, right to left")]
    [InlineData(4, 7, 9, "another card moves entirely to its right")]
    public void MoveCard_when_a_card_is_dragged_during_the_exchange_keeps_the_pick_on_the_same_card(int picked, int from, int to, string scenario)
    {
        ReachTheExchange();
        var expected = _vm.HumanCards[picked].Card;
        Tap(picked);

        _vm.MoveCard(from, to);

        var stillPicked = Assert.Single(_vm.HumanCards, c => c.IsSelected);
        Assert.True(stillPicked.Card == expected, scenario);
        Assert.Equal("Selected: 1 / 2", _vm.ExchangeSelectionText);
    }



    [Fact]
    public void ConfirmHawseyExchangeCommand_when_the_game_moved_on_unseen_is_refused_and_changes_nothing()
    {
        // The view model has not yet seen a New Game (its dispatcher is busy), so the
        // exchange still shows with two cards picked; the session refuses the stale move.
        var session = TestGameSessions.Unpaced();
        var dispatcher = new DeferredDispatcher();
        var vm = new GameViewModel(session, dispatcher);
        vm.NewGameCommand.Execute(null);
        vm.PlaceBidCommand.Execute("hawsey");
        vm.SelectTrumpCommand.Execute("spades");
        dispatcher.RunAll();
        vm.PlayCardCommand.Execute(vm.HumanCards[0]);
        vm.PlayCardCommand.Execute(vm.HumanCards[3]);
        session.StartNewGame();
        var dealt = session.CurrentState;

        vm.ConfirmHawseyExchangeCommand.Execute(null);

        Assert.Same(dealt, session.CurrentState);
        Assert.True(vm.IsHawseyExchangeVisible);
        Assert.Equal("Hawsey! Pick 2 cards to discard.", vm.StatusMessage);
    }



    [Fact]
    public void NewGameCommand_when_exchanging_closes_the_exchange_and_drops_the_picks()
    {
        ReachTheExchange();
        Tap(0);

        _vm.NewGameCommand.Execute(null);

        Assert.False(_vm.IsHawseyExchangeVisible);
        Assert.Equal("Selected: 0 / 2", _vm.ExchangeSelectionText);
        Assert.True(_vm.IsBiddingVisible);
    }
}
