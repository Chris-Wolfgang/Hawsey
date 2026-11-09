using Wolfgang.Hawsey.Engine.Cards;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// How a card is written and ordered on screen, as in the Blazor UI: the rank as "9" to
/// "A", the suit as its symbol, and the hand's suit order. Every MAUI view model reads it
/// from here.
/// </summary>
public static class CardText
{
    /// <summary>The rank as shown on a card: "9", "10", "J", "Q", "K", "A", or "?" for an undefined value.</summary>
    public static string Rank(Rank rank) => rank switch
    {
        Engine.Cards.Rank.Nine => "9",
        Engine.Cards.Rank.Ten => "10",
        Engine.Cards.Rank.Jack => "J",
        Engine.Cards.Rank.Queen => "Q",
        Engine.Cards.Rank.King => "K",
        Engine.Cards.Rank.Ace => "A",
        _ => "?",
    };



    /// <summary>The suit's symbol (♥ ♦ ♣ ♠), or "?" for an undefined value.</summary>
    public static string Suit(Suit suit) => suit switch
    {
        Engine.Cards.Suit.Hearts => "\u2665",
        Engine.Cards.Suit.Diamonds => "\u2666",
        Engine.Cards.Suit.Clubs => "\u2663",
        Engine.Cards.Suit.Spades => "\u2660",
        _ => "?",
    };



    /// <summary>
    /// Where the suit sorts in the hand: spades, hearts, clubs, diamonds, so the colours
    /// alternate. An undefined value sorts last.
    /// </summary>
    public static int DisplayOrder(Suit suit) => suit switch
    {
        Engine.Cards.Suit.Spades => 0,
        Engine.Cards.Suit.Hearts => 1,
        Engine.Cards.Suit.Clubs => 2,
        Engine.Cards.Suit.Diamonds => 3,
        _ => 4,
    };
}
