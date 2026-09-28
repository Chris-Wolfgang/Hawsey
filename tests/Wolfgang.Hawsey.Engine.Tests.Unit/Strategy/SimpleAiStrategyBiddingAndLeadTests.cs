using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Strategy;

/// <summary>
/// The hand-strength parts of <see cref="SimpleAiStrategy"/>: how it bids against
/// <see cref="GameState.HighBid"/>, the trump it names, not overtaking its partner, and
/// leading a card nothing left can beat. A hand's score is its best suit's length, plus
/// that suit's jacks, plus the aces in the other suits, minus one.
/// </summary>
public class SimpleAiStrategyBiddingAndLeadTests
{
    private static readonly PlayerPosition[] Seats =
        [PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West];

    private readonly SimpleAiStrategy _ai = new();



    private static Card C(Rank rank, Suit suit) => new(rank, suit);



    /// <summary>A bid has no value equality, so compare the amount.</summary>
    private static void AssertBids(int amount, BidAction bid) =>
        Assert.Equal(amount, Assert.IsType<BidAction.NumberBid>(bid).Amount);



    /// <summary>Scores 9 with spades: 6 spades + 2 jacks + 2 outside aces - 1.</summary>
    private static readonly Card[] NineTrickHand =
    [
        C(Rank.Jack, Suit.Spades), C(Rank.Jack, Suit.Spades), C(Rank.Ace, Suit.Spades),
        C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades), C(Rank.Ten, Suit.Spades),
        C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds),
        C(Rank.Nine, Suit.Clubs), C(Rank.Nine, Suit.Clubs), C(Rank.Ten, Suit.Clubs), C(Rank.King, Suit.Clubs),
    ];

    /// <summary>Scores 2 in every suit: no jacks, no aces, three of each suit.</summary>
    private static readonly Card[] WeakHand =
    [
        C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Hearts), C(Rank.Queen, Suit.Hearts),
        C(Rank.Nine, Suit.Diamonds), C(Rank.Ten, Suit.Diamonds), C(Rank.Queen, Suit.Diamonds),
        C(Rank.Nine, Suit.Clubs), C(Rank.Ten, Suit.Clubs), C(Rank.Queen, Suit.Clubs),
        C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades), C(Rank.Queen, Suit.Spades),
    ];



    private static GameState Bidding(PlayerPosition seat, Card[] hand, PlayerPosition dealer = PlayerPosition.North, int highBid = 0, PlayerPosition? highBidder = null) =>
        State(GamePhase.Bidding, seat, hand, dealer, trump: null, currentTrick: null, completedTricks: [], highBid, highBidder);



    private static GameState State
    (
        GamePhase phase,
        PlayerPosition seat,
        Card[] hand,
        PlayerPosition dealer,
        Suit? trump,
        Trick? currentTrick,
        List<TrickResult> completedTricks,
        int highBid = 0,
        PlayerPosition? highBidder = null
    )
    {
        var hands = Seats.ToDictionary(p => p, _ => new List<Card>());
        hands[seat] = hand.ToList();

        return new GameState
        (
            phase,
            dealer,
            hands,
            trump,
            TrumpMode.Suited,
            biddingResult: null,
            completedTricks,
            currentTrick,
            northSouthScore: 0,
            eastWestScore: 0,
            HouseRules.Default,
            nextToAct: seat,
            tricksPlayedInRound: completedTricks.Count,
            isHawseyRound: false,
            hawseyBidder: null,
            highBid,
            highBidder
        );
    }



    [Fact]
    public void DecideBid_with_a_weak_hand_passes()
    {
        Assert.IsType<BidAction.PassBid>(_ai.DecideBid(Bidding(PlayerPosition.East, WeakHand), PlayerPosition.East));
    }



    [Fact]
    public void DecideBid_bids_what_the_hand_is_worth()
    {
        var bid = _ai.DecideBid(Bidding(PlayerPosition.East, NineTrickHand), PlayerPosition.East);

        AssertBids(9, bid);
    }



    [Fact]
    public void DecideBid_never_bids_more_than_eleven()
    {
        // 8 spades + 2 jacks + 4 outside aces - 1 = 13, capped at 11.
        Card[] hand =
        [
            C(Rank.Jack, Suit.Spades), C(Rank.Jack, Suit.Spades), C(Rank.Ace, Suit.Spades), C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades), C(Rank.Queen, Suit.Spades),
            C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds), C(Rank.Ace, Suit.Diamonds),
        ];

        AssertBids(11, _ai.DecideBid(Bidding(PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecideBid_beats_a_lower_high_bid_and_passes_otherwise()
    {
        AssertBids
        (
            9,
            _ai.DecideBid(Bidding(PlayerPosition.South, NineTrickHand, highBid: 8, highBidder: PlayerPosition.East), PlayerPosition.South)
        );
        Assert.IsType<BidAction.PassBid>
        (
            _ai.DecideBid(Bidding(PlayerPosition.South, NineTrickHand, highBid: 9, highBidder: PlayerPosition.East), PlayerPosition.South)
        );
    }



    [Fact]
    public void DecideBid_as_the_stuck_dealer_bids_only_the_minimum()
    {
        var bid = _ai.DecideBid(Bidding(PlayerPosition.North, NineTrickHand, dealer: PlayerPosition.North), PlayerPosition.North);

        AssertBids(HouseRules.Default.MinimumBid, bid);
    }



    [Fact]
    public void DecideBid_as_the_dealer_facing_a_bid_competes_normally()
    {
        var bid = _ai.DecideBid
        (
            Bidding(PlayerPosition.North, NineTrickHand, dealer: PlayerPosition.North, highBid: 7, highBidder: PlayerPosition.West),
            PlayerPosition.North
        );

        AssertBids(9, bid);
    }



    [Fact]
    public void DecideBid_as_the_dealer_with_a_weak_hand_passes_and_is_left_stuck()
    {
        Assert.IsType<BidAction.PassBid>(_ai.DecideBid(Bidding(PlayerPosition.North, WeakHand, dealer: PlayerPosition.North), PlayerPosition.North));
    }



    [Fact]
    public void DecideTrump_names_the_best_scoring_suit_not_just_the_longest()
    {
        // Clubs is longest (5, scores 4); spades has both jacks (4 + 2 - 1 = 5).
        Card[] hand =
        [
            C(Rank.Nine, Suit.Clubs), C(Rank.Ten, Suit.Clubs), C(Rank.Queen, Suit.Clubs), C(Rank.King, Suit.Clubs), C(Rank.Nine, Suit.Clubs),
            C(Rank.Jack, Suit.Spades), C(Rank.Jack, Suit.Spades), C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades),
            C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Hearts), C(Rank.Nine, Suit.Diamonds),
        ];

        Assert.Equal(Suit.Spades, _ai.DecideTrump(Bidding(PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecidePlay_does_not_overtake_a_partner_who_is_winning()
    {
        // West (East's partner) leads the queen of spades and is winning; East could win
        // with the ace but throws the ten.
        var trick = new Trick(Suit.Hearts);
        trick.Play(PlayerPosition.West, C(Rank.Queen, Suit.Spades));
        trick.Play(PlayerPosition.North, C(Rank.Nine, Suit.Spades));
        Card[] hand = [C(Rank.Ace, Suit.Spades), C(Rank.Ten, Suit.Spades)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, trick, []), PlayerPosition.East);

        Assert.Equal(C(Rank.Ten, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_leads_a_trump_nothing_can_beat_over_a_higher_side_ace()
    {
        // Hearts are trump, and both bowers and both aces of hearts are gone, so the king
        // of hearts can't be beaten. The ace of spades is higher, but trumps are still out
        // to ruff it.
        List<TrickResult> gone =
        [
            new(PlayerPosition.North, [new(C(Rank.Jack, Suit.Hearts), PlayerPosition.North, 0), new(C(Rank.Jack, Suit.Hearts), PlayerPosition.East, 1), new(C(Rank.Nine, Suit.Clubs), PlayerPosition.South, 2), new(C(Rank.Nine, Suit.Clubs), PlayerPosition.West, 3)]),
            new(PlayerPosition.North, [new(C(Rank.Jack, Suit.Diamonds), PlayerPosition.North, 0), new(C(Rank.Jack, Suit.Diamonds), PlayerPosition.East, 1), new(C(Rank.Ace, Suit.Hearts), PlayerPosition.South, 2), new(C(Rank.Ace, Suit.Hearts), PlayerPosition.West, 3)]),
        ];
        Card[] hand = [C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Hearts), C(Rank.Ten, Suit.Clubs)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, new Trick(Suit.Hearts), gone), PlayerPosition.East);

        Assert.Equal(C(Rank.King, Suit.Hearts), play);
    }



    [Fact]
    public void DecidePlay_counts_the_left_bowers_as_trumps_still_out()
    {
        // Hearts are trump. All twelve hearts are gone, and so are both aces and kings of
        // clubs, so the queen of clubs is the best club left. But the two left bowers
        // (jacks of diamonds) are trumps too and are still out, so the queen of clubs can
        // be ruffed: not a sure winner. With no sure winner, the AI leads its highest
        // card, the king of spades. (Counting only 12 trumps would lead the queen.)
        Rank[] ranks = [Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace];
        var gone = ranks
            .Select(r => C(r, Suit.Hearts))
            .Concat(ranks.Select(r => C(r, Suit.Hearts)))
            .Concat([C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Clubs), C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Clubs)])
            .ToList();
        List<TrickResult> tricks = [];

        for (var i = 0; i < gone.Count; i += 4)
        {
            tricks.Add
            (
                new
                (
                    PlayerPosition.North,
                    [
                        new(gone[i], PlayerPosition.North, 0),
                        new(gone[i + 1], PlayerPosition.East, 1),
                        new(gone[i + 2], PlayerPosition.South, 2),
                        new(gone[i + 3], PlayerPosition.West, 3),
                    ]
                )
            );
        }

        Card[] hand = [C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Clubs)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, new Trick(Suit.Hearts), tricks), PlayerPosition.East);

        Assert.Equal(C(Rank.King, Suit.Spades), play);
    }



    private static Trick TrickOf(Suit? trump, params (PlayerPosition Player, Card Card)[] plays)
    {
        var trick = new Trick(trump);

        foreach (var (player, card) in plays)
        {
            trick.Play(player, card);
        }

        return trick;
    }



    [Fact]
    public void DecidePlay_with_two_sure_winners_leads_the_higher()
    {
        // No trump. Both aces of clubs are gone, so the king of clubs can't be beaten, and
        // neither can the ace of spades: the AI leads the higher of the two.
        List<TrickResult> gone =
        [
            new(PlayerPosition.North, [new(C(Rank.Ace, Suit.Clubs), PlayerPosition.North, 0), new(C(Rank.Ace, Suit.Clubs), PlayerPosition.East, 1), new(C(Rank.Nine, Suit.Hearts), PlayerPosition.South, 2), new(C(Rank.Nine, Suit.Hearts), PlayerPosition.West, 3)]),
        ];
        Card[] hand = [C(Rank.King, Suit.Clubs), C(Rank.Ace, Suit.Spades), C(Rank.Ten, Suit.Hearts)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, trump: null, new Trick(null), gone), PlayerPosition.East);

        Assert.Equal(C(Rank.Ace, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_does_not_count_the_other_copy_of_the_winning_card_as_winning()
    {
        // North (an opponent) led the king of spades. East's own king of spades only ties,
        // and the first copy keeps the trick, so East can't win and throws the nine.
        Card[] hand = [C(Rank.King, Suit.Spades), C(Rank.Nine, Suit.Spades)];

        var play = _ai.DecidePlay
        (
            State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, TrickOf(Suit.Hearts, (PlayerPosition.North, C(Rank.King, Suit.Spades))), []),
            PlayerPosition.East
        );

        Assert.Equal(C(Rank.Nine, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_when_the_partner_has_been_overtaken_tries_to_win()
    {
        // West (East's partner) led the nine of spades, but North has beaten it with the
        // queen. East is not protecting a partner's winning card, so it takes the trick
        // with the cheapest winner, the king, not the ten.
        Card[] hand = [C(Rank.King, Suit.Spades), C(Rank.Ten, Suit.Spades)];
        var trick = TrickOf(Suit.Hearts, (PlayerPosition.West, C(Rank.Nine, Suit.Spades)), (PlayerPosition.North, C(Rank.Queen, Suit.Spades)));

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, trick, []), PlayerPosition.East);

        Assert.Equal(C(Rank.King, Suit.Spades), play);
    }



    [Fact]
    public void DecideHawseyExchange_keeps_a_low_trump_and_discards_side_cards()
    {
        // Hearts are trump. The nine of hearts is the bidder's lowest card by rank, but it
        // is trump, so the two lowest side cards go instead.
        Card[] bidder = [C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Spades), C(Rank.Queen, Suit.Clubs), C(Rank.Ace, Suit.Diamonds)];
        Card[] partner = [C(Rank.Nine, Suit.Clubs), C(Rank.Ten, Suit.Clubs)];
        var hands = Seats.ToDictionary(p => p, _ => new List<Card>());
        hands[PlayerPosition.South] = bidder.ToList();
        hands[PlayerPosition.North] = partner.ToList();
        var state = new GameState
        (
            GamePhase.HawseyExchange,
            PlayerPosition.West,
            hands,
            Suit.Hearts,
            TrumpMode.Suited,
            biddingResult: null,
            completedTricks: [],
            currentTrick: null,
            northSouthScore: 0,
            eastWestScore: 0,
            HouseRules.Default,
            nextToAct: PlayerPosition.South,
            tricksPlayedInRound: 0,
            isHawseyRound: true,
            hawseyBidder: PlayerPosition.South
        );

        _ai.DecideHawseyExchange(state, PlayerPosition.South, out var discard, out _);

        Assert.Equal([C(Rank.Ten, Suit.Spades), C(Rank.Queen, Suit.Clubs)], discard);
    }



    private static List<TrickResult> TricksOf(IReadOnlyList<Card> cards)
    {
        List<TrickResult> tricks = [];

        for (var i = 0; i < cards.Count; i += 4)
        {
            tricks.Add
            (
                new
                (
                    PlayerPosition.North,
                    [
                        new(cards[i], PlayerPosition.North, 0),
                        new(cards[i + 1], PlayerPosition.East, 1),
                        new(cards[i + 2], PlayerPosition.South, 2),
                        new(cards[i + 3], PlayerPosition.West, 3),
                    ]
                )
            );
        }

        return tricks;
    }



    [Fact]
    public void DecidePlay_when_every_other_trump_is_gone_a_side_card_is_a_sure_winner()
    {
        // Hearts are trump. Of the 14 trumps (12 hearts and the two left bowers), East
        // holds one nine of hearts and the other 13 are gone, as are both aces and kings
        // of clubs. Nothing can beat or ruff the queen of clubs, and nothing can beat the
        // nine of hearts: both are sure winners, and the AI leads the higher, the queen.
        // Miscounting the trumps still out makes the queen look ruffable.
        Rank[] above = [Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace];
        var gone = above
            .SelectMany(r => new[] { C(r, Suit.Hearts), C(r, Suit.Hearts) })
            .Concat([C(Rank.Nine, Suit.Hearts), C(Rank.Jack, Suit.Diamonds), C(Rank.Jack, Suit.Diamonds)])
            .Concat([C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Clubs), C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Clubs)])
            .Concat([C(Rank.Nine, Suit.Spades), C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades)])
            .ToList();
        Card[] hand = [C(Rank.Nine, Suit.Hearts), C(Rank.Queen, Suit.Clubs)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, Suit.Hearts, new Trick(Suit.Hearts), TricksOf(gone)), PlayerPosition.East);

        Assert.Equal(C(Rank.Queen, Suit.Clubs), play);
    }



    [Fact]
    public void DecidePlay_leads_a_lower_sure_winner_over_a_higher_card_that_can_be_beaten()
    {
        // No trump. Both kings and both aces of spades are gone, so the queen of spades
        // can't be beaten; the king of clubs can, by either ace of clubs still out. The AI
        // leads the queen, not its highest card.
        var gone = TricksOf([C(Rank.King, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Ace, Suit.Spades), C(Rank.Ace, Suit.Spades)]);
        Card[] hand = [C(Rank.Queen, Suit.Spades), C(Rank.King, Suit.Clubs)];

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.East, hand, PlayerPosition.North, trump: null, new Trick(null), gone), PlayerPosition.East);

        Assert.Equal(C(Rank.Queen, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_when_an_opponent_ties_the_partners_card_the_partner_still_wins()
    {
        // North (South's partner) led the king of spades and East played the other king.
        // The first copy keeps the trick, so North is winning: South throws the nine
        // rather than spend the ace.
        Card[] hand = [C(Rank.Ace, Suit.Spades), C(Rank.Nine, Suit.Spades)];
        var trick = TrickOf(Suit.Hearts, (PlayerPosition.North, C(Rank.King, Suit.Spades)), (PlayerPosition.East, C(Rank.King, Suit.Spades)));

        var play = _ai.DecidePlay(State(GamePhase.TrickPlay, PlayerPosition.South, hand, PlayerPosition.West, Suit.Hearts, trick, []), PlayerPosition.South);

        Assert.Equal(C(Rank.Nine, Suit.Spades), play);
    }
}
