using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

public class CardViewModel
{
    public CardViewModel(Card card, bool isLegal)
    {
        Card = card;
        IsLegal = isLegal;
        RankText = card.Rank switch
        {
            Rank.Nine => "9",
            Rank.Ten => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => "?"
        };
        SuitSymbol = card.Suit switch
        {
            Suit.Hearts => "\u2665",
            Suit.Diamonds => "\u2666",
            Suit.Clubs => "\u2663",
            Suit.Spades => "\u2660",
            _ => "?"
        };
        SuitColor = card.Suit.IsRed() ? Colors.Red : Colors.Black;
    }



    public Card Card { get; }
    public bool IsLegal { get; }
    public string RankText { get; }
    public string SuitSymbol { get; }
    public Color SuitColor { get; }
}
