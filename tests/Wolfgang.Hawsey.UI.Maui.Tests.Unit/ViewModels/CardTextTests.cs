using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

public class CardTextTests
{
    [Theory]
    [InlineData(Rank.Nine, "9")]
    [InlineData(Rank.Ten, "10")]
    [InlineData(Rank.Jack, "J")]
    [InlineData(Rank.Queen, "Q")]
    [InlineData(Rank.King, "K")]
    [InlineData(Rank.Ace, "A")]
    [InlineData((Rank)99, "?")]
    public void Rank_when_given_a_rank_returns_its_text(Rank rank, string expected)
    {
        Assert.Equal(expected, CardText.Rank(rank));
    }



    [Theory]
    [InlineData(Suit.Hearts, "\u2665")]
    [InlineData(Suit.Diamonds, "\u2666")]
    [InlineData(Suit.Clubs, "\u2663")]
    [InlineData(Suit.Spades, "\u2660")]
    [InlineData((Suit)99, "?")]
    public void Suit_when_given_a_suit_returns_its_symbol(Suit suit, string expected)
    {
        Assert.Equal(expected, CardText.Suit(suit));
    }



    [Fact]
    public void DisplayOrder_when_sorting_puts_spades_hearts_clubs_diamonds_then_anything_else()
    {
        Suit[] suits = [(Suit)99, Suit.Diamonds, Suit.Clubs, Suit.Hearts, Suit.Spades];

        Assert.Equal
        (
            [Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds, (Suit)99],
            suits.OrderBy(CardText.DisplayOrder)
        );
    }
}
