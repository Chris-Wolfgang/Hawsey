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
        RankText = CardText.Rank(card.Rank);
        SuitSymbol = CardText.Suit(card.Suit);
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



    /// <summary>
    /// Whether <paramref name="other"/> shows the same: the same card, equally legal,
    /// picked and interactive (the text and colour follow from the card).
    /// </summary>
    public bool ShowsTheSameAs(CardViewModel other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Card == other.Card
            && IsLegal == other.IsLegal
            && IsSelected == other.IsSelected
            && IsInteractive == other.IsInteractive;
    }
}
