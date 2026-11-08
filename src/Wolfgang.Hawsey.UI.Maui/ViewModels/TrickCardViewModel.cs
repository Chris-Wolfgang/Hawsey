
using Wolfgang.Hawsey.Engine.Cards;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

public class TrickCardViewModel
{
    public TrickCardViewModel(Card card)
    {
        Card = card;
        RankText = CardText.Rank(card.Rank);
        SuitSymbol = CardText.Suit(card.Suit);
        SuitColor = card.Suit.IsRed() ? TableColors.CardRed : TableColors.CardBlack;
    }



    public Card Card { get; }
    public string RankText { get; }
    public string SuitSymbol { get; }
    public Color SuitColor { get; }
}
