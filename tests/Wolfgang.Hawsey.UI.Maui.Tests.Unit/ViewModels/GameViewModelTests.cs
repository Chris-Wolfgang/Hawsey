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



    private IEnumerable<SeatViewModel> Seats => [_vm.North, _vm.East, _vm.South, _vm.West];



    private void PlayFirstLegalCard()
    {
        var card = _vm.HumanCards.First(c => c.IsLegal);
        _vm.PlayCardCommand.Execute(card);
    }



    /// <summary>Passes every bid, names spades if asked, and plays the first legal card.</summary>
    private void TakeTheHumansTurn()
    {
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



    private void PlayToTheEndOfTheRound()
    {
        var steps = 0;

        while (!_vm.IsRoundSummaryVisible)
        {
            Assert.True(++steps < 1_000, "No round was completed.");
            TakeTheHumansTurn();
        }
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
        Assert.All(Seats, s => Assert.False(s.HasTableCard));
        Assert.Equal("Welcome to Hawsey. Click NEW GAME to start.", _vm.StatusMessage);
        Assert.False(_vm.IsBiddingVisible);
        Assert.False(_vm.IsTrumpPickerVisible);
        Assert.False(_vm.IsRoundSummaryVisible);
        Assert.Null(_vm.RoundSummary);
        Assert.Empty(_vm.GameLog);
        Assert.Equal("—", _vm.TrumpSymbol);
        Assert.Equal("—", _vm.WinningCardText);
    }



    [Fact]
    public void Seats_are_named_as_the_Blazor_UI_names_them()
    {
        Assert.Equal
        (
            ["PARTNER", "OPPONENT RIGHT", "YOU", "OPPONENT LEFT"],
            Seats.Select(s => s.Name)
        );
    }



    [Fact]
    public void NewGameCommand_deals_and_asks_the_human_to_bid()
    {
        _vm.NewGameCommand.Execute(null);

        Assert.True(_vm.IsBiddingVisible);
        Assert.Equal("Your turn to bid.", _vm.StatusMessage);
        Assert.Equal(12, _vm.HumanCards.Count);
        Assert.All(_vm.HumanCards, c => Assert.False(c.IsLegal));
        Assert.All(_vm.HumanCards, c => Assert.False(c.IsInteractive));
        Assert.All(Seats, s => Assert.Equal(12, s.CardCount));
        Assert.All(Seats, s => Assert.Equal(12, s.CardBacks.Count));
        Assert.Equal(0, _vm.NorthSouthScore);
        Assert.Equal(0, _vm.EastWestScore);
        Assert.Equal(0, _vm.UsTricks);
        Assert.Equal(0, _vm.ThemTricks);
        Assert.Equal("—", _vm.TrumpSymbol);
        Assert.False(_vm.HasTrumpSuit);
        Assert.Equal("", _vm.TrumpSetterTeam);
        Assert.Equal("", _vm.BidAmountText);
        Assert.True(_dispatcher.PostCount > 0);
    }



    [Fact]
    public void The_hand_is_sorted_spades_hearts_clubs_diamonds_high_card_first()
    {
        _vm.NewGameCommand.Execute(null);

        static int SuitOrder(Suit s) => s switch { Suit.Spades => 0, Suit.Hearts => 1, Suit.Clubs => 2, _ => 3 };
        var shown = _vm.HumanCards.Select(c => c.Card).ToList();
        var sorted = shown
            .OrderBy(c => SuitOrder(c.Suit))
            .ThenByDescending(c => (int)c.Rank)
            .ToList();

        Assert.Equal(sorted, shown);
    }



    [Fact]
    public void While_the_AIs_bid_their_bids_show_on_their_seats_and_the_bidder_is_highlighted()
    {
        _vm.NewGameCommand.Execute(null);

        // PassingAi: everyone before the human has passed.
        var state = _session.CurrentState!;
        Assert.True(_vm.South.IsActive);
        Assert.All(new[] { _vm.North, _vm.East, _vm.West }, s => Assert.False(s.IsActive));
        Assert.Contains(Seats, s => string.Equals(s.BidText, "Pass", StringComparison.Ordinal) && string.Equals(s.BidKind, "pass", StringComparison.Ordinal));
        Assert.Equal("", _vm.South.BidText);
        Assert.Single(Seats, s => s.IsDealer);
        Assert.True(Seats.Single(s => s.IsDealer).Position == state.Dealer);
    }



    [Fact]
    public void PlaceBidCommand_pass_leaves_the_stuck_dealer_and_plays_to_the_humans_card()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.PlaceBidCommand.Execute("pass");

        Assert.False(_vm.IsBiddingVisible);
        Assert.Equal("Your turn. Play a card.", _vm.StatusMessage);
        Assert.Equal("US", _vm.TrumpSetterTeam);
        Assert.True(_vm.IsTrumpSetterUs);
        Assert.Equal("· 6", _vm.BidAmountText);
        Assert.True(_vm.HasTrumpSuit);
        Assert.Contains(_vm.HumanCards, c => c.IsLegal);
        Assert.Contains(Seats, s => s.HasTableCard);
        Assert.StartsWith("Led by ", _vm.LedByText, StringComparison.Ordinal);
        Assert.NotEqual("—", _vm.WinningCardText);

        // Once play starts the badges go, as in the Blazor UI.
        Assert.All(Seats, s => Assert.Equal("", s.BidText));
        Assert.All(Seats, s => Assert.False(s.IsActive));
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
        Assert.Equal("Pick trump.", _vm.StatusMessage);
        Assert.Equal("Bid 8", _vm.South.BidText);
        Assert.Equal("number", _vm.South.BidKind);

        _vm.SelectTrumpCommand.Execute("diamonds");

        Assert.False(_vm.IsTrumpPickerVisible);
        Assert.Equal("♦", _vm.TrumpSymbol);
        Assert.True(_vm.IsTrumpRed);
        Assert.Equal(TableColors.RedTrumpMark, _vm.TrumpMarkColor);
        Assert.Equal("US", _vm.TrumpSetterTeam);
        Assert.Equal("· 8", _vm.BidAmountText);
        Assert.Equal("Your turn. Play a card.", _vm.StatusMessage);
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
    public void A_black_trump_shows_black_watermarks()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute("clubs");

        Assert.Equal("♣", _vm.TrumpSymbol);
        Assert.False(_vm.IsTrumpRed);
        Assert.Equal(TableColors.BlackTrumpMark, _vm.TrumpMarkColor);
    }



    [Fact]
    public void SelectTrumpCommand_acehigh_plays_without_trump()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute("acehigh");

        Assert.Equal("—", _vm.TrumpSymbol);
        Assert.False(_vm.HasTrumpSuit);
        Assert.Equal("US", _vm.TrumpSetterTeam);
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

        Assert.Equal("Hawsey", _vm.South.BidText);
        Assert.Equal("hawsey", _vm.South.BidKind);

        _vm.SelectTrumpCommand.Execute("hearts");

        Assert.Equal("Hawsey! Pick 2 cards to discard.", _vm.StatusMessage);
        Assert.Equal("· Hawsey (24)", _vm.BidAmountText);
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
    public void PlayCardCommand_with_no_card_does_nothing()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");
        var before = _session.CurrentState;

        _vm.PlayCardCommand.Execute(null);

        Assert.Same(before, _session.CurrentState);
    }



    [Fact]
    public void PlayCardCommand_plays_the_card_and_the_AI_answers()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");

        PlayFirstLegalCard();

        Assert.Equal(11, _vm.HumanCards.Count);
        Assert.Equal(11, _vm.South.CardCount);
        Assert.Equal("Your turn. Play a card.", _vm.StatusMessage);
    }



    [Fact]
    public void A_round_ends_in_a_summary_that_waits_for_Next_round()
    {
        _vm.NewGameCommand.Execute(null);

        PlayToTheEndOfTheRound();

        var state = _session.CurrentState!;
        var summary = _vm.RoundSummary!;
        Assert.Equal(GamePhase.RoundScoring, state.Phase);
        Assert.Equal("Round complete.", _vm.StatusMessage);
        Assert.False(summary.IsGameOver);
        Assert.Equal("Round complete", summary.Title);
        Assert.Equal(12, summary.UsTricks + summary.ThemTricks);
        Assert.Equal(12, _vm.UsTricks + _vm.ThemTricks);
        Assert.Equal(state.NorthSouthScore, summary.UsTotal);
        Assert.Equal(state.EastWestScore, summary.ThemTotal);
        Assert.Equal(12, Seats.Sum(s => s.TricksWon));

        _vm.NextRoundCommand.Execute(null);

        Assert.False(_vm.IsRoundSummaryVisible);
        Assert.NotEqual(GamePhase.RoundScoring, _session.CurrentState!.Phase);
        Assert.All(Seats, s => Assert.Equal(0, s.TricksWon));
    }



    [Fact]
    public void NextRoundCommand_before_the_round_is_over_is_ignored()
    {
        _vm.NewGameCommand.Execute(null);
        var before = _session.CurrentState;

        _vm.NextRoundCommand.Execute(null);

        Assert.Same(before, _session.CurrentState);
        Assert.True(_vm.IsBiddingVisible);
    }



    [Fact]
    public void Playing_whole_games_reaches_game_over_and_Play_again_resets_the_table()
    {
        _vm.NewGameCommand.Execute(null);

        var steps = 0;

        while (_vm.RoundSummary is not { IsGameOver: true })
        {
            Assert.True(++steps < 10_000, "The game did not finish.");

            if (_vm.IsRoundSummaryVisible)
            {
                _vm.NextRoundCommand.Execute(null);
            }
            else
            {
                TakeTheHumansTurn();
            }
        }

        var state = _session.CurrentState!;
        var summary = _vm.RoundSummary;
        Assert.Equal(GamePhase.GameOver, state.Phase);
        Assert.Equal("Game over.", _vm.StatusMessage);
        Assert.EndsWith(" wins the game!", summary.Title, StringComparison.Ordinal);
        Assert.Equal(state.Winner == Team.NorthSouth, summary.IsTitleUs);
        Assert.Equal(state.NorthSouthScore, summary.UsTotal);
        Assert.Equal(state.NorthSouthScore, _vm.NorthSouthScore);
        Assert.Equal(state.EastWestScore, _vm.EastWestScore);

        _vm.NewGameCommand.Execute(null);

        Assert.False(_vm.IsRoundSummaryVisible);
        Assert.True(_vm.IsBiddingVisible);
    }



    [Fact]
    public void A_completed_trick_stays_on_the_table_until_the_next_lead()
    {
        _vm.NewGameCommand.Execute(null);

        // The session refreshes the table (StateChanged) before it announces the trick
        // (TrickCompleted), and the view model subscribed before this test did, so when
        // the test's handler runs each seat must show its card of the trick just won.
        // That includes the twelfth trick, when the round is over and there is no
        // current trick at all.
        var checkedTricks = 0;
        _session.TrickCompleted += (_, _) =>
        {
            var trick = _session.CurrentState!.CompletedTricks[^1];

            foreach (var play in trick.Cards)
            {
                Assert.Equal(play.Card, Seats.Single(s => s.Position == play.Player).TableCard?.Card);
            }

            checkedTricks++;
        };

        _vm.PlaceBidCommand.Execute("pass");

        while (checkedTricks < 12)
        {
            PlayFirstLegalCard();
        }
    }



    [Fact]
    public void The_game_log_lists_the_bids_then_each_tricks_winner()
    {
        _vm.NewGameCommand.Execute(null);

        Assert.All(_vm.GameLog, e => Assert.EndsWith(" passes.", e, StringComparison.Ordinal));
        var bidsBefore = _vm.GameLog.Count;

        _vm.PlaceBidCommand.Execute("pass");
        PlayFirstLegalCard();

        Assert.Equal($"{PlayerPosition.South} passes.", _vm.GameLog[bidsBefore]);
        Assert.Contains(_vm.GameLog, e => e.StartsWith("Trick 1: ", StringComparison.Ordinal));
    }



    [Fact]
    public void ToggleLogCommand_opens_and_closes_the_log()
    {
        _vm.ToggleLogCommand.Execute(null);

        Assert.True(_vm.IsLogVisible);

        _vm.ToggleLogCommand.Execute(null);

        Assert.False(_vm.IsLogVisible);
    }



    [Fact]
    public void MoveCard_moves_a_card_before_the_card_it_is_dropped_on()
    {
        _vm.NewGameCommand.Execute(null);
        var before = _vm.HumanCards.Select(c => c.Card).ToList();

        _vm.MoveCard(0, 3);

        var after = _vm.HumanCards.Select(c => c.Card).ToList();
        Assert.Equal([before[1], before[2], before[0], before[3]], after.Take(4));

        _vm.MoveCard(5, 1);

        var again = _vm.HumanCards.Select(c => c.Card).ToList();
        Assert.Equal(after[5], again[1]);
        Assert.Equal(after[1], again[2]);
    }



    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 2)]
    [InlineData(2, 12)]
    [InlineData(12, 2)]
    [InlineData(2, -1)]
    public void MoveCard_out_of_range_or_onto_itself_does_nothing(int from, int to)
    {
        _vm.NewGameCommand.Execute(null);
        var before = _vm.HumanCards.Select(c => c.Card).ToList();

        _vm.MoveCard(from, to);

        Assert.Equal(before, _vm.HumanCards.Select(c => c.Card));
    }



    [Fact]
    public void MoveCard_before_a_deal_does_nothing()
    {
        _vm.MoveCard(0, 1);

        Assert.Empty(_vm.HumanCards);
    }



    [Fact]
    public void The_humans_order_survives_playing_a_card()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.MoveCard(0, 12 - 1);
        var order = _vm.HumanCards.Select(c => c.Card).ToList();
        _vm.PlaceBidCommand.Execute("pass");

        var played = _vm.HumanCards.First(c => c.IsLegal).Card;
        PlayFirstLegalCard();

        var expected = new List<Card>(order);
        expected.Remove(played);
        Assert.Equal(expected, _vm.HumanCards.Select(c => c.Card));
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
