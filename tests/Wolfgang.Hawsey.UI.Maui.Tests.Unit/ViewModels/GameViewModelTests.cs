using System.Collections.Specialized;
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
    public void Constructor_when_no_game_has_started_shows_an_empty_table_and_the_welcome()
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
    public void Seats_when_created_are_named_as_the_Blazor_UI_names_them()
    {
        Assert.Equal
        (
            ["PARTNER", "OPPONENT RIGHT", "YOU", "OPPONENT LEFT"],
            Seats.Select(s => s.Name)
        );
    }



    [Fact]
    public void NewGameCommand_when_the_AI_passes_deals_and_asks_the_human_to_bid()
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
    public void HumanCards_when_dealt_are_sorted_spades_hearts_clubs_diamonds_high_card_first()
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
    public void Seats_when_the_AI_bids_show_the_bids_and_highlight_the_bidder()
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
    public void PlaceBidCommand_when_the_human_passes_leaves_the_stuck_dealer_and_plays_to_the_humans_card()
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
    public void PlaceBidCommand_when_the_value_is_unknown_does_nothing()
    {
        _vm.NewGameCommand.Execute(null);

        _vm.PlaceBidCommand.Execute("seven");

        Assert.True(_vm.IsBiddingVisible);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlaceBidCommand_when_it_is_not_the_humans_turn_is_ignored()
    {
        _session.StartNewGame();

        _vm.PlaceBidCommand.Execute("pass");

        Assert.False(_vm.IsBiddingVisible);
        Assert.Equal(PlayerPosition.East, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public void PlaceBidCommand_when_a_number_bid_wins_asks_the_human_to_name_trump()
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
    public void SelectTrumpCommand_when_given_a_suit_names_it(string parameter, Suit expected)
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute(parameter);

        Assert.Equal(expected, _session.CurrentState!.TrumpSuit);
    }



    [Fact]
    public void SelectTrumpCommand_when_trump_is_black_shows_black_watermarks()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("7");

        _vm.SelectTrumpCommand.Execute("clubs");

        Assert.Equal("♣", _vm.TrumpSymbol);
        Assert.False(_vm.IsTrumpRed);
        Assert.Equal(TableColors.BlackTrumpMark, _vm.TrumpMarkColor);
    }



    [Fact]
    public void SelectTrumpCommand_when_acehigh_plays_without_trump()
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
    public void PlaceBidCommand_when_the_human_bids_Hawsey_asks_for_the_exchange()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("hawsey");

        Assert.Equal("Hawsey", _vm.South.BidText);
        Assert.Equal("hawsey", _vm.South.BidKind);

        _vm.SelectTrumpCommand.Execute("hearts");

        Assert.Equal("Hawsey! Pick 2 cards to discard.", _vm.StatusMessage);
        Assert.Equal($"· Hawsey ({GameState.HawseyBidAmount})", _vm.BidAmountText);
        Assert.Equal($"Hawsey ({GameState.HawseyBidAmount})", _vm.HawseyBidText);
        Assert.Equal(GamePhase.HawseyExchange, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlayCardCommand_when_the_card_is_illegal_is_ignored()
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
    public void PlayCardCommand_when_the_session_rejects_the_card_is_ignored()
    {
        _vm.NewGameCommand.Execute(null);

        // Marked legal by the caller, but it is still the bidding phase.
        _vm.PlayCardCommand.Execute(new CardViewModel(_vm.HumanCards[0].Card, isLegal: true));

        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PlayCardCommand_when_there_is_no_card_does_nothing()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");
        var before = _session.CurrentState;

        _vm.PlayCardCommand.Execute(null);

        Assert.Same(before, _session.CurrentState);
    }



    [Fact]
    public void PlayCardCommand_when_the_card_is_legal_plays_it_and_the_AI_answers()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");

        PlayFirstLegalCard();

        Assert.Equal(11, _vm.HumanCards.Count);
        Assert.Equal(11, _vm.South.CardCount);
        Assert.Equal("Your turn. Play a card.", _vm.StatusMessage);
    }



    [Fact]
    public void PlayCardCommand_when_the_round_ends_shows_a_summary_that_waits_for_Next_round()
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
    public void NewGameCommand_when_clicked_during_an_AI_pause_shows_only_the_new_games_turn()
    {
        // The first New Game's AI loop is paused when the second starts; the session tells
        // it to stand down (WaitingFor.Nothing), and only the second game's turn shows.
        var session = TestGameSessions.Paced();
        var vm = new GameViewModel(session, new ImmediateDispatcher());

        vm.NewGameCommand.Execute(null);
        vm.NewGameCommand.Execute(null);

        Assert.True
        (
            SpinWait.SpinUntil(() => string.Equals(vm.StatusMessage, "Your turn to bid.", StringComparison.Ordinal), TimeSpan.FromSeconds(30)),
            vm.StatusMessage
        );
        Assert.True(vm.IsBiddingVisible);
        Assert.Equal(GamePhase.Bidding, session.CurrentState!.Phase);
    }



    [Fact]
    public void NextRoundCommand_when_the_round_is_not_over_is_ignored()
    {
        _vm.NewGameCommand.Execute(null);
        var before = _session.CurrentState;

        _vm.NextRoundCommand.Execute(null);

        Assert.Same(before, _session.CurrentState);
        Assert.True(_vm.IsBiddingVisible);
    }



    [Fact]
    public void NewGameCommand_when_a_game_is_over_resets_the_table_for_another()
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
    public void Seats_when_a_trick_completes_keep_its_cards_on_the_table()
    {
        _vm.NewGameCommand.Execute(null);

        // The session refreshes the table (StateChanged) before it announces the trick
        // (TrickCompleted), and the view model subscribed before this test did, so when
        // the test's handler runs each seat must show its card of the trick just won.
        // That includes the twelfth trick, when the round is over and there is no
        // current trick at all. The handler runs inside the command, which reports any
        // exception in the status line instead of throwing, so it only records what it
        // sees; the assertions come after.
        var tricks = new List<(PlayerPosition Player, Card Played, Card? Shown)[]>();
        _session.TrickCompleted += (_, _) =>
        {
            var trick = _session.CurrentState!.CompletedTricks[^1];
            tricks.Add
            (
                trick.Cards
                    .Select(play => (play.Player, play.Card, Seats.Single(s => s.Position == play.Player).TableCard?.Card))
                    .ToArray()
            );
        };

        _vm.PlaceBidCommand.Execute("pass");

        for (var steps = 0; tricks.Count < 12; steps++)
        {
            Assert.True(steps < 100, "The round did not finish.");
            PlayFirstLegalCard();
        }

        Assert.All
        (
            tricks.SelectMany(t => t),
            seen => Assert.True(seen.Shown == seen.Played, $"{seen.Player} played {seen.Played} but the table shows {seen.Shown}")
        );
    }



    [Fact]
    public void GameLog_when_the_round_goes_on_lists_the_bids_then_each_tricks_winner()
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
    public void ToggleLogCommand_when_executed_opens_and_closes_the_log()
    {
        _vm.ToggleLogCommand.Execute(null);

        Assert.True(_vm.IsLogVisible);

        _vm.ToggleLogCommand.Execute(null);

        Assert.False(_vm.IsLogVisible);
    }



    [Fact]
    public void MoveCard_when_in_range_moves_the_card_before_the_card_it_is_dropped_on()
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
    public void MoveCard_when_out_of_range_or_onto_itself_does_nothing(int from, int to)
    {
        _vm.NewGameCommand.Execute(null);
        var before = _vm.HumanCards.Select(c => c.Card).ToList();

        _vm.MoveCard(from, to);

        Assert.Equal(before, _vm.HumanCards.Select(c => c.Card));
    }



    [Fact]
    public void MoveCard_when_nothing_is_dealt_does_nothing()
    {
        _vm.MoveCard(0, 1);

        Assert.Empty(_vm.HumanCards);
    }



    [Fact]
    public void PlayCardCommand_when_the_AI_answers_rebuilds_the_hand_once_and_otherwise_replaces_only_changed_cards()
    {
        _vm.NewGameCommand.Execute(null);
        _vm.PlaceBidCommand.Execute("pass");
        var changes = new List<NotifyCollectionChangedAction>();
        _vm.HumanCards.CollectionChanged += (_, e) => changes.Add(e.Action);

        // The human's card leaves the hand (one rebuild); then the AI's moves change
        // nothing in it, until the human's next turn makes some cards legal again.
        PlayFirstLegalCard();

        Assert.Equal(1, changes.Count(a => a == NotifyCollectionChangedAction.Reset));
        Assert.All
        (
            changes.Where(a => a != NotifyCollectionChangedAction.Reset),
            a => Assert.True(a is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace, a.ToString())
        );
        Assert.True(changes.Count(a => a == NotifyCollectionChangedAction.Replace) <= _vm.HumanCards.Count);
        Assert.Equal(11, _vm.HumanCards.Count);
    }



    [Fact]
    public void PlayCardCommand_when_the_human_has_rearranged_the_hand_keeps_the_order()
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
    public void PropertyChanged_when_a_value_changes_is_raised_and_otherwise_not()
    {
        var changed = new List<string?>();
        _vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        // The log opens (a change); the page's size leaves the layout as it was (none).
        _vm.ToggleLogCommand.Execute(null);
        _vm.SetPageSize(0, 0);

        Assert.Equal([nameof(GameViewModel.IsLogVisible)], changed);
    }
}
