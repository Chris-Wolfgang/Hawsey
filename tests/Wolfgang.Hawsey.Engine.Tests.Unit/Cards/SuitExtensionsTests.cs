using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Cards;

public class SuitExtensionsTests
{
    [Theory]
    [InlineData(Suit.Hearts, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    [InlineData(Suit.Clubs, Suit.Spades)]
    [InlineData(Suit.Spades, Suit.Clubs)]
    public void GetSameColorSuit_returns_partner_suit(Suit input, Suit expected)
    {
        Assert.Equal(expected, input.GetSameColorSuit());
    }



    [Theory]
    [InlineData(Suit.Hearts, true)]
    [InlineData(Suit.Diamonds, true)]
    [InlineData(Suit.Clubs, false)]
    [InlineData(Suit.Spades, false)]
    public void IsRed_returns_correct_value(Suit suit, bool expected)
    {
        Assert.Equal(expected, suit.IsRed());
    }



    [Theory]
    [InlineData(Suit.Hearts, false)]
    [InlineData(Suit.Diamonds, false)]
    [InlineData(Suit.Clubs, true)]
    [InlineData(Suit.Spades, true)]
    public void IsBlack_returns_correct_value(Suit suit, bool expected)
    {
        Assert.Equal(expected, suit.IsBlack());
    }



    [Fact]
    public void GetSameColorSuit_when_invalid_suit_throws()
    {
        var invalid = (Suit)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetSameColorSuit());
    }
}
