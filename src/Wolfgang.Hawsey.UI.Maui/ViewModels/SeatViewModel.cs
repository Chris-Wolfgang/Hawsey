using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// One seat at the table: who sits there, how many cards they hold (shown face down for
/// the AI seats), the tricks they have won this round, their bid while the bidding is
/// on, whether they deal, whether it is their turn, and the card they have on the table.
/// </summary>
// `partial` for the Windows TFM's CsWinRT generator, as on GameViewModel.
// ReSharper disable once PartialTypeWithSinglePart
public partial class SeatViewModel : INotifyPropertyChanged
{
    private IReadOnlyList<int> _cardBacks = Array.Empty<int>();
    private int _cardCount;
    private int _tricksWon;
    private string _bidText = "";
    private string _bidKind = "";
    private bool _isDealer;
    private bool _isActive;
    private TrickCardViewModel? _tableCard;



    public SeatViewModel(PlayerPosition position, string name)
    {
        Position = position;
        Name = name;
    }



    public PlayerPosition Position { get; }



    /// <summary>
    /// Whether the seat is on the human's team (North/South). The Blazor UI colours the
    /// human's team blue and the opponents red.
    /// </summary>
    public bool IsUs => Position is PlayerPosition.North or PlayerPosition.South;



    /// <summary>The seat's label, for example "Partner".</summary>
    public string Name { get; }



    public int CardCount
    {
        get => _cardCount;
        private set => SetProperty(ref _cardCount, value);
    }



    /// <summary>
    /// One item per card in the hand, for the face-down fan. A new list replaces the old
    /// one when the count changes, so a bound layout redraws.
    /// </summary>
    public IReadOnlyList<int> CardBacks
    {
        get => _cardBacks;
        private set => SetProperty(ref _cardBacks, value);
    }



    /// <summary>Tricks this seat has won this round.</summary>
    public int TricksWon
    {
        get => _tricksWon;
        private set => SetProperty(ref _tricksWon, value);
    }



    /// <summary>
    /// The seat's bid this round ("Pass", "Bid 7", "Hawsey") while the bidding and trump
    /// are being decided; empty once play starts, when the tricks panel shows the result.
    /// </summary>
    public string BidText
    {
        get => _bidText;
        private set => SetProperty(ref _bidText, value);
    }



    /// <summary>
    /// The kind of bid in <see cref="BidText"/>, for its badge colour: "pass", "number",
    /// "hawsey", or empty when there is no badge.
    /// </summary>
    public string BidKind
    {
        get => _bidKind;
        private set => SetProperty(ref _bidKind, value);
    }



    public bool IsDealer
    {
        get => _isDealer;
        private set => SetProperty(ref _isDealer, value);
    }



    /// <summary>Whether it is this seat's turn to bid (the Blazor UI highlights only the bidding).</summary>
    public bool IsActive
    {
        get => _isActive;
        private set => SetProperty(ref _isActive, value);
    }



    /// <summary>The card this seat has on the table, or <see langword="null"/>.</summary>
    public TrickCardViewModel? TableCard
    {
        get => _tableCard;
        private set
        {
            if (SetProperty(ref _tableCard, value))
            {
                OnPropertyChanged(nameof(HasTableCard));
            }
        }
    }



    public bool HasTableCard => _tableCard != null;



    public event PropertyChangedEventHandler? PropertyChanged;



    /// <summary>
    /// Refreshes the seat from the game state, this round's bids and the cards on the
    /// table (<see cref="GameSession.TableCards"/>, which clears a won trick before the
    /// human leads).
    /// </summary>
    public void Update(GameState state, IReadOnlyList<PlacedBid> bids, IReadOnlyList<PlayedCard> tableCards)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(bids);
        ArgumentNullException.ThrowIfNull(tableCards);

        var count = state.Hands[Position].Count;

        if (count != CardCount || CardBacks.Count != count)
        {
            CardCount = count;
            CardBacks = Enumerable.Range(0, count).ToArray();
        }

        TricksWon = state.CompletedTricks.Count(t => t.Winner == Position);
        (BidText, BidKind) = GetBid(state, bids);
        IsDealer = state.Dealer == Position;
        IsActive = state.Phase == GamePhase.Bidding && state.NextToAct == Position;
        TableCard = GetTableCard(tableCards);
    }



    private (string Text, string Kind) GetBid(GameState state, IReadOnlyList<PlacedBid> bids)
    {
        if (state.Phase is not (GamePhase.Bidding or GamePhase.TrumpSelection or GamePhase.HawseyExchange))
        {
            return ("", "");
        }

        for (var i = bids.Count - 1; i >= 0; i--)
        {
            if (bids[i].Player == Position)
            {
                return bids[i].Action switch
                {
                    BidAction.NumberBid n => ($"Bid {n.Amount}", "number"),
                    BidAction.HawseyBid => ("Hawsey", "hawsey"),
                    _ => ("Pass", "pass"),
                };
            }
        }

        return ("", "");
    }



    private TrickCardViewModel? GetTableCard(IReadOnlyList<PlayedCard> plays)
    {

        for (var i = 0; i < plays.Count; i++)
        {
            if (plays[i].Player == Position)
            {
                // Keep the same view model while the same card stays on the table, so the
                // binding does not redraw it on every state change.
                return _tableCard?.Card == plays[i].Card ? _tableCard : new TrickCardViewModel(plays[i].Card);
            }
        }

        return null;
    }



    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }



    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
