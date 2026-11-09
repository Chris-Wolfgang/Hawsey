using Microsoft.Maui.Graphics;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

public class CardViewModelTests
{
    [Theory]
    [InlineData(Suit.Hearts, true)]
    [InlineData(Suit.Diamonds, true)]
    [InlineData(Suit.Clubs, false)]
    [InlineData(Suit.Spades, false)]
    public void Constructor_when_given_a_card_shows_its_text_and_colour(Suit suit, bool red)
    {
        var card = new Card(Rank.Queen, suit);
        var expectedColor = red ? TableColors.CardRed : TableColors.CardBlack;

        var hand = new CardViewModel(card, isLegal: true);
        var trick = new TrickCardViewModel(card);

        Assert.Equal("Q", hand.RankText);
        Assert.Equal("Q", trick.RankText);
        Assert.Equal(CardText.Suit(suit), hand.SuitSymbol);
        Assert.Equal(CardText.Suit(suit), trick.SuitSymbol);
        Assert.Equal(expectedColor, hand.SuitColor);
        Assert.Equal(expectedColor, trick.SuitColor);
        Assert.Equal(card, hand.Card);
        Assert.Equal(card, trick.Card);
        Assert.True(hand.IsLegal);
    }



    [Fact]
    public void ShowsTheSameAs_when_card_and_flags_match_is_true()
    {
        var card = new Card(Rank.Ace, Suit.Spades);

        Assert.True(new CardViewModel(card, isLegal: true).ShowsTheSameAs(new CardViewModel(card, isLegal: true)));
    }



    [Fact]
    public void ShowsTheSameAs_when_the_card_or_any_flag_differs_is_false()
    {
        var card = new Card(Rank.Ace, Suit.Spades);
        var shown = new CardViewModel(card, isLegal: false, isSelected: false, isInteractive: false);

        Assert.False(shown.ShowsTheSameAs(new CardViewModel(new Card(Rank.King, Suit.Spades), isLegal: false, isSelected: false, isInteractive: false)));
        Assert.False(shown.ShowsTheSameAs(new CardViewModel(card, isLegal: true, isSelected: false, isInteractive: false)));
        Assert.False(shown.ShowsTheSameAs(new CardViewModel(card, isLegal: false, isSelected: true, isInteractive: false)));
        Assert.False(shown.ShowsTheSameAs(new CardViewModel(card, isLegal: false, isSelected: false, isInteractive: true)));
    }



    [Fact]
    public void ShowsTheSameAs_when_other_is_null_throws()
    {
        var shown = new CardViewModel(new Card(Rank.Ace, Suit.Spades), isLegal: false);

        Assert.Equal("other", Assert.Throws<ArgumentNullException>(() => shown.ShowsTheSameAs(null!)).ParamName);
    }
}
