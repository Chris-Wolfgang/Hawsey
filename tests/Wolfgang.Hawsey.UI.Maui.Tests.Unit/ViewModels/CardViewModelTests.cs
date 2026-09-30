using Microsoft.Maui.Graphics;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

public class CardViewModelTests
{
    [Theory]
    [InlineData(Rank.Nine, "9")]
    [InlineData(Rank.Ten, "10")]
    [InlineData(Rank.Jack, "J")]
    [InlineData(Rank.Queen, "Q")]
    [InlineData(Rank.King, "K")]
    [InlineData(Rank.Ace, "A")]
    [InlineData((Rank)99, "?")]
    public void Both_card_view_models_show_the_rank(Rank rank, string expected)
    {
        var card = new Card(rank, Suit.Spades);

        Assert.Equal(expected, new CardViewModel(card, isLegal: false).RankText);
        Assert.Equal(expected, new TrickCardViewModel(card).RankText);
    }



    [Theory]
    [InlineData(Suit.Hearts, "\u2665", true)]
    [InlineData(Suit.Diamonds, "\u2666", true)]
    [InlineData(Suit.Clubs, "\u2663", false)]
    [InlineData(Suit.Spades, "\u2660", false)]
    [InlineData((Suit)99, "?", false)]
    public void Both_card_view_models_show_the_suit_symbol_and_colour(Suit suit, string symbol, bool red)
    {
        var card = new Card(Rank.Ace, suit);
        var expectedColor = red ? TableColors.CardRed : TableColors.CardBlack;

        var hand = new CardViewModel(card, isLegal: true);
        var trick = new TrickCardViewModel(card);

        Assert.Equal(symbol, hand.SuitSymbol);
        Assert.Equal(symbol, trick.SuitSymbol);
        Assert.Equal(expectedColor, hand.SuitColor);
        Assert.Equal(expectedColor, trick.SuitColor);
        Assert.Equal(card, hand.Card);
        Assert.True(hand.IsLegal);
    }
}
