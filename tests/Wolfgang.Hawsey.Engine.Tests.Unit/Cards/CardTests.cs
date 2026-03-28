using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Cards;

public class CardTests
{
    [Fact]
    public void Card_when_same_rank_and_suit_are_equal()
    {
        var card1 = new Card(Rank.Ace, Suit.Spades);
        var card2 = new Card(Rank.Ace, Suit.Spades);

        Assert.Equal(card1, card2);
    }



    [Fact]
    public void Card_when_different_rank_are_not_equal()
    {
        var card1 = new Card(Rank.Ace, Suit.Spades);
        var card2 = new Card(Rank.King, Suit.Spades);

        Assert.NotEqual(card1, card2);
    }



    [Fact]
    public void Card_when_different_suit_are_not_equal()
    {
        var card1 = new Card(Rank.Ace, Suit.Spades);
        var card2 = new Card(Rank.Ace, Suit.Hearts);

        Assert.NotEqual(card1, card2);
    }



    [Fact]
    public void ToString_returns_rank_of_suit()
    {
        var card = new Card(Rank.Queen, Suit.Diamonds);

        Assert.Equal("Queen of Diamonds", card.ToString());
    }
}
