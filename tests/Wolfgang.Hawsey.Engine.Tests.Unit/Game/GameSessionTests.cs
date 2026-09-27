using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameSession"/> driven synchronously: pacing is off, so every AI loop
/// completes before its task is returned. The concurrent interleavings are covered
/// by the Coyote tests (Wolfgang.Hawsey.UI.Maui.Tests.Concurrency).
/// </summary>
public class GameSessionTests
{
    private readonly GameSession _session = TestGameSessions.Unpaced();



    /// <summary>
    /// Deals, then lets the AI bid up to the human's turn (East passes first, since
    /// North deals).
    /// </summary>
    private async Task StartAndReachHumanBidAsync()
    {
        _session.StartNewGame();
        Assert.True(await _session.AdvanceAiBiddingAsync());
    }



    private async Task ReachHumanTrumpChoiceAsync(BidAction humanBid)
    {
        await StartAndReachHumanBidAsync();
        Assert.True(_session.PlaceHumanBid(humanBid));
        await _session.AdvanceAiBiddingAsync();
        Assert.True(await _session.HandleTrumpSelectionAsync());
    }



    /// <summary>
    /// Plays until the game is over: the human passes every bid, names hearts when
    /// stuck, and plays its first legal card.
    /// </summary>
    private async Task PlayToGameOverAsync()
    {
        _session.StartNewGame();

        var steps = 0;

        while (_session.CurrentState!.Phase != GamePhase.GameOver)
        {
            Assert.True(++steps < 10_000, "The game did not finish.");
            var state = _session.CurrentState;

            switch (state.Phase)
            {
                case GamePhase.Bidding:
                    if (await _session.AdvanceAiBiddingAsync())
                    {
                        Assert.True(_session.PlaceHumanBid(BidAction.PassBid.Instance));
                    }

                    break;

                case GamePhase.TrumpSelection:
                    if (await _session.HandleTrumpSelectionAsync())
                    {
                        Assert.True(_session.SelectTrump(Suit.Hearts));
                    }

                    break;

                case GamePhase.TrickPlay:
                    if (await _session.AdvanceAiPlaysAsync())
                    {
                        Assert.True(_session.PlayHumanCard(_session.CurrentState.GetLegalPlays()[0]));
                    }

                    break;

                default:
                    // Nobody bids Hawsey here, so the only other phase is RoundScoring.
                    Assert.Equal(GamePhase.RoundScoring, state.Phase);
                    _session.StartNextRound();
                    break;
            }
        }
    }



    [Fact]
    public void Before_the_first_game_there_is_no_state_and_it_is_not_the_humans_turn()
    {
        Assert.Null(_session.CurrentState);
        Assert.False(_session.IsHumanTurn);
    }



    [Fact]
    public async Task Before_the_first_game_every_move_is_rejected()
    {
        Assert.False(await _session.AdvanceAiBiddingAsync());
        Assert.False(_session.PlaceHumanBid(BidAction.PassBid.Instance));
        Assert.False(_session.SelectTrump(Suit.Hearts));
        Assert.False(await _session.HandleTrumpSelectionAsync());
        Assert.False(_session.PerformHawseyExchange([], []));
        Assert.False(await _session.HandleHawseyExchangeAsync());
        Assert.False(_session.PlayHumanCard(new Card(Rank.Ace, Suit.Spades)));
        Assert.False(await _session.AdvanceAiPlaysAsync());
    }



    [Fact]
    public void StartNewGame_deals_with_North_dealing_and_raises_StateChanged()
    {
        var raised = 0;
        _session.StateChanged += (_, _) => raised++;

        _session.StartNewGame();

        var state = _session.CurrentState!;
        Assert.Equal(GamePhase.Bidding, state.Phase);
        Assert.Equal(PlayerPosition.North, state.Dealer);
        Assert.Equal(12, state.Hands[GameSession.HumanPosition].Count);
        Assert.Equal(1, raised);
    }



    [Fact]
    public void StartNewGame_uses_the_injected_random_so_a_seed_repeats_the_deal()
    {
        var first = TestGameSessions.Unpaced(seed: 7);
        var second = TestGameSessions.Unpaced(seed: 7);

        first.StartNewGame();
        second.StartNewGame();

        Assert.Equal
        (
            first.CurrentState!.Hands[GameSession.HumanPosition],
            second.CurrentState!.Hands[GameSession.HumanPosition]
        );
    }



    [Fact]
    public async Task AdvanceAiBiddingAsync_stops_at_the_humans_turn()
    {
        await StartAndReachHumanBidAsync();

        Assert.True(_session.IsHumanTurn);
        Assert.Equal(PlayerPosition.South, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public void PlaceHumanBid_before_the_humans_turn_is_rejected()
    {
        _session.StartNewGame();

        Assert.False(_session.PlaceHumanBid(BidAction.PassBid.Instance));
        Assert.Equal(PlayerPosition.East, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public async Task A_human_number_bid_wins_and_the_human_names_trump()
    {
        await ReachHumanTrumpChoiceAsync(new BidAction.NumberBid(7));

        Assert.Equal(GameSession.HumanPosition, _session.CurrentState!.BiddingResult!.Winner);
        Assert.False(await _session.AdvanceAiBiddingAsync());
        Assert.True(_session.SelectTrump(Suit.Spades));
        Assert.Equal(GamePhase.TrickPlay, _session.CurrentState.Phase);
        Assert.Equal(Suit.Spades, _session.CurrentState.TrumpSuit);
    }



    [Fact]
    public async Task SelectTrump_when_the_human_is_not_naming_trump_is_rejected()
    {
        await StartAndReachHumanBidAsync();

        Assert.False(_session.SelectTrump(Suit.Hearts));
    }



    [Fact]
    public async Task When_everyone_passes_the_AI_dealer_is_stuck_and_names_trump()
    {
        await StartAndReachHumanBidAsync();
        Assert.True(_session.PlaceHumanBid(BidAction.PassBid.Instance));
        Assert.False(await _session.AdvanceAiBiddingAsync());

        Assert.False(await _session.HandleTrumpSelectionAsync());

        var state = _session.CurrentState!;
        Assert.True(state.BiddingResult!.IsStuck);
        Assert.Equal(PlayerPosition.North, state.BiddingResult.Winner);
        Assert.Equal(GamePhase.TrickPlay, state.Phase);
        Assert.NotNull(state.TrumpSuit);
    }



    [Fact]
    public async Task A_human_Hawsey_bid_leads_to_the_humans_exchange()
    {
        await ReachHumanTrumpChoiceAsync(BidAction.HawseyBid.Instance);
        Assert.True(_session.SelectTrump(Suit.Clubs));

        Assert.True(await _session.HandleHawseyExchangeAsync());

        var state = _session.CurrentState!;
        Assert.Equal(GamePhase.HawseyExchange, state.Phase);
        Assert.Equal(GameSession.HumanPosition, state.HawseyBidder);

        var hand = state.Hands[GameSession.HumanPosition];
        var partner = state.Hands[PlayerPosition.North];
        Assert.True(_session.PerformHawseyExchange([hand[0], hand[1]], [partner[0], partner[1]]));
        Assert.Equal(GamePhase.TrickPlay, _session.CurrentState!.Phase);
        Assert.False(_session.PerformHawseyExchange([hand[0], hand[1]], [partner[0], partner[1]]));
    }



    [Fact]
    public async Task PlayHumanCard_rejects_an_out_of_turn_or_illegal_card()
    {
        await ReachHumanTrumpChoiceAsync(new BidAction.NumberBid(7));
        Assert.True(_session.SelectTrump(Suit.Spades));

        // The human won the bid, so South leads the first trick.
        var state = _session.CurrentState!;
        Assert.True(_session.IsHumanTurn);
        var notHeld = Deck.CreatePinochleDeck().First(c => !state.Hands[GameSession.HumanPosition].Contains(c));
        Assert.False(_session.PlayHumanCard(notHeld));

        Assert.True(_session.PlayHumanCard(state.GetLegalPlays()[0]));
        Assert.False(_session.PlayHumanCard(state.GetLegalPlays()[0]));
    }



    [Fact]
    public async Task A_whole_game_raises_every_trick_round_and_the_game_over_once()
    {
        var tricks = 0;
        var rounds = 0;
        var gameOvers = new List<GameOverEventArgs>();
        _session.TrickCompleted += (_, _) => tricks++;
        _session.RoundCompleted += (_, _) => rounds++;
        _session.GameOver += (_, e) => gameOvers.Add(e);

        await PlayToGameOverAsync();

        var state = _session.CurrentState!;
        // GameState decides the winner (GameStateWinnerTests covers its rule, #862);
        // the session must announce exactly that team.
        Assert.Single(gameOvers);
        Assert.NotNull(state.Winner);
        Assert.Equal(state.Winner, gameOvers[0].Winner);
        Assert.True(tricks >= 12);
        Assert.True(rounds >= 1);
        Assert.Equal(12 * (rounds + 1), tricks);
    }



    [Fact]
    public async Task After_the_game_is_over_the_AI_loops_do_nothing()
    {
        await PlayToGameOverAsync();

        Assert.False(await _session.AdvanceAiPlaysAsync());
        Assert.False(await _session.AdvanceAiBiddingAsync());
    }



    [Fact]
    public void StartNextRound_outside_round_scoring_does_nothing()
    {
        _session.StartNewGame();
        var raised = 0;
        _session.StateChanged += (_, _) => raised++;

        _session.StartNextRound();

        Assert.Equal(0, raised);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public void PauseBeforeNextRoundAsync_when_unpaced_completes_at_once()
    {
        Assert.True(_session.PauseBeforeNextRoundAsync().IsCompleted);
    }



    [Fact]
    public async Task The_public_constructor_paces_the_AI_and_deals_a_game()
    {
        var service = new GameSession();

        var pause = service.PauseBeforeNextRoundAsync();

        Assert.False(pause.IsCompleted);
        await pause;

        service.StartNewGame();
        Assert.Equal(GamePhase.Bidding, service.CurrentState!.Phase);
    }
}
