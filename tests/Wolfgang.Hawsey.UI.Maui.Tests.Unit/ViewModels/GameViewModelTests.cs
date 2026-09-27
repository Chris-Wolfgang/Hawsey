using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// <see cref="GameViewModel"/> over an unpaced <see cref="GameSession"/> and a
/// dispatcher that runs at once, so each command runs the game forward
/// synchronously to the next point where the human must act.
/// </summary>
public class GameViewModelTests
{
    private readonly GameSession _session = TestGameSessions.Unpaced();
    private readonly ImmediateDispatcher _dispatcher = new();
    private readonly GameViewModel _vm;



    public GameViewModelTests()
    {
        _vm = new GameViewModel(_session, _dispatcher);
    }



    private void PlayFirstLegalCard()
    {
        var card = _vm.HumanCards.First(c => c.IsLegal);
        _vm.PlayCardCommand.Execute(card);
    }



    [Fact]
    public void Constructor_when_the_game_session_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new GameViewModel(null!, _dispatcher));

        Assert.Equal("gameService", ex.ParamName);
    }



    [Fact]
    public void Constructor_when_the_dispatcher_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new GameViewModel(_session, null!));

        Assert.Equal("dispatcher", ex.ParamName);
    }



    [Fact]
    public void Initially_the_table_is_empty_and_the_player_is_welcomed()
    {
        Assert.Empty(_vm.HumanCards);
        Assert.Empty(_vm.TrickCards);
        Assert.Contains("Welcome", _vm.StatusMessage, StringComparison.Ordinal);
        Assert.False(_vm.IsBiddingVisible);
        Assert.False(_vm.IsTrumpPickerVisible);
        Assert.False(_vm.IsGameOverVisible);
        Assert.Equal("", _vm.GameOverMessage);
    }



    [Fact]
    public void NewGameCommand_deals_and_asks_the_human_to_bid()
    {
        _vm.NewGameCommand.Execute(null);

        Assert.True(_vm.IsBiddingVisible);
        Assert.Equal("Your turn to bid", _vm.StatusMessage);
        Assert.Equal(12, _vm.HumanCards.Count);
        Assert.All(_vm.HumanCards, c => Assert.False(c.IsLegal));
        Assert.Equal(12, _vm.NorthCardCount);
        Assert.Equal(12, _vm.EastCardCount);
        Assert.Equal(12, _vm.WestCardCount);
        Assert.Equal(0, _vm.NorthSouthScore);
        Assert.Equal(0, _vm.EastWestScore);
        Assert.Equal("", _vm.TrumpDisplay);
        Assert.Equal("", _vm.BidInfoDisplay);
        Assert.True(_dispatcher.PostCount > 0);
    }



    [Fact]
    public void PlaceBidCommand_pass_leaves_the_stuck_dealer_and_plays_to_the_humans_card()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.PlaceBidCommand.Execute("pass");

        Assert.False(_vm.IsBiddingVisible);
        Assert.Equal("Your turn to play", _vm.StatusMessage);
        Assert.Equal("North stuck at 6", _vm.BidInfoDisplay);
        Assert.StartsWith("Trump: ", _vm.TrumpDisplay, StringComparison.Ordinal);
        Assert.Contains(_vm.HumanCards, c => c.IsLegal);
        Assert.NotEmpty(_vm.TrickCards);
    }



    [Fact]
    public void PlaceBidCommand_with_an_unknown_value_does_nothing()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.PlaceBidCommand.Execute("seven");

        Assert.True(_vm.IsBiddingVisible);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlaceBidCommand_before_the_humans_turn_is_ignored()
    {
        _session.StartNewGame();

        _vm.PlaceBidCommand.Execute("pass");

        Assert.False(_vm.IsBiddingVisible);
        Assert.Equal(PlayerPosition.East, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public void A_number_bid_wins_and_the_human_names_trump()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.PlaceBidCommand.Execute("8");

        Assert.True(_vm.IsTrumpPickerVisible);
        Assert.Equal("Choose trump suit or Ace High", _vm.StatusMessage);

        _vm.SelectTrumpCommand.Execute("diamonds");

        Assert.False(_vm.IsTrumpPickerVisible);
        Assert.Equal("Trump: ♦", _vm.TrumpDisplay);
        Assert.Equal("South bid 8", _vm.BidInfoDisplay);
        Assert.Equal("Your turn to play", _vm.StatusMessage);
    }



    [Theory]
    [InlineData("hearts", Suit.Hearts)]
    [InlineData("clubs", Suit.Clubs)]
    [InlineData("spades", Suit.Spades)]
    public void SelectTrumpCommand_names_the_suit(string parameter, Suit expected)
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute(parameter);

        Assert.Equal(expected, _session.CurrentState!.TrumpSuit);
    }



    [Fact]
    public void SelectTrumpCommand_acehigh_plays_without_trump()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute("acehigh");

        Assert.Equal("Ace High", _vm.TrumpDisplay);
    }



    [Fact]
    public void SelectTrumpCommand_when_the_human_is_not_naming_trump_is_ignored()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.SelectTrumpCommand.Execute("hearts");

        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
        Assert.True(_vm.IsBiddingVisible);
    }



    [Fact]
    public void A_Hawsey_bid_asks_the_human_for_the_exchange()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("hawsey");
        _vm.SelectTrumpCommand.Execute("hearts");

        Assert.Equal("Hawsey! Select cards to exchange", _vm.StatusMessage);
        Assert.Equal("South called Hawsey!", _vm.BidInfoDisplay);
        Assert.Equal(GamePhase.HawseyExchange, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlayCardCommand_ignores_an_illegal_card()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");
        var before = _session.CurrentState;

        var illegal = _vm.HumanCards.FirstOrDefault(c => !c.IsLegal)
            ?? new CardViewModel(new Card(Rank.Nine, Suit.Hearts), isLegal: false);
        _vm.PlayCardCommand.Execute(illegal);

        Assert.Same(before, _session.CurrentState);
    }



    [Fact]
    public void PlayCardCommand_ignores_a_card_the_session_rejects()
    {
        _vm.NewGameCommand.Execute(null);

        // Marked legal by the caller, but it is still the bidding phase.
        _vm.PlayCardCommand.Execute(new CardViewModel(_vm.HumanCards[0].Card, isLegal: true));

        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlayCardCommand_plays_the_card_and_the_AI_answers()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");

        PlayFirstLegalCard();

        Assert.Equal(11, _vm.HumanCards.Count);
        Assert.Equal("Your turn to play", _vm.StatusMessage);
    }



    [Fact]
    public void Playing_whole_games_reaches_game_over_and_a_new_game_resets_the_table()
    {
        _vm.NewGameCommand.Execute(null);

        var steps = 0;

        while (!_vm.IsGameOverVisible)
        {
            Assert.True(++steps < 10_000, "The game did not finish.");

            if (_vm.IsBiddingVisible)
            {
                _vm.PlaceBidCommand.Execute("pass");
            }
            else if (_vm.IsTrumpPickerVisible)
            {
                _vm.SelectTrumpCommand.Execute("spades");
            }
            else
            {
                PlayFirstLegalCard();
            }
        }

        var state = _session.CurrentState!;
        Assert.Equal(GamePhase.GameOver, state.Phase);
        Assert.Equal("Game Over!", _vm.StatusMessage);
        Assert.Contains($"NS: {state.NorthSouthScore}", _vm.GameOverMessage, StringComparison.Ordinal);
        Assert.Contains("wins!", _vm.GameOverMessage, StringComparison.Ordinal);
        Assert.Equal(state.NorthSouthScore, _vm.NorthSouthScore);
        Assert.Equal(state.EastWestScore, _vm.EastWestScore);

        _vm.NewGameCommand.Execute(null);

        Assert.False(_vm.IsGameOverVisible);
        Assert.True(_vm.IsBiddingVisible);
    }



    [Fact]
    public void Trick_and_round_events_update_the_status()
    {
        _vm.NewGameCommand.Execute(null);
        var messages = new List<string>();
        _vm.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, nameof(GameViewModel.StatusMessage), StringComparison.Ordinal))
            {
                messages.Add(_vm.StatusMessage);
            }
        };

        var steps = 0;

        while (!messages.Exists(m => m.StartsWith("Round over!", StringComparison.Ordinal)))
        {
            Assert.True(++steps < 1_000, "No round was completed.");
            Assert.False(_vm.IsTrumpPickerVisible);

            if (_vm.IsBiddingVisible)
            {
                _vm.PlaceBidCommand.Execute("pass");
            }
            else
            {
                PlayFirstLegalCard();
            }
        }

        Assert.Contains(messages, m => m.EndsWith(" wins the trick!", StringComparison.Ordinal));
    }



    [Fact]
    public void PropertyChanged_is_raised_only_when_a_value_changes()
    {
        var changed = new List<string?>();
        _vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _vm.StatusMessage = "x";
        _vm.StatusMessage = "x";
        _vm.IsBiddingVisible = false;

        Assert.Equal([nameof(GameViewModel.StatusMessage)], changed);
    }
}
