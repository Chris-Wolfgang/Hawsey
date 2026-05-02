using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Cards;

public class CardComparerTests
{
    [Fact]
    public void Compare_when_trump_beats_non_trump()
    {
        var comparer = new CardComparer(Suit.Hearts, Suit.Clubs);

        var trump = new Card(Rank.Nine, Suit.Hearts);
        var nonTrump = new Card(Rank.Ace, Suit.Clubs);

        Assert.True(comparer.Compare(trump, nonTrump) > 0);
    }



    [Fact]
    public void Compare_when_led_suit_beats_off_suit()
    {
        var comparer = new CardComparer(Suit.Hearts, Suit.Clubs);

        var led = new Card(Rank.Nine, Suit.Clubs);
        var offSuit = new Card(Rank.Ace, Suit.Diamonds);

        Assert.True(comparer.Compare(led, offSuit) > 0);
    }



    [Fact]
    public void Compare_when_higher_rank_in_same_suit_wins()
    {
        var comparer = new CardComparer(Suit.Hearts, Suit.Clubs);

        var higher = new Card(Rank.Ace, Suit.Clubs);
        var lower = new Card(Rank.King, Suit.Clubs);

        Assert.True(comparer.Compare(higher, lower) > 0);
    }



    [Fact]
    public void Compare_when_right_bower_beats_left_bower()
    {
        var comparer = new CardComparer(Suit.Clubs, Suit.Clubs);

        var rightBower = new Card(Rank.Jack, Suit.Clubs);
        var leftBower = new Card(Rank.Jack, Suit.Spades);

        Assert.True(comparer.Compare(rightBower, leftBower) > 0);
    }



    [Fact]
    public void Compare_when_left_bower_beats_ace_of_trump()
    {
        var comparer = new CardComparer(Suit.Clubs, Suit.Clubs);

        var leftBower = new Card(Rank.Jack, Suit.Spades);
        var aceOfTrump = new Card(Rank.Ace, Suit.Clubs);

        Assert.True(comparer.Compare(leftBower, aceOfTrump) > 0);
    }



    [Fact]
    public void Compare_when_duplicate_cards_returns_zero()
    {
        var comparer = new CardComparer(Suit.Hearts, Suit.Clubs);

        var card1 = new Card(Rank.Ace, Suit.Clubs);
        var card2 = new Card(Rank.Ace, Suit.Clubs);

        Assert.Equal(0, comparer.Compare(card1, card2));
    }



    [Fact]
    public void Compare_when_ace_high_no_trump_higher_rank_wins()
    {
        var comparer = new CardComparer(trumpSuit: null, ledSuit: Suit.Clubs);

        var ace = new Card(Rank.Ace, Suit.Clubs);
        var king = new Card(Rank.King, Suit.Clubs);

        Assert.True(comparer.Compare(ace, king) > 0);
    }



    [Fact]
    public void Compare_when_ace_high_off_suit_loses_to_led()
    {
        var comparer = new CardComparer(trumpSuit: null, ledSuit: Suit.Clubs);

        var offSuit = new Card(Rank.Ace, Suit.Hearts);
        var led = new Card(Rank.Nine, Suit.Clubs);

        Assert.True(comparer.Compare(offSuit, led) < 0);
    }



    [Fact]
    public void Compare_when_two_off_suit_cards_returns_zero_category()
    {
        var comparer = new CardComparer(Suit.Hearts, Suit.Clubs);

        var offSuit1 = new Card(Rank.Ace, Suit.Diamonds);
        var offSuit2 = new Card(Rank.King, Suit.Spades);

        // Both are off-suit (category 0), so rank comparison applies
        // But they are different suits and both off-suit, rank determines order
        var result = comparer.Compare(offSuit1, offSuit2);
        Assert.True(result > 0);
    }
}
