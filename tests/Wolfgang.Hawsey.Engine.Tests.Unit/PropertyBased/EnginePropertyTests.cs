using FsCheck.Xunit;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Scoring;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// FsCheck property-based tests: each property runs against many generated
/// inputs (seeds, suits, hand sizes, rule switches) instead of hand-picked cases.
/// </summary>
public class EnginePropertyTests
{
    private static readonly Suit[] Suits = [Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades];

    private static readonly PlayerPosition[] Positions =
        [PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West];



    private static Suit SuitFrom(int value) => Suits[Math.Abs(value % Suits.Length)];



    private static IReadOnlyList<Card> ShuffledDeck(int seed) =>
        Deck.Shuffle(Deck.CreatePinochleDeck(), new Random(seed));



    [Property]
    public void Shuffle_for_any_seed_returns_a_permutation_of_the_deck(int seed)
    {
        var deck = Deck.CreatePinochleDeck();

        var shuffled = Deck.Shuffle(deck, new Random(seed));

        Assert.Equal
        (
            deck.OrderBy(c => c.Suit).ThenBy(c => c.Rank),
            shuffled.OrderBy(c => c.Suit).ThenBy(c => c.Rank)
        );
    }



    [Property]
    public void Compare_for_any_two_cards_is_antisymmetric(int seed, int trump, int led, bool noTrump)
    {
        var cards = ShuffledDeck(seed);
        var comparer = new CardComparer(noTrump ? null : SuitFrom(trump), SuitFrom(led));

        var forward = Math.Sign(comparer.Compare(cards[0], cards[1]));
        var backward = Math.Sign(comparer.Compare(cards[1], cards[0]));

        Assert.Equal(-forward, backward);
    }



    [Property]
    public void Compare_for_any_card_against_itself_is_zero(int seed, int trump, int led)
    {
        var card = ShuffledDeck(seed)[0];
        var comparer = new CardComparer(SuitFrom(trump), SuitFrom(led));

        Assert.Equal(0, comparer.Compare(card, card));
    }



    [Property]
    public void GetLegalPlays_for_any_hand_is_a_nonempty_subset_of_the_hand
    (
        int seed,
        int handSize,
        int trump,
        int led,
        bool leading,
        bool mustBeat
    )
    {
        var deck = ShuffledDeck(seed);
        var hand = deck.Take(1 + Math.Abs(handSize % 12)).ToList();
        var trumpSuit = SuitFrom(trump);
        Suit? ledSuit = leading ? null : SuitFrom(led);
        var winning = ledSuit is null
            ? (Card?)null
            : deck.Skip(12).FirstOrDefault(c => CardRanking.GetEffectiveSuit(c, trumpSuit) == ledSuit);
        var rules = new HouseRules { MustBeat = mustBeat };

        var legal = FollowSuitValidator.GetLegalPlays(hand, ledSuit, trumpSuit, rules, winning);

        Assert.NotEmpty(legal);
        Assert.All(legal, card => Assert.Contains(card, hand));
    }



    [Property]
    public void GetLegalPlays_when_the_hand_can_follow_suit_only_allows_the_led_suit
    (
        int seed,
        int handSize,
        int trump,
        int led,
        bool mustBeat
    )
    {
        var deck = ShuffledDeck(seed);
        var hand = deck.Take(1 + Math.Abs(handSize % 12)).ToList();
        var trumpSuit = SuitFrom(trump);
        var ledSuit = SuitFrom(led);
        var rules = new HouseRules { MustBeat = mustBeat };
        var canFollow = hand.Exists(c => CardRanking.GetEffectiveSuit(c, trumpSuit) == ledSuit);

        var legal = FollowSuitValidator.GetLegalPlays(hand, ledSuit, trumpSuit, rules, currentWinningCard: null);

        if (canFollow)
        {
            Assert.All(legal, card => Assert.Equal(ledSuit, CardRanking.GetEffectiveSuit(card, trumpSuit)));
        }
        else
        {
            Assert.Equal(hand.Count, legal.Count);
        }
    }



    [Property]
    public void Left_bower_for_any_trump_is_trump_and_ranks_just_below_the_right_bower(int trump)
    {
        var trumpSuit = SuitFrom(trump);
        var leftBower = new Card(Rank.Jack, trumpSuit.GetSameColorSuit());

        Assert.True(CardRanking.IsTrump(leftBower, trumpSuit));
        Assert.Equal(CardRanking.RightBowerRank - 1, CardRanking.GetEffectiveRank(leftBower, trumpSuit));
    }



    [Property]
    public void Seat_rotation_for_any_seat_returns_home_after_four_steps(int seat)
    {
        var start = Positions[Math.Abs(seat % Positions.Length)];

        var end = start.NextClockwise().NextClockwise().NextClockwise().NextClockwise();

        Assert.Equal(start, end);
    }



    [Property]
    public void Partner_for_any_seat_is_on_the_same_team_and_is_its_own_inverse(int seat)
    {
        var position = Positions[Math.Abs(seat % Positions.Length)];

        Assert.Equal(position.GetTeam(), position.Partner().GetTeam());
        Assert.Equal(position, position.Partner().Partner());
        Assert.NotEqual(position, position.Partner());
    }



    [Property]
    public void RoundScore_for_any_number_bid_scores_tricks_when_made_and_minus_bid_when_set(int bid, int tricks)
    {
        var bidAmount = 6 + Math.Abs(bid % 7);
        var biddingTricks = Math.Abs(tricks % 13);

        var score = new RoundScore(Team.NorthSouth, bidAmount, biddingTricks, 12 - biddingTricks, isHawsey: false);

        Assert.Equal(biddingTricks >= bidAmount ? biddingTricks : -bidAmount, score.BiddingTeamDelta);
        Assert.Equal(12 - biddingTricks, score.DefendingTeamDelta);
    }
}
