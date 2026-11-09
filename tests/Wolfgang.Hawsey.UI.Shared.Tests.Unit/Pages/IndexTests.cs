using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.UI.Shared.Layout;
using IndexPage = Wolfgang.Hawsey.UI.Shared.Pages.Index;

namespace Wolfgang.Hawsey.UI.Shared.Tests.Unit.Pages;

/// <summary>
/// The game page against a real <see cref="GameSession"/>: tests click what a player
/// clicks and read what a player sees.
/// </summary>
public sealed class IndexTests : BunitContext
{
    private const string NewGame = "☰ NEW GAME";
    private const string Confirm = "Discard & receive";

    private static readonly TimeSpan PacedTimeout = TimeSpan.FromSeconds(30);



    private GameSession UseSession(IPlayerStrategy? ai = null, int seed = 42, bool paced = false)
    {
        var session = new GameSession
        (
            aiPacing: paced,
            randomFactory: () => new Random(seed),
            aiStrategy: ai ?? new ScriptedAi()
        );

        Services.AddSingleton(session);

        return session;
    }



    private static void ClickButton(IRenderedComponent<IndexPage> page, string text) =>
        page
            .FindAll("button")
            .First(b => string.Equals(b.TextContent.Trim(), text, StringComparison.Ordinal))
            .Click();



    private static bool HasButton(IRenderedComponent<IndexPage> page, string text) =>
        page
            .FindAll("button")
            .Any(b => string.Equals(b.TextContent.Trim(), text, StringComparison.Ordinal));



    private static string Status(IRenderedComponent<IndexPage> page) => page.Find(".statusbar").TextContent;



    private static string[] HandText(IRenderedComponent<IndexPage> page) =>
        page
            .FindAll(".hand .card")
            .Select(c => string.Concat(c.TextContent.Where(ch => !char.IsWhiteSpace(ch))))
            .ToArray();



    private static void StartGameAndWinBid(IRenderedComponent<IndexPage> page, string trumpButton)
    {
        ClickButton(page, NewGame);
        ClickButton(page, "Bid 7");
        ClickButton(page, trumpButton);
    }



    /// <summary>
    /// Answers whatever the page asks until it shows the button named
    /// <paramref name="stopButton"/>: plays the first playable card, passes when it can,
    /// and otherwise takes the only bid a stuck dealer has, at ace high.
    /// </summary>
    private static void PlayUntil(IRenderedComponent<IndexPage> page, string stopButton)
    {
        var steps = 0;

        while (!HasButton(page, stopButton))
        {
            steps++;
            Assert.True(steps <= 10_000, "The game never reached the button " + stopButton);

            var playable = page.FindAll(".hand .card.playable");

            if (playable.Count > 0)
            {
                playable[0].Click();
            }
            else if (HasButton(page, "Pass"))
            {
                ClickButton(page, "Pass");
            }
            else if (HasButton(page, "Next round"))
            {
                ClickButton(page, "Next round");
            }
            else
            {
                page.Find(".bid-buttons button").Click();
                ClickButton(page, "Ace High (no trump)");
            }
        }
    }



    /// <summary>Plays out the two rounds before the human's first deal as dealer.</summary>
    private static void PlayToRoundThree(IRenderedComponent<IndexPage> page)
    {
        ClickButton(page, NewGame);
        PlayUntil(page, "Next round");
        ClickButton(page, "Next round");
        PlayUntil(page, "Next round");
        ClickButton(page, "Next round");
    }



    [Fact]
    public void MainLayout_when_rendered_shows_the_page_in_its_body()
    {
        var layout = Render<MainLayout>
        (
            parameters => parameters.Add(l => l.Body, (RenderFragment)(builder => builder.AddContent(0, "the page")))
        );

        Assert.Contains("the page", layout.Markup, StringComparison.Ordinal);
    }



    [Fact]
    public void Render_when_no_game_has_started_shows_the_welcome_and_an_empty_table()
    {
        UseSession();

        var page = Render<IndexPage>();

        Assert.Contains("Click NEW GAME", Status(page), StringComparison.Ordinal);
        Assert.Empty(page.FindAll(".hand .card"));
        Assert.Empty(page.FindAll(".card-back"));
        Assert.Empty(page.FindAll(".trump-corner"));
        Assert.Empty(page.FindAll(".dealer-puck"));
        Assert.Equal("—", page.Find(".suit-mark").TextContent);
        Assert.Equal("—", page.Find(".winner-card").TextContent);
        Assert.Equal("", page.Find(".led").TextContent);
        Assert.Equal("0", page.Find(".score .col-us .value").TextContent);
        Assert.Equal("0", page.Find(".tricks .col-them .value").TextContent);
    }



    [Fact]
    public void NewGame_when_the_AI_passes_asks_the_human_to_bid()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        Assert.Equal("Your turn to bid.", Status(page));
        Assert.Contains("No bids yet", page.Find(".modal-sub").TextContent, StringComparison.Ordinal);
        Assert.Equal("Bid 6", page.Find(".bid-buttons button").TextContent);
        Assert.Equal
        (
            $"Bid {GameState.MaximumBid}",
            page.FindAll(".bid-buttons button")[^1].TextContent
        );
        Assert.True(HasButton(page, "Pass"));
        Assert.Equal(12, page.FindAll(".hand .card").Count);
        Assert.Equal(12, page.FindAll(".seat.partner .card-back").Count);
        Assert.Equal(12, page.FindAll(".seat.left .card-back").Count);
        Assert.Equal(12, page.FindAll(".seat.right .card-back").Count);
        Assert.Equal("Pass", page.Find(".seat.right .bid-badge.pass").TextContent);
        Assert.Single(page.FindAll(".seat.partner .dealer-puck"));
        Assert.Contains("active", page.Find(".seat.you").ClassName, StringComparison.Ordinal);
    }



    [Fact]
    public void BidPicker_when_an_opponent_has_bid_shows_the_high_bid_and_only_higher_bids()
    {
        UseSession
        (
            new ScriptedAi((_, player) => player == PlayerPosition.East ? new BidAction.NumberBid(8) : BidAction.PassBid.Instance)
        );
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        Assert.Contains("Current high bid: 8 by East (opponents)", page.Find(".modal-sub").TextContent, StringComparison.Ordinal);
        Assert.Equal("Bid 9", page.Find(".bid-buttons button").TextContent);
        Assert.Equal("Bid 8", page.Find(".seat.right .bid-badge.number").TextContent);
    }



    [Fact]
    public void BidPicker_when_the_partner_has_bid_names_the_partner()
    {
        UseSession
        (
            new ScriptedAi((_, player) => player == PlayerPosition.North ? new BidAction.NumberBid(7) : BidAction.PassBid.Instance)
        );
        var page = Render<IndexPage>();

        PlayToRoundThree(page);

        Assert.Contains("Current high bid: 7 by your partner", page.Find(".modal-sub").TextContent, StringComparison.Ordinal);
        Assert.Single(page.FindAll(".seat.you .dealer-puck"));
    }



    [Fact]
    public void BidPicker_when_the_human_is_the_stuck_dealer_offers_only_the_minimum_or_Hawsey()
    {
        UseSession();
        var page = Render<IndexPage>();

        PlayToRoundThree(page);

        Assert.Contains("stuck", page.Find(".modal-sub").TextContent, StringComparison.Ordinal);
        Assert.Equal("Bid 6", Assert.Single(page.FindAll(".bid-buttons button")).TextContent);
        Assert.False(HasButton(page, "Pass"));

        PlayUntil(page, "Next round");

        Assert.Equal("6", page.Find(".rs-bid strong").TextContent);
        Assert.Equal("US", page.Find(".rs-bid span").TextContent);
    }



    [Theory]
    [InlineData("♥ Hearts", "♥", "red")]
    [InlineData("♦ Diamonds", "♦", "red")]
    [InlineData("♣ Clubs", "♣", "black")]
    [InlineData("♠ Spades", "♠", "black")]
    public void SelectTrump_when_a_suit_is_chosen_marks_the_table_with_it(string button, string symbol, string color)
    {
        UseSession();
        var page = Render<IndexPage>();

        StartGameAndWinBid(page, button);

        Assert.Equal("Your turn. Play a card.", Status(page));
        Assert.Equal(symbol, page.Find(".suit-mark").TextContent);
        Assert.Contains(color, page.Find(".suit-mark").ClassName, StringComparison.Ordinal);
        Assert.Equal(4, page.FindAll(".trump-corner." + color).Count);
        Assert.Equal("US", page.Find(".trump .who.us").TextContent);
        Assert.Contains("7", page.Find(".bid-amount").TextContent, StringComparison.Ordinal);
        Assert.Empty(page.FindAll(".bid-badge"));
    }



    [Fact]
    public void SelectTrump_when_Ace_High_is_chosen_shows_no_trump_mark()
    {
        UseSession();
        var page = Render<IndexPage>();

        StartGameAndWinBid(page, "Ace High (no trump)");

        Assert.Equal("—", page.Find(".suit-mark").TextContent);
        Assert.Empty(page.FindAll(".trump-corner"));
    }



    [Fact]
    public void Bidding_when_an_opponent_wins_shows_THEM_and_lets_the_AI_choose_trump()
    {
        var session = UseSession
        (
            new ScriptedAi((_, player) => player == PlayerPosition.East ? new BidAction.NumberBid(7) : BidAction.PassBid.Instance)
        );
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        ClickButton(page, "Pass");

        Assert.Equal("THEM", page.Find(".trump .who.them").TextContent);
        Assert.Equal(GamePhase.TrickPlay, session.CurrentState!.Phase);
    }



    [Fact]
    public void Bidding_when_an_opponent_calls_Hawsey_shows_it_and_makes_the_exchange_for_them()
    {
        var session = UseSession
        (
            new ScriptedAi((_, player) => player == PlayerPosition.East ? BidAction.HawseyBid.Instance : BidAction.PassBid.Instance)
        );
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        Assert.Equal(GamePhase.TrickPlay, session.CurrentState!.Phase);
        Assert.Empty(page.FindAll(".modal-card"));
        Assert.Contains("Hawsey", page.Find(".bid-amount").TextContent, StringComparison.Ordinal);

        ClickButton(page, "Game log");

        Assert.Contains(page.FindAll(".ai-log-entry"), e => string.Equals(e.TextContent, "East calls Hawsey.", StringComparison.Ordinal));
    }



    [Fact]
    public void HawseyExchange_when_two_cards_are_chosen_discards_them_and_plays_on()
    {
        var session = UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        page.Find(".modal-card > button.btn.red").Click();

        Assert.Equal("Pick trump.", Status(page));

        ClickButton(page, "♠ Spades");

        Assert.Equal("Hawsey! Pick 2 cards to discard.", Status(page));
        Assert.True(page.FindAll("button").First(b => string.Equals(b.TextContent.Trim(), Confirm, StringComparison.Ordinal)).HasAttribute("disabled"));

        page.FindAll(".hand .card")[0].Click();
        page.FindAll(".hand .card")[3].Click();
        page.FindAll(".hand .card")[4].Click();

        Assert.Equal(2, page.FindAll(".hand .card.selected-discard").Count);
        Assert.Contains("2 / 2", page.FindAll(".modal-sub")[1].TextContent, StringComparison.Ordinal);

        page.FindAll(".hand .card")[3].Click();

        Assert.Single(page.FindAll(".hand .card.selected-discard"));

        page.FindAll(".hand .card")[5].Click();
        ClickButton(page, Confirm);

        Assert.Empty(page.FindAll(".modal-card"));
        Assert.Equal(12, page.FindAll(".hand .card").Count);
        Assert.Equal(GamePhase.TrickPlay, session.CurrentState!.Phase);
        Assert.Contains("Hawsey", page.Find(".bid-amount").TextContent, StringComparison.Ordinal);
    }



    private static void StartHawseyExchange(IRenderedComponent<IndexPage> page)
    {
        ClickButton(page, NewGame);
        page.Find(".modal-card > button.btn.red").Click();
        ClickButton(page, "♠ Spades");
    }



    private static string[] SelectedText(IRenderedComponent<IndexPage> page) =>
        page
            .FindAll(".hand .card.selected-discard")
            .Select(c => string.Concat(c.TextContent.Where(ch => !char.IsWhiteSpace(ch))))
            .ToArray();



    [Fact]
    public void HawseyExchange_when_both_copies_of_a_card_are_chosen_discards_both()
    {
        var session = UseSession();
        var page = Render<IndexPage>();

        StartHawseyExchange(page);

        // Seed 42 deals two nines of spades, in positions 1 and 2.
        Assert.Equal("9♠", HandText(page)[1]);
        Assert.Equal("9♠", HandText(page)[2]);

        page.FindAll(".hand .card")[1].Click();

        Assert.Single(page.FindAll(".hand .card.selected-discard"));

        page.FindAll(".hand .card")[2].Click();

        Assert.Equal(["9♠", "9♠"], SelectedText(page));

        ClickButton(page, Confirm);

        Assert.Equal(GamePhase.TrickPlay, session.CurrentState!.Phase);
        Assert.Equal(12, page.FindAll(".hand .card").Count);
        Assert.DoesNotContain("9♠", HandText(page));
    }



    [Fact]
    public void HawseyExchange_when_one_copy_is_clicked_twice_deselects_it_and_leaves_the_other_copy_alone()
    {
        UseSession();
        var page = Render<IndexPage>();

        StartHawseyExchange(page);

        page.FindAll(".hand .card")[1].Click();
        page.FindAll(".hand .card")[2].Click();
        page.FindAll(".hand .card")[1].Click();

        Assert.Equal(["9♠"], SelectedText(page));
        Assert.Contains("selected-discard", page.FindAll(".hand .card")[2].ClassName, StringComparison.Ordinal);
    }



    [Theory]
    [InlineData(3, 3, 7, "the selected card itself is dragged right")]
    [InlineData(5, 5, 3, "the selected card itself is dragged left")]
    [InlineData(5, 3, 8, "another card is dragged across it, left to right")]
    [InlineData(5, 9, 3, "another card is dragged across it, right to left")]
    [InlineData(4, 7, 9, "another card moves entirely to its right")]
    public void HawseyExchange_when_a_card_is_dragged_keeps_the_selection_on_the_same_card(int selected, int dragged, int dropOn, string scenario)
    {
        UseSession();
        var page = Render<IndexPage>();

        StartHawseyExchange(page);

        var expected = HandText(page)[selected];

        page.FindAll(".hand .card")[selected].Click();
        page.FindAll(".hand .card")[dragged].DragStart();
        page.FindAll(".hand .card")[dropOn].Drop();

        Assert.True(string.Equals(Assert.Single(SelectedText(page)), expected, StringComparison.Ordinal), scenario);
    }



    [Fact]
    public void CardClick_when_a_legal_card_is_played_moves_it_to_the_table()
    {
        // East wins the bid and leads, so a card is on the table when the human plays. (When
        // the human wins a trick and is to lead, the session clears the table first.)
        var session = UseSession
        (
            new ScriptedAi((_, player) => player == PlayerPosition.East ? new BidAction.NumberBid(7) : BidAction.PassBid.Instance)
        );
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        ClickButton(page, "Pass");

        Assert.Single(page.FindAll(".trick-slot.east .trick-card"));
        Assert.Equal("Led by Right", page.Find(".led").TextContent);
        Assert.NotEqual("—", page.Find(".winner-card").TextContent);

        var before = session.CurrentState!.Hands[PlayerPosition.South].ToList();

        page.FindAll(".hand .card.playable")[0].Click();

        var played = before.ToList();

        foreach (var card in session.CurrentState!.Hands[PlayerPosition.South])
        {
            played.Remove(card);
        }

        Assert.Equal(11, page.FindAll(".hand .card").Count);
        Assert.Contains
        (
            session.CurrentState.CompletedTricks[0].Cards,
            p => p.Player == PlayerPosition.South && p.Card == Assert.Single(played)
        );
    }



    [Fact]
    public void CardClick_when_it_is_not_the_humans_turn_to_play_does_nothing()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        Assert.Equal(12, page.FindAll(".hand .card.disabled").Count);

        page.FindAll(".hand .card")[0].Click();

        Assert.Equal(12, page.FindAll(".hand .card").Count);
        Assert.Equal("Your turn to bid.", Status(page));
    }



    [Fact]
    public void Hand_when_dealt_is_sorted_by_suit()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        var suits = HandText(page)
            .Select(c => "♠♥♣♦".IndexOf(c[^1]))
            .ToArray();

        Assert.Equal(suits.Order().ToArray(), suits);
    }



    [Fact]
    public void Drop_when_a_card_is_dragged_right_inserts_it_before_the_target()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        var before = HandText(page);

        page.FindAll(".hand .card")[0].DragStart();

        Assert.Contains("dragging", page.FindAll(".hand .card")[0].ClassName, StringComparison.Ordinal);

        page.FindAll(".hand .card")[3].DragOver();
        page.FindAll(".hand .card")[3].Drop();

        var after = HandText(page);

        Assert.Equal(before[0], after[2]);
        Assert.Equal(before[1], after[0]);
        Assert.Equal(before[3], after[3]);
        Assert.Empty(page.FindAll(".hand .card.dragging"));
    }



    [Fact]
    public void Drop_when_a_card_is_dragged_left_takes_the_targets_place()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        var before = HandText(page);

        page.FindAll(".hand .card")[3].DragStart();
        page.FindAll(".hand .card")[0].Drop();

        var after = HandText(page);

        Assert.Equal(before[3], after[0]);
        Assert.Equal(before[0], after[1]);
    }



    [Fact]
    public void Drop_when_a_card_is_dropped_on_itself_or_nothing_was_dragged_keeps_the_order()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        var before = HandText(page);

        page.FindAll(".hand .card")[2].DragStart();
        page.FindAll(".hand .card")[2].Drop();
        page.FindAll(".hand .card")[4].Drop();

        Assert.Equal(before, HandText(page));
    }



    [Fact]
    public void DragEnd_when_the_drag_is_abandoned_clears_the_dragging_mark()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);

        page.FindAll(".hand .card")[1].DragStart();
        page.FindAll(".hand .card")[1].DragEnd();

        Assert.Empty(page.FindAll(".hand .card.dragging"));
    }



    [Fact]
    public void Hand_when_a_card_is_played_keeps_the_order_the_player_chose()
    {
        UseSession();
        var page = Render<IndexPage>();

        StartGameAndWinBid(page, "Ace High (no trump)");

        page.FindAll(".hand .card")[0].DragStart();
        page.FindAll(".hand .card")[5].Drop();

        var moved = HandText(page);

        page.FindAll(".hand .card")[1].Click();

        Assert.Equal
        (
            moved.Where((_, i) => i != 1).ToArray(),
            HandText(page)
        );
    }



    [Fact]
    public void GameLog_when_opened_lists_the_bids_and_tricks_and_closes_again()
    {
        UseSession();
        var page = Render<IndexPage>();

        StartGameAndWinBid(page, "♠ Spades");

        page.FindAll(".hand .card.playable")[0].Click();
        page.FindAll(".hand .card.playable")[0].Click();

        ClickButton(page, "Game log");

        var entries = page
            .FindAll(".ai-log-entry")
            .Select(e => e.TextContent)
            .ToArray();

        Assert.Contains("East passes.", entries);
        Assert.Contains("South bids 7.", entries);
        Assert.Contains(entries, e => e.StartsWith("Trick 1: ", StringComparison.Ordinal));

        ClickButton(page, "close");

        Assert.Empty(page.FindAll(".ai-log"));
    }



    [Fact]
    public void RoundSummary_when_a_round_ends_shows_the_engines_score_and_leads_to_the_next_deal()
    {
        var session = UseSession();
        var page = Render<IndexPage>();

        StartGameAndWinBid(page, "♠ Spades");
        PlayUntil(page, "Next round");

        Assert.Equal("Round complete.", Status(page));

        var state = session.CurrentState!;
        var score = state.RoundScore!;
        var grid = page
            .FindAll(".rs-grid div")
            .Select(d => d.TextContent)
            .ToArray();

        Assert.Equal("Round complete", page.Find(".round-summary h2").TextContent);
        Assert.Contains(score.BiddingTeamDelta > 0 ? "made it" : "went set", page.Find(".rs-bid").TextContent, StringComparison.Ordinal);
        Assert.Equal("7", page.Find(".rs-bid strong").TextContent);
        Assert.Equal(score.TricksFor(Team.NorthSouth).ToString(), grid[4]);
        Assert.Equal(score.TricksFor(Team.EastWest).ToString(), grid[5]);
        Assert.Equal(state.NorthSouthScore.ToString(), grid[10]);
        Assert.Equal(state.EastWestScore.ToString(), grid[11]);

        ClickButton(page, "Next round");

        Assert.Empty(page.FindAll(".round-summary"));
        Assert.Equal(PlayerPosition.East, session.CurrentState!.Dealer);
        Assert.Single(page.FindAll(".seat.right .dealer-puck"));
    }



    [Fact]
    public void GameOver_when_a_team_reaches_the_target_names_the_winner_and_offers_another_game()
    {
        var session = UseSession(new SimpleAiStrategy());
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        PlayUntil(page, "Play again");

        Assert.Equal("Game over.", Status(page));

        var expected = session.CurrentState!.Winner == Team.NorthSouth ? "US" : "THEM";

        Assert.Equal(expected + " wins the game!", page.Find(".round-summary h2").TextContent);

        ClickButton(page, "Play again");

        Assert.Empty(page.FindAll(".round-summary"));
        Assert.Equal("0", page.Find(".score .col-us .value").TextContent);
        Assert.Equal("0", page.Find(".score .col-them .value").TextContent);
    }



    [Fact]
    public void NewGame_when_a_picker_is_open_closes_it_and_deals_again()
    {
        UseSession();
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        ClickButton(page, "Bid 7");

        Assert.Equal("Pick trump.", Status(page));

        ClickButton(page, NewGame);

        Assert.DoesNotContain("Pick trump", page.Markup, StringComparison.Ordinal);
        Assert.Equal("Your turn to bid.", Status(page));
    }



    [Fact]
    public void NewGame_when_clicked_during_an_AI_pause_shows_only_the_new_games_turn()
    {
        UseSession(paced: true);
        var page = Render<IndexPage>();

        ClickButton(page, NewGame);
        ClickButton(page, NewGame);

        page.WaitForAssertion(() => Assert.Equal("Your turn to bid.", Status(page)), PacedTimeout);

        Assert.Single(page.FindAll(".bid-buttons"));
    }



    [Fact]
    public async Task StateChanged_when_the_session_moves_on_redraws_until_the_page_is_disposed()
    {
        var session = UseSession();
        var page = Render<IndexPage>();

        session.StartNewGame();

        page.WaitForAssertion(() => Assert.Equal(12, page.FindAll(".hand .card").Count));

        var renders = page.RenderCount;

        await DisposeComponentsAsync();
        session.StartNewGame();

        Assert.Equal(renders, page.RenderCount);
    }
}
