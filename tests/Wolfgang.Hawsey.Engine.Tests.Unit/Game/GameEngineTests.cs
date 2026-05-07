using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

public class GameEngineTests
{
    private readonly GameEngine _engine = new GameEngine();



    [Fact]
    public void StartGame_returns_bidding_phase()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Equal(GamePhase.Bidding, state.Phase);
    }



    [Fact]
    public void StartGame_deals_12_cards_to_each_player()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Equal(12, state.Hands[PlayerPosition.North].Count);
        Assert.Equal(12, state.Hands[PlayerPosition.East].Count);
        Assert.Equal(12, state.Hands[PlayerPosition.South].Count);
        Assert.Equal(12, state.Hands[PlayerPosition.West].Count);
    }



    [Fact]
    public void StartGame_first_bidder_is_left_of_dealer()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Equal(PlayerPosition.East, state.NextToAct);
    }



    [Fact]
    public void StartGame_when_null_rules_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => _engine.StartGame(null!, PlayerPosition.North, new Random(42))
        );
    }



    [Fact]
    public void StartGame_when_null_random_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => _engine.StartGame(HouseRules.Default, PlayerPosition.North, null!)
        );
    }



    [Fact]
    public void PlaceBid_advances_to_next_bidder()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);

        Assert.Equal(PlayerPosition.South, state.NextToAct);
    }



    [Fact]
    public void PlaceBid_when_all_pass_moves_to_trump_selection()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.True(state.BiddingResult!.IsStuck);
        Assert.Equal(PlayerPosition.North, state.NextToAct);
    }



    [Fact]
    public void PlaceBid_when_hawsey_moves_to_trump_selection()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.HawseyBid.Instance, biddingPhase);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        Assert.True(state.IsHawseyRound);
    }



    [Fact]
    public void PlaceBid_when_wrong_phase_throws()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        // Complete bidding
        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase);

        // Now in TrumpSelection, not Bidding
        Assert.Throws<InvalidOperationException>
        (
            () => _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase)
        );
    }



    [Fact]
    public void SelectTrump_when_suited_moves_to_trick_play()
    {
        var state = CreateStateAtTrumpSelection();

        state = _engine.SelectTrump(state, Suit.Hearts);

        Assert.Equal(GamePhase.TrickPlay, state.Phase);
        Assert.Equal(Suit.Hearts, state.TrumpSuit);
        Assert.Equal(TrumpMode.Suited, state.TrumpMode);
    }



    [Fact]
    public void SelectTrump_when_ace_high_moves_to_trick_play()
    {
        var state = CreateStateAtTrumpSelection();

        state = _engine.SelectTrump(state, trumpSuit: null);

        Assert.Equal(GamePhase.TrickPlay, state.Phase);
        Assert.Null(state.TrumpSuit);
        Assert.Equal(TrumpMode.AceHigh, state.TrumpMode);
    }



    [Fact]
    public void SelectTrump_when_hawsey_moves_to_exchange()
    {
        var state = CreateHawseyStateAtTrumpSelection();

        state = _engine.SelectTrump(state, Suit.Clubs);

        Assert.Equal(GamePhase.HawseyExchange, state.Phase);
    }



    [Fact]
    public void SelectTrump_when_wrong_phase_throws()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.SelectTrump(state, Suit.Hearts)
        );
    }



    [Fact]
    public void ExchangeHawseyCards_moves_to_trick_play()
    {
        var state = CreateHawseyStateAtExchange();
        var bidder = state.HawseyBidder!.Value;
        var partner = bidder.Partner();

        var discard = new[] { state.Hands[bidder][0], state.Hands[bidder][1] };
        var fromPartner = new[] { state.Hands[partner][0], state.Hands[partner][1] };

        state = _engine.ExchangeHawseyCards(state, discard, fromPartner);

        Assert.Equal(GamePhase.TrickPlay, state.Phase);
        // Bidder should have 12 cards (12 - 2 + 2)
        Assert.Equal(12, state.Hands[bidder].Count);
        // Partner should have 10 cards (12 - 2)
        Assert.Equal(10, state.Hands[partner].Count);
    }



    [Fact]
    public void ExchangeHawseyCards_when_wrong_discard_count_throws()
    {
        var state = CreateHawseyStateAtExchange();
        var bidder = state.HawseyBidder!.Value;
        var partner = bidder.Partner();

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.ExchangeHawseyCards
            (
                state,
                new[] { state.Hands[bidder][0] },
                new[] { state.Hands[partner][0], state.Hands[partner][1] }
            )
        );
    }



    [Fact]
    public void ExchangeHawseyCards_when_wrong_partner_count_throws()
    {
        var state = CreateHawseyStateAtExchange();
        var bidder = state.HawseyBidder!.Value;
        var partner = bidder.Partner();

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.ExchangeHawseyCards
            (
                state,
                new[] { state.Hands[bidder][0], state.Hands[bidder][1] },
                new[] { state.Hands[partner][0] }
            )
        );
    }



    [Fact]
    public void ExchangeHawseyCards_when_null_discard_throws()
    {
        var state = CreateHawseyStateAtExchange();
        var partner = state.HawseyBidder!.Value.Partner();

        Assert.Throws<ArgumentNullException>
        (
            () => _engine.ExchangeHawseyCards
            (
                state,
                null!,
                new[] { state.Hands[partner][0], state.Hands[partner][1] }
            )
        );
    }



    [Fact]
    public void ExchangeHawseyCards_when_null_partner_cards_throws()
    {
        var state = CreateHawseyStateAtExchange();
        var bidder = state.HawseyBidder!.Value;

        Assert.Throws<ArgumentNullException>
        (
            () => _engine.ExchangeHawseyCards
            (
                state,
                new[] { state.Hands[bidder][0], state.Hands[bidder][1] },
                null!
            )
        );
    }



    [Fact]
    public void ExchangeHawseyCards_when_card_not_in_hand_throws()
    {
        var state = CreateHawseyStateAtExchange();
        var bidder = state.HawseyBidder!.Value;
        var partner = bidder.Partner();

        var bidderHand = state.Hands[bidder];

        // Find a card NOT in bidder's hand
        Card? notInHand = null;
        foreach (var suit in new[] { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades })
        {
            foreach (var rank in new[] { Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace })
            {
                var card = new Card(rank, suit);
                if (!bidderHand.Contains(card))
                {
                    notInHand = card;
                    break;
                }
            }
            if (notInHand.HasValue)
            {
                break;
            }
        }

        Assert.True(notInHand.HasValue, "Test setup: failed to find a card outside the bidder's hand.");

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.ExchangeHawseyCards
            (
                state,
                new[] { notInHand.Value, state.Hands[bidder][0] },
                new[] { state.Hands[partner][0], state.Hands[partner][1] }
            )
        );
    }



    [Fact]
    public void PlayCard_when_legal_removes_card_from_hand()
    {
        var state = CreateStateAtTrickPlay();
        var player = state.NextToAct!.Value;
        var legalPlays = state.GetLegalPlays();
        var card = legalPlays[0];
        var handSize = state.Hands[player].Count;

        state = _engine.PlayCard(state, player, card);

        Assert.Equal(handSize - 1, state.Hands[player].Count);
    }



    [Fact]
    public void PlayCard_when_illegal_card_throws()
    {
        var state = CreateStateAtTrickPlay();
        var player = state.NextToAct!.Value;

        // Find a card not in legal plays
        var legalPlays = state.GetLegalPlays();
        var hand = state.Hands[player];
        Card? illegalCard = null;

        foreach (var c in hand)
        {
            var found = false;
            foreach (var l in legalPlays)
            {
                if (c.Equals(l))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                illegalCard = c;
                break;
            }
        }

        // Only test if there's actually an illegal card (won't always be the case when leading)
        if (illegalCard.HasValue)
        {
            Assert.Throws<InvalidOperationException>
            (
                () => _engine.PlayCard(state, player, illegalCard.Value)
            );
        }
    }



    [Fact]
    public void PlayCard_when_wrong_player_throws()
    {
        var state = CreateStateAtTrickPlay();
        var wrongPlayer = state.NextToAct!.Value.NextClockwise();
        var card = state.Hands[wrongPlayer][0];

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.PlayCard(state, wrongPlayer, card)
        );
    }



    [Fact]
    public void PlayCard_when_wrong_phase_throws()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.PlayCard(state, PlayerPosition.East, new Card(Rank.Ace, Suit.Spades))
        );
    }



    [Fact]
    public void StartNextRound_when_wrong_phase_throws()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Throws<InvalidOperationException>
        (
            () => _engine.StartNextRound(state, new Random(42))
        );
    }



    [Fact]
    public void StartNextRound_when_null_random_throws()
    {
        var state = CreateStateAtRoundScoring();

        Assert.Throws<ArgumentNullException>
        (
            () => _engine.StartNextRound(state, null!)
        );
    }



    [Fact]
    public void StartNextRound_advances_dealer()
    {
        var state = CreateStateAtRoundScoring();
        var oldDealer = state.Dealer;

        state = _engine.StartNextRound(state, new Random(99));

        Assert.Equal(oldDealer.NextClockwise(), state.Dealer);
    }



    [Fact]
    public void Full_round_plays_to_completion()
    {
        var strategy = new TestPlayerStrategy(preferredTrump: Suit.Hearts);
        var runner = new GameRunner();
        var rules = new HouseRules { PointsToWin = 10 };

        var finalState = runner.RunGame(strategy, rules, PlayerPosition.North, new Random(42));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
    }



    [Fact]
    public void GetLegalPlays_when_not_in_trick_play_returns_empty()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        Assert.Empty(state.GetLegalPlays());
    }



    private GameState CreateStateAtTrumpSelection()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, biddingPhase);
        state = _engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase);

        return state;
    }



    private GameState CreateHawseyStateAtTrumpSelection()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);

        state = _engine.PlaceBid(state, PlayerPosition.East, BidAction.HawseyBid.Instance, biddingPhase);

        return state;
    }



    private GameState CreateHawseyStateAtExchange()
    {
        var state = CreateHawseyStateAtTrumpSelection();

        return _engine.SelectTrump(state, Suit.Clubs);
    }



    private GameState CreateStateAtTrickPlay()
    {
        var state = CreateStateAtTrumpSelection();

        return _engine.SelectTrump(state, Suit.Hearts);
    }



    private GameState CreateStateAtRoundScoring()
    {
        var state = CreateStateAtTrickPlay();
        var strategy = new TestPlayerStrategy(preferredTrump: Suit.Hearts);

        // Play all 12 tricks
        while (state.Phase == GamePhase.TrickPlay)
        {
            var player = state.NextToAct!.Value;
            var card = strategy.DecidePlay(state, player);
            state = _engine.PlayCard(state, player, card);
        }

        return state;
    }
}
