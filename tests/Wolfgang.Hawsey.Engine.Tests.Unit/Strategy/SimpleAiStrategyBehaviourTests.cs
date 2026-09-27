using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Strategy;

/// <summary>
/// What <see cref="SimpleAiStrategy"/> chooses, on hand-built positions: the trump it
/// names, the card it leads or follows with, and the Hawsey exchange. The other AI
/// tests only check that its choices are legal.
/// </summary>
public class SimpleAiStrategyBehaviourTests
{
    private static readonly PlayerPosition[] Seats =
        [PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West];

    private readonly SimpleAiStrategy _ai = new();



    private static Card C(Rank rank, Suit suit) => new(rank, suit);



    private static Dictionary<PlayerPosition, List<Card>> Hands(PlayerPosition seat, IEnumerable<Card> hand, PlayerPosition? other = null, IEnumerable<Card>? otherHand = null)
    {
        var hands = Seats.ToDictionary(p => p, _ => new List<Card>());
        hands[seat] = hand.ToList();

        if (other.HasValue && otherHand != null)
        {
            hands[other.Value] = otherHand.ToList();
        }

        return hands;
    }



    /// <summary>
    /// A trick-play position: <paramref name="seat"/> is to act, holding
    /// <paramref name="hand"/>, after <paramref name="plays"/> were made to the trick.
    /// </summary>
    private static GameState Playing(Suit trump, PlayerPosition seat, Card[] hand, params (PlayerPosition Player, Card Card)[] plays)
    {
        var trick = new Trick(trump);

        foreach (var (player, card) in plays)
        {
            trick.Play(player, card);
        }

        return new GameState
        (
            GamePhase.TrickPlay,
            PlayerPosition.North,
            Hands(seat, hand),
            trump,
            TrumpMode.Suited,
            biddingResult: null,
            completedTricks: [],
            currentTrick: trick,
            northSouthScore: 0,
            eastWestScore: 0,
            HouseRules.Default,
            nextToAct: seat,
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );
    }



    private static GameState Holding(PlayerPosition seat, Card[] hand, Suit? trump = null, PlayerPosition? other = null, Card[]? otherHand = null) =>
        new
        (
            GamePhase.TrumpSelection,
            PlayerPosition.North,
            Hands(seat, hand, other, otherHand),
            trump,
            TrumpMode.Suited,
            biddingResult: null,
            completedTricks: [],
            currentTrick: null,
            northSouthScore: 0,
            eastWestScore: 0,
            HouseRules.Default,
            nextToAct: seat,
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );



    [Fact]
    public void DecideTrump_names_the_longest_suit()
    {
        var hand = new[]
        {
            C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Hearts),
            C(Rank.Nine, Suit.Diamonds),
            C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Ace, Suit.Spades),
            C(Rank.Nine, Suit.Clubs),
        };

        Assert.Equal(Suit.Spades, _ai.DecideTrump(Holding(PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecideTrump_on_a_tie_names_the_first_suit_in_suit_order()
    {
        var hand = new[]
        {
            C(Rank.Nine, Suit.Diamonds), C(Rank.Ten, Suit.Diamonds),
            C(Rank.Nine, Suit.Clubs), C(Rank.Ten, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
        };

        Assert.Equal(Suit.Diamonds, _ai.DecideTrump(Holding(PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecidePlay_when_leading_plays_its_highest_card()
    {
        var hand = new[] { C(Rank.Nine, Suit.Clubs), C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Clubs) };

        Assert.Equal(C(Rank.Ace, Suit.Spades), _ai.DecidePlay(Playing(Suit.Hearts, PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecidePlay_when_leading_counts_the_right_bower_as_highest()
    {
        var hand = new[] { C(Rank.Ace, Suit.Spades), C(Rank.Jack, Suit.Hearts), C(Rank.King, Suit.Clubs) };

        Assert.Equal(C(Rank.Jack, Suit.Hearts), _ai.DecidePlay(Playing(Suit.Hearts, PlayerPosition.East, hand), PlayerPosition.East));
    }



    [Fact]
    public void DecidePlay_when_it_can_win_plays_the_lowest_winning_card()
    {
        // West led the queen of spades; East holds three spades, two of which win.
        var hand = new[] { C(Rank.Ace, Suit.Spades), C(Rank.Nine, Suit.Spades), C(Rank.King, Suit.Spades) };

        var play = _ai.DecidePlay
        (
            Playing(Suit.Hearts, PlayerPosition.East, hand, (PlayerPosition.West, C(Rank.Queen, Suit.Spades))),
            PlayerPosition.East
        );

        Assert.Equal(C(Rank.King, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_when_it_cannot_win_plays_its_lowest_card()
    {
        var hand = new[] { C(Rank.King, Suit.Spades), C(Rank.Nine, Suit.Spades), C(Rank.Queen, Suit.Spades) };

        var play = _ai.DecidePlay
        (
            Playing(Suit.Hearts, PlayerPosition.East, hand, (PlayerPosition.West, C(Rank.Ace, Suit.Spades))),
            PlayerPosition.East
        );

        Assert.Equal(C(Rank.Nine, Suit.Spades), play);
    }



    [Fact]
    public void DecidePlay_when_it_cannot_win_keeps_its_trump_and_throws_a_low_side_card()
    {
        // Spades led, North trumped with the queen of hearts. East is void in spades
        // and cannot beat the queen: it keeps the (lower) nine of hearts and throws
        // the king of clubs.
        var hand = new[] { C(Rank.Nine, Suit.Hearts), C(Rank.King, Suit.Clubs) };

        var play = _ai.DecidePlay
        (
            Playing
            (
                Suit.Hearts,
                PlayerPosition.East,
                hand,
                (PlayerPosition.West, C(Rank.Nine, Suit.Spades)),
                (PlayerPosition.North, C(Rank.Queen, Suit.Hearts))
            ),
            PlayerPosition.East
        );

        Assert.Equal(C(Rank.King, Suit.Clubs), play);
    }



    [Fact]
    public void DecidePlay_when_only_trumps_are_left_throws_the_lowest_trump()
    {
        var hand = new[] { C(Rank.King, Suit.Hearts), C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Hearts) };

        var play = _ai.DecidePlay
        (
            Playing
            (
                Suit.Hearts,
                PlayerPosition.East,
                hand,
                (PlayerPosition.West, C(Rank.Nine, Suit.Spades)),
                (PlayerPosition.North, C(Rank.Jack, Suit.Hearts))
            ),
            PlayerPosition.East
        );

        Assert.Equal(C(Rank.Nine, Suit.Hearts), play);
    }



    [Fact]
    public void DecidePlay_with_no_current_trick_plays_as_the_leader()
    {
        var hand = new[] { C(Rank.Nine, Suit.Clubs), C(Rank.Ace, Suit.Clubs) };
        var state = Holding(PlayerPosition.East, hand, trump: Suit.Hearts);
        var leading = new GameState
        (
            GamePhase.TrickPlay,
            state.Dealer,
            state.Hands,
            state.TrumpSuit,
            state.TrumpMode,
            state.BiddingResult,
            state.CompletedTricks,
            currentTrick: null,
            state.NorthSouthScore,
            state.EastWestScore,
            state.Rules,
            state.NextToAct,
            state.TricksPlayedInRound,
            state.IsHawseyRound,
            state.HawseyBidder
        );

        Assert.Equal(C(Rank.Ace, Suit.Clubs), _ai.DecidePlay(leading, PlayerPosition.East));
    }



    [Fact]
    public void DecideHawseyExchange_discards_the_lowest_side_cards_and_takes_the_partners_best_trumps()
    {
        var bidder = new[]
        {
            C(Rank.King, Suit.Hearts), C(Rank.Ace, Suit.Clubs), C(Rank.Ten, Suit.Diamonds),
            C(Rank.Nine, Suit.Spades), C(Rank.Queen, Suit.Hearts), C(Rank.Queen, Suit.Clubs),
        };
        var partner = new[]
        {
            C(Rank.Ace, Suit.Spades), C(Rank.Nine, Suit.Hearts), C(Rank.Jack, Suit.Diamonds),
            C(Rank.King, Suit.Clubs), C(Rank.Ace, Suit.Hearts),
        };
        var state = Holding(PlayerPosition.South, bidder, trump: Suit.Hearts, other: PlayerPosition.North, otherHand: partner);

        _ai.DecideHawseyExchange(state, PlayerPosition.South, out var discard, out var fromPartner);

        // Lowest side cards first; the bidder's trumps (K, Q of hearts) are kept.
        Assert.Equal([C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Diamonds)], discard);

        // The partner's trumps, highest first: the left bower (jack of diamonds), then
        // the ace of hearts. The ace of spades is higher than the nine of hearts but
        // is not trump.
        Assert.Equal([C(Rank.Jack, Suit.Diamonds), C(Rank.Ace, Suit.Hearts)], fromPartner);
    }
}
