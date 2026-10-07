#pragma warning disable AsyncFixer01
#pragma warning disable AsyncFixer03
#pragma warning disable VSTHRD101
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.UI.Maui.Threading;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// The game table, as the Blazor UI shows it: the four seats, the trick on the table, the
/// tricks and score panels, the human's hand, the bid / trump / Hawsey-exchange panels,
/// the round summary and the game log. The game itself is the engine's
/// <see cref="GameSession"/>; this class only shows it and passes on the human's moves.
/// </summary>
// `partial` is required on the Windows TFM: GameViewModel implements WinRT-projected
// interfaces, and the CsWinRT AOT source generator emits the other part of this
// class there (without it, CsWinRT1028). InspectCode analyzes a slice where that
// generator does not run, sees a single part, and reports the modifier as redundant.
// ReSharper disable once PartialTypeWithSinglePart
public partial class GameViewModel : INotifyPropertyChanged
{
    private const string NoCard = "—";

    private readonly GameSession _gameService;
    private readonly IUiDispatcher _dispatcher;
    private string _statusMessage = "Welcome to Hawsey. Click NEW GAME to start.";
    private bool _isBiddingVisible;
    private bool _isTrumpPickerVisible;
    private bool _isLogVisible;
    private RoundSummaryViewModel? _roundSummary;
    private int _northSouthScore;
    private int _eastWestScore;
    private int _usTricks;
    private int _themTricks;
    private string _trumpSymbol = NoCard;
    private bool _isTrumpRed;
    private bool _hasTrumpSuit;
    private string _trumpSetterTeam = "";
    private string _bidAmountText = "";
    private string _ledByText = "";
    private string _winningCardText = NoCard;
    private string _bidPrompt = "";
    private bool _canPass = true;
    private bool _isHawseyExchangeVisible;
    private string _exchangeSelectionText = "";
    private LayoutChoice _layoutChoice = LayoutChoice.Auto;
    private double _pageWidth;
    private double _pageHeight;
    private bool _isCompactLayout;

    // The hand in the order the human sees it: sorted by suit and rank when dealt, then
    // kept as the human rearranges it. Null until the first deal of a game.
    private List<Card>? _handOrder;

    // Positions in the shown hand, not cards: a pinochle hand can hold both copies of a
    // card, and the human may discard both.
    private readonly List<int> _discardIndexes = new();
    private readonly Command _confirmHawseyExchangeCommand;



    public GameViewModel(GameSession gameService, IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(gameService);
        ArgumentNullException.ThrowIfNull(dispatcher);

        _gameService = gameService;
        _dispatcher = dispatcher;
        _gameService.StateChanged += OnStateChanged;

        // Command takes Action/Action<T>; an `async () => await ...` lambda would be
        // async-void (MA0147). Each command starts its move through Run, which reports
        // a failure instead of letting the discarded task lose it.
        NewGameCommand = new Command(() => Run(StartNewGameAsync));
        NextRoundCommand = new Command(() => Run(StartNextRoundAsync));
        PlaceBidCommand = new Command<string>(s => Run(() => PlaceBidAsync(s)));
        SelectTrumpCommand = new Command<string>(s => Run(() => SelectTrumpAsync(s)));
        PlayCardCommand = new Command<CardViewModel>(c => Run(() => PlayCardAsync(c)));
        ToggleLogCommand = new Command(() => IsLogVisible = !IsLogVisible);
        ToggleLayoutCommand = new Command(ToggleLayout);
        _confirmHawseyExchangeCommand = new Command
        (
            () => Run(ConfirmHawseyExchangeAsync),
            () => _discardIndexes.Count == HawseyDiscardCount
        );
    }



    /// <summary>The human discards two cards in a Hawsey exchange.</summary>
    public const int HawseyDiscardCount = 2;



    /// <summary>The human's hand, in the order the human has arranged it.</summary>
    public ObservableCollection<CardViewModel> HumanCards { get; } = new();



    /// <summary>The partner's seat, across the table.</summary>
    public SeatViewModel North { get; } = new(PlayerPosition.North, "PARTNER");



    /// <summary>The opponent to the human's right.</summary>
    public SeatViewModel East { get; } = new(PlayerPosition.East, "OPPONENT RIGHT");



    /// <summary>The opponent to the human's left.</summary>
    public SeatViewModel West { get; } = new(PlayerPosition.West, "OPPONENT LEFT");



    /// <summary>The human's own seat: tricks, bid and dealer (the hand is <see cref="HumanCards"/>).</summary>
    public SeatViewModel South { get; } = new(PlayerPosition.South, "YOU");



    /// <summary>This round so far: each bid, then each trick's winner.</summary>
    public ObservableCollection<string> GameLog { get; } = new();



    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }



    public bool IsBiddingVisible
    {
        get => _isBiddingVisible;
        set => SetProperty(ref _isBiddingVisible, value);
    }



    public bool IsTrumpPickerVisible
    {
        get => _isTrumpPickerVisible;
        set => SetProperty(ref _isTrumpPickerVisible, value);
    }



    /// <summary>Whether the game log panel is open.</summary>
    public bool IsLogVisible
    {
        get => _isLogVisible;
        set => SetProperty(ref _isLogVisible, value);
    }



    /// <summary>
    /// The summary of the round just scored, shown until the human starts the next round
    /// (or, at game over, a new game); <see langword="null"/> otherwise.
    /// </summary>
    public RoundSummaryViewModel? RoundSummary
    {
        get => _roundSummary;
        private set
        {
            if (SetProperty(ref _roundSummary, value))
            {
                OnPropertyChanged(nameof(IsRoundSummaryVisible));
            }
        }
    }



    public bool IsRoundSummaryVisible => _roundSummary != null;



    /// <summary>
    /// Whether the page shows the compact phone layout rather than the table: picked
    /// from the page's size (<see cref="LayoutPicker"/>) until the human flips it with
    /// <see cref="ToggleLayoutCommand"/>.
    /// </summary>
    public bool IsCompactLayout
    {
        get => _isCompactLayout;
        private set
        {
            if (SetProperty(ref _isCompactLayout, value))
            {
                OnPropertyChanged(nameof(IsTableLayout));
            }
        }
    }



    /// <summary>Whether the page shows the table: the opposite of <see cref="IsCompactLayout"/>.</summary>
    public bool IsTableLayout => !_isCompactLayout;



    /// <summary>The human's team's score (North/South: "US").</summary>
    public int NorthSouthScore
    {
        get => _northSouthScore;
        set => SetProperty(ref _northSouthScore, value);
    }



    /// <summary>The opponents' score (East/West: "THEM").</summary>
    public int EastWestScore
    {
        get => _eastWestScore;
        set => SetProperty(ref _eastWestScore, value);
    }



    /// <summary>Tricks the human's team has won this round.</summary>
    public int UsTricks
    {
        get => _usTricks;
        private set => SetProperty(ref _usTricks, value);
    }



    /// <summary>Tricks the opponents have won this round.</summary>
    public int ThemTricks
    {
        get => _themTricks;
        private set => SetProperty(ref _themTricks, value);
    }



    /// <summary>The trump suit's symbol, or a dash before trump is named or in ace high.</summary>
    public string TrumpSymbol
    {
        get => _trumpSymbol;
        private set => SetProperty(ref _trumpSymbol, value);
    }



    public bool IsTrumpRed
    {
        get => _isTrumpRed;
        private set
        {
            if (SetProperty(ref _isTrumpRed, value))
            {
                OnPropertyChanged(nameof(TrumpMarkColor));
            }
        }
    }



    /// <summary>The colour of the trump watermarks in the corners of the table.</summary>
    public Color TrumpMarkColor => _isTrumpRed ? TableColors.RedTrumpMark : TableColors.BlackTrumpMark;



    /// <summary>Whether a trump suit is named (the table shows it in its corners).</summary>
    public bool HasTrumpSuit
    {
        get => _hasTrumpSuit;
        private set => SetProperty(ref _hasTrumpSuit, value);
    }



    /// <summary>"US" or "THEM": the team that won the bid, or empty during the bidding.</summary>
    public string TrumpSetterTeam
    {
        get => _trumpSetterTeam;
        private set
        {
            if (SetProperty(ref _trumpSetterTeam, value))
            {
                OnPropertyChanged(nameof(IsTrumpSetterUs));
            }
        }
    }



    public bool IsTrumpSetterUs => string.Equals(_trumpSetterTeam, "US", StringComparison.Ordinal);



    /// <summary>The winning bid next to the team, for example "· 7" or "· Hawsey (24)".</summary>
    public string BidAmountText
    {
        get => _bidAmountText;
        private set => SetProperty(ref _bidAmountText, value);
    }



    /// <summary>Who led the trick on the table, for example "Led by Partner".</summary>
    public string LedByText
    {
        get => _ledByText;
        private set => SetProperty(ref _ledByText, value);
    }



    /// <summary>The card winning the trick on the table, for example "J ♦", or a dash.</summary>
    public string WinningCardText
    {
        get => _winningCardText;
        private set => SetProperty(ref _winningCardText, value);
    }



    /// <summary>
    /// The number bids the human may make now, as command parameters: from the
    /// minimum legal bid up to 11 (12 tricks is a Hawsey bid), or only the minimum
    /// when the human is the stuck dealer.
    /// </summary>
    public ObservableCollection<string> BidOptions { get; } = new();



    /// <summary>
    /// The line above the bid buttons: the high bid and who holds it, the minimum when
    /// nobody has bid, or that the human is stuck.
    /// </summary>
    public string BidPrompt
    {
        get => _bidPrompt;
        set => SetProperty(ref _bidPrompt, value);
    }



    /// <summary>
    /// Whether the human may pass. The stuck dealer must bid (the minimum, or Hawsey).
    /// </summary>
    public bool CanPass
    {
        get => _canPass;
        set => SetProperty(ref _canPass, value);
    }



    /// <summary>
    /// Whether the human is making a Hawsey exchange: tapping a card in the hand then
    /// picks (or unpicks) it as a discard instead of playing it.
    /// </summary>
    public bool IsHawseyExchangeVisible
    {
        get => _isHawseyExchangeVisible;
        set => SetProperty(ref _isHawseyExchangeVisible, value);
    }



    /// <summary>How many of the two discards are picked, for example "Selected: 1 / 2".</summary>
    public string ExchangeSelectionText
    {
        get => _exchangeSelectionText;
        set => SetProperty(ref _exchangeSelectionText, value);
    }



    /// <summary>
    /// Makes the Hawsey exchange with the two picked cards; the partner gives their two
    /// best. It can run only when exactly two cards are picked.
    /// </summary>
    public ICommand ConfirmHawseyExchangeCommand => _confirmHawseyExchangeCommand;



    public ICommand NewGameCommand { get; }
    public ICommand NextRoundCommand { get; }
    public ICommand PlaceBidCommand { get; }
    public ICommand SelectTrumpCommand { get; }
    public ICommand PlayCardCommand { get; }
    public ICommand ToggleLogCommand { get; }



    /// <summary>Flips between the table and the compact layout, whatever the page's size.</summary>
    public ICommand ToggleLayoutCommand { get; }



    /// <summary>
    /// The space the page has, in device-independent units; the page passes it on each
    /// resize, and <see cref="LayoutChoice.Auto"/> picks the layout from it.
    /// </summary>
    public void SetPageSize(double width, double height)
    {
        _pageWidth = width;
        _pageHeight = height;
        UpdateLayout();
    }



    public event PropertyChangedEventHandler? PropertyChanged;



    /// <summary>
    /// Moves a card in the shown hand, as dragging it does: the card at
    /// <paramref name="from"/> goes before the card now at <paramref name="to"/>. It
    /// changes only the order the human sees, never the game. Any picked Hawsey
    /// discards are dropped, since they are positions in the old order.
    /// </summary>
    public void MoveCard(int from, int to)
    {
        if (_handOrder == null || from == to
            || from < 0 || from >= _handOrder.Count
            || to < 0 || to >= _handOrder.Count)
        {
            return;
        }

        var card = _handOrder[from];
        _handOrder.RemoveAt(from);
        _handOrder.Insert(to > from ? to - 1 : to, card);
        _discardIndexes.Clear();
        UpdateFromState();
    }



    /// <summary>
    /// Runs a command's move. MAUI commands can't await it, so an exception thrown
    /// inside would vanish with the discarded task and the game would just stop. It is
    /// reported in the status line instead, where the player can see it.
    /// </summary>
    private void Run(Func<Task> move) => _ = RunAsync(move);



    private async Task RunAsync(Func<Task> move)
    {
        try
        {
            await move().ConfigureAwait(true);
        }
        // The top of a UI command: every failure is reported, none rethrown.
        catch (Exception ex)
        {
            StatusMessage = $"Something went wrong: {ex.Message}";
        }
    }



    private async Task StartNewGameAsync()
    {
        IsBiddingVisible = false;
        IsTrumpPickerVisible = false;
        RoundSummary = null;
        _handOrder = null;
        _discardIndexes.Clear();
        _gameService.StartNewGame();
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task StartNextRoundAsync()
    {
        var state = _gameService.CurrentState;

        if (state is not { Phase: GamePhase.RoundScoring })
        {
            return;
        }

        RoundSummary = null;
        _handOrder = null;
        _gameService.StartNextRound();
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task PlaceBidAsync(string bidString)
    {
        BidAction? bid = null;

        if (string.Equals(bidString, "pass", StringComparison.Ordinal))
        {
            bid = BidAction.PassBid.Instance;
        }
        else if (string.Equals(bidString, "hawsey", StringComparison.Ordinal))
        {
            bid = BidAction.HawseyBid.Instance;
        }
        else if (int.TryParse(bidString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
        {
            bid = new BidAction.NumberBid(amount);
        }

        // A stale tap (not the human's turn to bid any more) is ignored by the service.
        if (bid == null || !_gameService.PlaceHumanBid(bid))
        {
            return;
        }

        IsBiddingVisible = false;
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task SelectTrumpAsync(string suitString)
    {
        Suit? trump = suitString switch
        {
            "hearts" => Suit.Hearts,
            "diamonds" => Suit.Diamonds,
            "clubs" => Suit.Clubs,
            "spades" => Suit.Spades,
            _ => null
        };

        if (!_gameService.SelectTrump(trump))
        {
            return;
        }

        IsTrumpPickerVisible = false;
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task PlayCardAsync(CardViewModel? cardVm)
    {
        if (cardVm == null)
        {
            return;
        }

        if (IsHawseyExchangeVisible)
        {
            ToggleDiscard(cardVm);
            return;
        }

        if (!cardVm.IsLegal)
        {
            return;
        }

        // A double-tap, or a tap that lands during an AI turn, is rejected by the
        // service; only an accepted move advances the game.
        if (!_gameService.PlayHumanCard(cardVm.Card))
        {
            return;
        }

        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private void ToggleDiscard(CardViewModel cardVm)
    {
        var index = HumanCards.IndexOf(cardVm);

        if (index < 0)
        {
            return;
        }

        if (!_discardIndexes.Remove(index))
        {
            if (_discardIndexes.Count == HawseyDiscardCount)
            {
                return;
            }

            _discardIndexes.Add(index);
        }

        UpdateFromState();
    }



    private async Task ConfirmHawseyExchangeAsync()
    {
        if (_handOrder == null || _discardIndexes.Count != HawseyDiscardCount)
        {
            return;
        }

        var discard = _discardIndexes.Select(i => _handOrder[i]).ToArray();

        if (!_gameService.PerformHumanHawseyExchange(discard))
        {
            return;
        }

        await AdvanceGameAsync().ConfigureAwait(true);
    }



    /// <summary>
    /// Lets the engine play the AI seats until it waits for the human (or for the next
    /// round), then shows what the human must do. The loop itself is the engine's
    /// (<see cref="GameSession.AdvanceAsync"/>), shared with the Blazor UI, and so are the
    /// messages: after each round the summary waits for "Next round".
    /// </summary>
    private async Task AdvanceGameAsync()
    {
        if (_gameService.CurrentState?.Phase == GamePhase.Bidding)
        {
            StatusMessage = "Bidding...";
        }

        switch (await _gameService.AdvanceAsync().ConfigureAwait(true))
        {
            case WaitingFor.HumanBid:
                IsBiddingVisible = true;
                StatusMessage = "Your turn to bid.";
                break;

            case WaitingFor.HumanTrump:
                IsTrumpPickerVisible = true;
                StatusMessage = "Pick trump.";
                break;

            case WaitingFor.HumanHawseyExchange:
                // The exchange panel follows the state (UpdateHawseyExchange).
                StatusMessage = "Hawsey! Pick 2 cards to discard.";
                break;

            case WaitingFor.HumanCard:
                StatusMessage = "Your turn. Play a card.";
                break;

            case WaitingFor.NextRound:
                StatusMessage = "Round complete.";
                RoundSummary = RoundSummaryViewModel.From(_gameService.CurrentState!, isGameOver: false);
                break;

            case WaitingFor.GameOver:
                StatusMessage = "Game over.";
                RoundSummary = RoundSummaryViewModel.From(_gameService.CurrentState!, isGameOver: true);
                break;

            default:
                // A New Game took over during an AI pause; its own loop shows it.
                return;
        }

        UpdateFromState();
    }



    private void OnStateChanged(object? sender, EventArgs e)
    {
        _dispatcher.Post(UpdateFromState);
    }



    private void UpdateFromState()
    {
        var state = _gameService.CurrentState;

        if (state == null)
        {
            return;
        }

        UpdatePanels(state);
        UpdateSeats(state);
        UpdateTrickInfo();
        UpdateHawseyExchange(state);
        UpdateHumanHand(state);
        UpdateGameLog(state);

        if (state.Phase == GamePhase.Bidding)
        {
            UpdateBidChoices(state);
        }
    }



    private void UpdatePanels(GameState state)
    {
        NorthSouthScore = state.NorthSouthScore;
        EastWestScore = state.EastWestScore;
        UsTricks = state.CompletedTricks.Count(t => t.Winner.GetTeam() == Team.NorthSouth);
        ThemTricks = state.CompletedTricks.Count(t => t.Winner.GetTeam() == Team.EastWest);

        // Until trump is named the engine's TrumpSuit is empty, as it is in ace high.
        var trump = state.Phase is GamePhase.Bidding or GamePhase.TrumpSelection ? null : state.TrumpSuit;
        HasTrumpSuit = trump.HasValue;
        TrumpSymbol = trump.HasValue ? SuitSymbol(trump.Value) : NoCard;
        IsTrumpRed = trump is Suit.Hearts or Suit.Diamonds;

        var result = state.BiddingResult;

        if (result == null)
        {
            TrumpSetterTeam = "";
            BidAmountText = "";
            return;
        }

        TrumpSetterTeam = result.Winner.GetTeam() == Team.NorthSouth ? "US" : "THEM";
        var bid = result.IsHawsey ? "Hawsey (24)" : result.BidAmount.ToString(CultureInfo.InvariantCulture);
        BidAmountText = $"· {bid}";
    }



    private void UpdateSeats(GameState state)
    {
        var bids = _gameService.Bids;
        var table = _gameService.TableCards;

        North.Update(state, bids, table);
        East.Update(state, bids, table);
        West.Update(state, bids, table);
        South.Update(state, bids, table);
    }



    private void UpdateTrickInfo()
    {
        var table = _gameService.TableCards;
        LedByText = table.Count == 0 ? "" : $"Led by {LeaderName(table[0].Player)}";

        var winning = _gameService.TableWinningCard;
        WinningCardText = winning is { } card ? $"{RankText(card.Rank)} {SuitSymbol(card.Suit)}" : NoCard;
    }



    /// <summary>
    /// The exchange panel shows while the human, as the Hawsey bidder, has yet to
    /// exchange. Any other state (the exchange made, or a New Game) drops the picks.
    /// </summary>
    private void UpdateHawseyExchange(GameState state)
    {
        var exchanging = state is { Phase: GamePhase.HawseyExchange, HawseyBidder: GameSession.HumanPosition };

        if (!exchanging)
        {
            _discardIndexes.Clear();
        }

        IsHawseyExchangeVisible = exchanging;
        ExchangeSelectionText = $"Selected: {_discardIndexes.Count} / {HawseyDiscardCount}";
        _confirmHawseyExchangeCommand.ChangeCanExecute();
    }



    /// <summary>
    /// The bid panel's prompt and buttons, from the engine's bidding progress. Same
    /// rules and words as the Blazor UI: the minimum legal bid up to 11; the stuck dealer
    /// may only bid the minimum or call Hawsey.
    /// </summary>
    private void UpdateBidChoices(GameState state)
    {
        const int HighestNumberBid = 11;

        var stuck = state.IsNextBidderStuck;
        var highest = stuck ? state.MinimumLegalBid : HighestNumberBid;

        BidOptions.Clear();

        for (var amount = state.MinimumLegalBid; amount <= highest; amount++)
        {
            BidOptions.Add(amount.ToString(CultureInfo.InvariantCulture));
        }

        CanPass = !stuck;
        BidPrompt = GetBidPrompt(state, stuck);
    }



    private static string GetBidPrompt(GameState state, bool stuck)
    {
        if (stuck)
        {
            return $"You're stuck as dealer — bid {state.MinimumLegalBid} or call Hawsey.";
        }

        if (state.HighBidder is { } holder)
        {
            return $"Current high bid: {state.HighBid} by {BidderName(holder)}";
        }

        return $"No bids yet. Minimum: {state.MinimumLegalBid}";
    }



    // The human never faces their own bid (bidding goes round once), so the holder is
    // the partner or an opponent.
    private static string BidderName(PlayerPosition player) =>
        player == PlayerPosition.North ? "your partner" : $"{player} (opponents)";



    private static string LeaderName(PlayerPosition player) => player switch
    {
        PlayerPosition.North => "Partner",
        PlayerPosition.East => "Right",
        PlayerPosition.South => "You",
        _ => "Left",
    };



    private void UpdateHumanHand(GameState state)
    {
        var humanPlays = state.Phase == GamePhase.TrickPlay && state.NextToAct == GameSession.HumanPosition;
        var legalPlays = humanPlays ? state.GetLegalPlays() : Array.Empty<Card>();
        var hand = SyncHandOrder(state.Hands[GameSession.HumanPosition]);

        HumanCards.Clear();

        for (var i = 0; i < hand.Count; i++)
        {
            var card = hand[i];

            // In the exchange every card may be picked as a discard.
            HumanCards.Add
            (
                IsHawseyExchangeVisible
                    ? new CardViewModel(card, isLegal: false, isSelected: _discardIndexes.Contains(i), isInteractive: true)
                    : new CardViewModel(card, legalPlays.Contains(card))
            );
        }
    }



    /// <summary>
    /// Brings the shown order up to date with the hand, as the Blazor UI does: a new deal
    /// is sorted; after that, cards that left the hand (played, or discarded in the
    /// exchange) drop out, the rest keep the human's order, and cards that arrived (from
    /// the partner in the exchange) are added at the end in sorted order.
    /// </summary>
    private List<Card> SyncHandOrder(List<Card> hand)
    {
        if (_handOrder == null)
        {
            _handOrder = new List<Card>(hand);
            _handOrder.Sort(CompareForDisplay);
            return _handOrder;
        }

        var remaining = new List<Card>(hand);
        var order = new List<Card>(hand.Count);

        foreach (var card in _handOrder)
        {
            if (remaining.Remove(card))
            {
                order.Add(card);
            }
        }

        remaining.Sort(CompareForDisplay);
        order.AddRange(remaining);
        _handOrder = order;
        return order;
    }



    // Spades, hearts, clubs, diamonds (alternating colours), high rank first.
    private static int CompareForDisplay(Card a, Card b)
    {
        var bySuit = SuitDisplayOrder(a.Suit).CompareTo(SuitDisplayOrder(b.Suit));
        return bySuit != 0 ? bySuit : ((int)b.Rank).CompareTo((int)a.Rank);
    }



    private static int SuitDisplayOrder(Suit suit) => suit switch
    {
        Suit.Spades => 0,
        Suit.Hearts => 1,
        Suit.Clubs => 2,
        _ => 3,
    };



    private void UpdateGameLog(GameState state)
    {
        var entries = new List<string>();

        foreach (var bid in _gameService.Bids)
        {
            var what = bid.Action switch
            {
                BidAction.NumberBid n => $"bids {n.Amount}",
                BidAction.HawseyBid => "calls Hawsey",
                _ => "passes",
            };
            entries.Add($"{bid.Player} {what}.");
        }

        for (var i = 0; i < state.CompletedTricks.Count; i++)
        {
            entries.Add($"Trick {i + 1}: {state.CompletedTricks[i].Winner} wins.");
        }

        if (entries.SequenceEqual(GameLog, StringComparer.Ordinal))
        {
            return;
        }

        GameLog.Clear();

        foreach (var entry in entries)
        {
            GameLog.Add(entry);
        }
    }



    private static string SuitSymbol(Suit suit) => suit switch
    {
        Suit.Hearts => "♥",
        Suit.Diamonds => "♦",
        Suit.Clubs => "♣",
        _ => "♠",
    };



    private static string RankText(Rank rank) => rank switch
    {
        Rank.Nine => "9",
        Rank.Ten => "10",
        Rank.Jack => "J",
        Rank.Queen => "Q",
        Rank.King => "K",
        _ => "A",
    };



    private void ToggleLayout()
    {
        _layoutChoice = _isCompactLayout ? LayoutChoice.Table : LayoutChoice.Compact;
        UpdateLayout();
    }



    private void UpdateLayout() =>
        IsCompactLayout = LayoutPicker.UseCompact(_pageWidth, _pageHeight, _layoutChoice);



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
