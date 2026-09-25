using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// Whole-round and guard-clause behaviour of <see cref="GameEngine"/> that the
/// Stryker run (#36) showed was not pinned by any test: trick sizes and flags in
/// normal vs Hawsey rounds, the deal, and the phase / legality guards.
/// </summary>
public class GameEngineRoundTests
{
    private readonly GameEngine _engine = new();



    private GameState StartAtNorth(int seed = 42) =>
        _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(seed));



    /// <summary>First bidder (East, left of the North dealer) calls Hawsey; everyone else would pass.</summary>
    private static TestPlayerStrategy FirstBidderCallsHawsey() =>
        new(new Queue<BidAction>([BidAction.HawseyBid.Instance]));



    private GameState ReachFirstTrick()
    {
        var state = StartAtNorth();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (state.Phase == GamePhase.Bidding)
        {
            state = _engine.PlaceBid(state, biddingPhase.GetNextBidder()!.Value, BidAction.PassBid.Instance, biddingPhase);
        }

        return _engine.SelectTrump(state, Suit.Hearts);
    }



    private GameState ReachHawseyExchange()
    {
        var state = StartAtNorth();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);
        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.HawseyBid.Instance, biddingPhase);

        return _engine.SelectTrump(state, Suit.Hearts);
    }



    [Fact]
    public void StartGame_is_not_a_Hawsey_round()
    {
        Assert.False(StartAtNorth().IsHawseyRound);
    }



    [Fact]
    public void PlaceBid_while_bidding_continues_is_not_a_Hawsey_round()
    {
        var state = StartAtNorth();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        var next = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);

        Assert.Equal(GamePhase.Bidding, next.Phase);
        Assert.False(next.IsHawseyRound);
    }



    [Fact]
    public void PlaceBid_when_normal_bidding_completes_has_no_Hawsey_bidder()
    {
        var state = StartAtNorth();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (state.Phase == GamePhase.Bidding)
        {
            state = _engine.PlaceBid(state, biddingPhase.GetNextBidder()!.Value, BidAction.PassBid.Instance, biddingPhase);
        }

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.False(state.IsHawseyRound);
        Assert.Null(state.HawseyBidder);
    }



    [Fact]
    public void SelectTrump_in_a_Hawsey_round_keeps_the_Hawsey_flag()
    {
        var state = ReachHawseyExchange();

        Assert.Equal(GamePhase.HawseyExchange, state.Phase);
        Assert.True(state.IsHawseyRound);
        Assert.Equal(PlayerPosition.East, state.HawseyBidder);
    }



    [Fact]
    public void ExchangeHawseyCards_when_the_exchange_is_already_done_throws()
    {
        var state = ReachHawseyExchange();
        var bidder = state.Hands[PlayerPosition.East];
        var partner = state.Hands[PlayerPosition.West];
        state = _engine.ExchangeHawseyCards(state, [bidder[0], bidder[1]], [partner[0], partner[1]]);
        Assert.Equal(GamePhase.TrickPlay, state.Phase);

        bidder = state.Hands[PlayerPosition.East];
        partner = state.Hands[PlayerPosition.West];

        Assert.Throws<InvalidOperationException>(() => _engine.ExchangeHawseyCards(state, [bidder[0], bidder[1]], [partner[0], partner[1]]));
    }



    [Fact]
    public void PlayCard_when_state_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _engine.PlayCard(null!, PlayerPosition.East, new Card(Rank.Ace, Suit.Spades)));

        Assert.Equal("state", ex.ParamName);
    }



    [Fact]
    public void StartGame_deals_every_card_of_the_deck_exactly_once()
    {
        var state = StartAtNorth();

        var dealt = state.Hands.Values.SelectMany(h => h).OrderBy(c => c.Suit).ThenBy(c => c.Rank);
        var deck = Deck.CreatePinochleDeck().OrderBy(c => c.Suit).ThenBy(c => c.Rank);

        Assert.Equal(deck, dealt);
    }



    [Fact]
    public void Normal_round_has_four_card_tricks_and_no_Hawsey_state()
    {
        var state = RoundDriver.PlayOneRound(StartAtNorth(), new TestPlayerStrategy());

        Assert.False(state.IsHawseyRound);
        Assert.Null(state.HawseyBidder);
        Assert.All(state.CompletedTricks, t => Assert.Equal(4, t.Cards.Count));
    }



    [Fact]
    public void Hawsey_round_has_three_card_tricks_and_records_the_bidder()
    {
        var state = RoundDriver.PlayOneRound(StartAtNorth(), FirstBidderCallsHawsey());

        Assert.True(state.IsHawseyRound);
        Assert.Equal(PlayerPosition.East, state.HawseyBidder);
        Assert.Equal(12, state.CompletedTricks.Count);
        Assert.All(state.CompletedTricks, t => Assert.Equal(3, t.Cards.Count));
    }



    [Fact]
    public void StartNextRound_after_a_Hawsey_round_clears_the_Hawsey_state()
    {
        var scored = RoundDriver.PlayOneRound(StartAtNorth(), FirstBidderCallsHawsey());
        Assert.Equal(GamePhase.RoundScoring, scored.Phase);

        var next = _engine.StartNextRound(scored, new Random(7));

        Assert.False(next.IsHawseyRound);
        Assert.Null(next.HawseyBidder);
    }



    [Fact]
    public void PlayCard_when_the_card_is_not_the_first_in_hand_plays_that_card()
    {
        var state = ReachFirstTrick();
        var leader = state.NextToAct!.Value;
        var hand = state.Hands[leader];
        var card = hand[hand.Count - 1];

        var next = _engine.PlayCard(state, leader, card);

        Assert.Equal(11, next.Hands[leader].Count);
        Assert.Equal(card, next.CurrentTrick!.Plays[0].Card);
    }



    [Fact]
    public void PlayCard_when_the_card_is_not_a_legal_play_throws()
    {
        var state = ReachFirstTrick();
        var leader = state.NextToAct!.Value;
        var notHeld = Deck.CreatePinochleDeck().First(c => !state.Hands[leader].Contains(c));

        Assert.Throws<InvalidOperationException>(() => _engine.PlayCard(state, leader, notHeld));
    }



    [Fact]
    public void PlayCard_when_still_bidding_throws()
    {
        var state = StartAtNorth();

        Assert.Throws<InvalidOperationException>(() => _engine.PlayCard(state, PlayerPosition.East, state.Hands[PlayerPosition.East][0]));
    }



    [Fact]
    public void PlaceBid_when_trick_play_has_started_throws_even_if_player_and_bidding_phase_agree()
    {
        // The stuck dealer (North) leads the first trick. A bidding phase dealt by West
        // makes North its next bidder, so only the phase guard can reject this bid.
        var state = ReachFirstTrick();
        Assert.Equal(PlayerPosition.North, state.NextToAct);
        var agreeingPhase = new BiddingPhase(PlayerPosition.West, state.Rules.MinimumBid);

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, agreeingPhase));
    }



    [Fact]
    public void PlaceBid_when_it_is_not_the_players_turn_throws_even_if_the_bidding_phase_agrees()
    {
        // East is next to bid in the game; a phase dealt by East makes South its next
        // bidder, so only the engine's turn guard can reject South's bid.
        var state = StartAtNorth();
        var agreeingPhase = new BiddingPhase(PlayerPosition.East, state.Rules.MinimumBid);

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, agreeingPhase));
    }



    [Fact]
    public void PlaceBid_when_bidding_phase_is_null_throws()
    {
        var state = StartAtNorth();

        var ex = Assert.Throws<ArgumentNullException>(() => _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, null!));

        Assert.Equal("biddingPhase", ex.ParamName);
    }



    [Fact]
    public void ExchangeHawseyCards_when_not_a_Hawsey_round_throws()
    {
        var state = ReachFirstTrick();
        var hand = state.Hands[PlayerPosition.East];

        Assert.Throws<InvalidOperationException>(() => _engine.ExchangeHawseyCards(state, [hand[0], hand[1]], [hand[2], hand[3]]));
    }
}
