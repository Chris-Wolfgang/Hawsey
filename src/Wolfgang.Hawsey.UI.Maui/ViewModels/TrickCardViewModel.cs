using Wolfgang.Hawsey.Engine.Cards;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>A card on the table, in front of the seat that played it.</summary>
public sealed class TrickCardViewModel
{
    public TrickCardViewModel(Card card)
    {
        Card = card;
        RankText = CardText.Rank(card.Rank);
        SuitSymbol = CardText.Suit(card.Suit);
        SuitColor = card.Suit.IsRed() ? TableColors.CardRed : TableColors.CardBlack;
    }



    /// <summary>The card.</summary>
    public Card Card { get; }
    /// <summary>The rank as shown on the card (<see cref="CardText.Rank"/>).</summary>
    public string RankText { get; }
    /// <summary>The suit's symbol (<see cref="CardText.Suit"/>).</summary>
    public string SuitSymbol { get; }
    /// <summary>The ink colour: red for hearts and diamonds, black otherwise.</summary>
    public Color SuitColor { get; }
}
