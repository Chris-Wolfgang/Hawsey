using Wolfgang.Hawsey.Engine.Cards;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// A card in the human's hand. As in the Blazor UI, a card the human may play now has a
/// green outline, a card that does nothing when tapped now is dimmed, and a card picked as
/// a Hawsey-exchange discard is raised with a red outline.
/// </summary>
public class CardViewModel
{
    /// <param name="card">The card.</param>
    /// <param name="isLegal">Whether the human may play it now.</param>
    /// <param name="isSelected">Whether it is picked as a Hawsey-exchange discard.</param>
    /// <param name="isInteractive">
    /// Whether tapping it does anything now. By default, when it is legal or picked.
    /// </param>
    public CardViewModel(Card card, bool isLegal, bool isSelected = false, bool? isInteractive = null)
    {
        Card = card;
        IsLegal = isLegal;
        IsSelected = isSelected;
        IsInteractive = isInteractive ?? (isLegal || isSelected);
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
        SuitColor = card.Suit.IsRed() ? TableColors.CardRed : TableColors.CardBlack;
    }



    public Card Card { get; }

    /// <summary>Whether the human may play the card now (it gets the green outline).</summary>
    public bool IsLegal { get; }

    /// <summary>Whether the card is picked as one of the two Hawsey-exchange discards.</summary>
    public bool IsSelected { get; }

    /// <summary>Whether tapping the card does anything now; the others are dimmed.</summary>
    public bool IsInteractive { get; }
    public string RankText { get; }
    public string SuitSymbol { get; }
    public Color SuitColor { get; }
}
