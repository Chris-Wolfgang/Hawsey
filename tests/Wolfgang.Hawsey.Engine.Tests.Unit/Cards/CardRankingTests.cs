using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Cards;

public class CardRankingTests
{
    [Fact]
    public void GetEffectiveSuit_when_no_trump_returns_printed_suit()
    {
        var card = new Card(Rank.Jack, Suit.Spades);

        Assert.Equal(Suit.Spades, CardRanking.GetEffectiveSuit(card, trumpSuit: null));
    }



    [Fact]
    public void GetEffectiveSuit_when_left_bower_returns_trump_suit()
    {
        // Clubs is trump, Jack of Spades is the left bower
        var leftBower = new Card(Rank.Jack, Suit.Spades);

        Assert.Equal(Suit.Clubs, CardRanking.GetEffectiveSuit(leftBower, Suit.Clubs));
    }



    [Fact]
    public void GetEffectiveSuit_when_right_bower_returns_trump_suit()
    {
        var rightBower = new Card(Rank.Jack, Suit.Clubs);

        Assert.Equal(Suit.Clubs, CardRanking.GetEffectiveSuit(rightBower, Suit.Clubs));
    }



    [Fact]
    public void GetEffectiveSuit_when_non_jack_in_same_color_returns_printed_suit()
    {
        // Clubs is trump, Ace of Spades is NOT the left bower
        var card = new Card(Rank.Ace, Suit.Spades);

        Assert.Equal(Suit.Spades, CardRanking.GetEffectiveSuit(card, Suit.Clubs));
    }



    [Fact]
    public void GetEffectiveRank_when_right_bower_returns_highest()
    {
        var rightBower = new Card(Rank.Jack, Suit.Hearts);

        Assert.Equal(CardRanking.RightBowerRank, CardRanking.GetEffectiveRank(rightBower, Suit.Hearts));
    }



    [Fact]
    public void GetEffectiveRank_when_left_bower_returns_second_highest()
    {
        // Hearts is trump, Jack of Diamonds is left bower
        var leftBower = new Card(Rank.Jack, Suit.Diamonds);

        Assert.Equal(CardRanking.LeftBowerRank, CardRanking.GetEffectiveRank(leftBower, Suit.Hearts));
    }



    [Fact]
    public void GetEffectiveRank_when_ace_of_trump_returns_ace_value()
    {
        var aceOfTrump = new Card(Rank.Ace, Suit.Hearts);

        Assert.Equal((int)Rank.Ace, CardRanking.GetEffectiveRank(aceOfTrump, Suit.Hearts));
    }



    [Fact]
    public void GetEffectiveRank_when_no_trump_returns_printed_rank()
    {
        var card = new Card(Rank.Jack, Suit.Spades);

        Assert.Equal((int)Rank.Jack, CardRanking.GetEffectiveRank(card, trumpSuit: null));
    }



    [Fact]
    public void GetEffectiveRank_right_bower_beats_left_bower()
    {
        var rightBower = new Card(Rank.Jack, Suit.Clubs);
        var leftBower = new Card(Rank.Jack, Suit.Spades);

        Assert.True
        (
            CardRanking.GetEffectiveRank(rightBower, Suit.Clubs)
            > CardRanking.GetEffectiveRank(leftBower, Suit.Clubs)
        );
    }



    [Fact]
    public void GetEffectiveRank_left_bower_beats_ace_of_trump()
    {
        var leftBower = new Card(Rank.Jack, Suit.Spades);
        var aceOfTrump = new Card(Rank.Ace, Suit.Clubs);

        Assert.True
        (
            CardRanking.GetEffectiveRank(leftBower, Suit.Clubs)
            > CardRanking.GetEffectiveRank(aceOfTrump, Suit.Clubs)
        );
    }



    [Fact]
    public void IsRightBower_when_jack_of_trump_returns_true()
    {
        var card = new Card(Rank.Jack, Suit.Diamonds);

        Assert.True(CardRanking.IsRightBower(card, Suit.Diamonds));
    }



    [Fact]
    public void IsRightBower_when_jack_of_other_suit_returns_false()
    {
        var card = new Card(Rank.Jack, Suit.Diamonds);

        Assert.False(CardRanking.IsRightBower(card, Suit.Clubs));
    }



    [Fact]
    public void IsRightBower_when_non_jack_of_trump_returns_false()
    {
        var card = new Card(Rank.Ace, Suit.Diamonds);

        Assert.False(CardRanking.IsRightBower(card, Suit.Diamonds));
    }



    [Fact]
    public void IsLeftBower_when_jack_of_same_color_returns_true()
    {
        var card = new Card(Rank.Jack, Suit.Diamonds);

        Assert.True(CardRanking.IsLeftBower(card, Suit.Hearts));
    }



    [Fact]
    public void IsLeftBower_when_jack_of_different_color_returns_false()
    {
        var card = new Card(Rank.Jack, Suit.Diamonds);

        Assert.False(CardRanking.IsLeftBower(card, Suit.Clubs));
    }



    [Fact]
    public void IsTrump_when_card_is_trump_suit_returns_true()
    {
        var card = new Card(Rank.Nine, Suit.Hearts);

        Assert.True(CardRanking.IsTrump(card, Suit.Hearts));
    }



    [Fact]
    public void IsTrump_when_left_bower_returns_true()
    {
        var leftBower = new Card(Rank.Jack, Suit.Diamonds);

        Assert.True(CardRanking.IsTrump(leftBower, Suit.Hearts));
    }



    [Fact]
    public void IsTrump_when_no_trump_returns_false()
    {
        var card = new Card(Rank.Ace, Suit.Hearts);

        Assert.False(CardRanking.IsTrump(card, trumpSuit: null));
    }



    [Fact]
    public void IsTrump_when_off_suit_returns_false()
    {
        var card = new Card(Rank.Ace, Suit.Spades);

        Assert.False(CardRanking.IsTrump(card, Suit.Hearts));
    }



    [Theory]
    [InlineData(Suit.Hearts, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    [InlineData(Suit.Clubs, Suit.Spades)]
    [InlineData(Suit.Spades, Suit.Clubs)]
    public void GetEffectiveSuit_left_bower_for_all_trump_suits(Suit trump, Suit leftBowerPrintedSuit)
    {
        var leftBower = new Card(Rank.Jack, leftBowerPrintedSuit);

        Assert.Equal(trump, CardRanking.GetEffectiveSuit(leftBower, trump));
    }
}
