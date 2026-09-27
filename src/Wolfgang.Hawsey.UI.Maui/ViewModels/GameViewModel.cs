#pragma warning disable AsyncFixer01
#pragma warning disable AsyncFixer03
#pragma warning disable VSTHRD101
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.UI.Maui.Threading;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

// `partial` is required on the Windows TFM: GameViewModel implements WinRT-projected
// interfaces, and the CsWinRT AOT source generator emits the other part of this
// class there (without it, CsWinRT1028). InspectCode analyzes a slice where that
// generator does not run, sees a single part, and reports the modifier as redundant.
// ReSharper disable once PartialTypeWithSinglePart
public partial class GameViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameService;
    private readonly IUiDispatcher _dispatcher;
    private string _statusMessage = "Welcome to Hawsey! Tap New Game to start.";
    private bool _isBiddingVisible;
    private bool _isTrumpPickerVisible;
    private bool _isGameOverVisible;
    private string _gameOverMessage = "";
    private int _northSouthScore;
    private int _eastWestScore;
    private string _trumpDisplay = "";
    private string _bidInfoDisplay = "";
    private int _northCardCount;
    private int _eastCardCount;
    private int _westCardCount;
    private string _bidPrompt = "";
    private bool _canPass = true;
    private bool _isHawseyExchangeVisible;
    private string _exchangeSelectionText = "";

    // Indexes into the human's hand, not cards: a pinochle hand can hold both copies
    // of a card, and the human may discard both.
    private readonly List<int> _discardIndexes = new();
    private readonly Command _confirmHawseyExchangeCommand;



    public GameViewModel(GameSession gameService, IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(gameService);
        ArgumentNullException.ThrowIfNull(dispatcher);

        _gameService = gameService;
        _dispatcher = dispatcher;
        _gameService.StateChanged += OnStateChanged;
        _gameService.TrickCompleted += OnTrickCompleted;
        _gameService.RoundCompleted += OnRoundCompleted;
        _gameService.GameOver += OnGameOver;

        // Command takes Action/Action<T>; an `async () => await ...` lambda would be
        // async-void (MA0147). Each command starts its move through Run, which reports
        // a failure instead of letting the discarded task lose it.
        NewGameCommand = new Command(() => Run(StartNewGameAsync));
        PlaceBidCommand = new Command<string>(s => Run(() => PlaceBidAsync(s)));
        SelectTrumpCommand = new Command<string>(s => Run(() => SelectTrumpAsync(s)));
        PlayCardCommand = new Command<CardViewModel>(c => Run(() => PlayCardAsync(c)));
        _confirmHawseyExchangeCommand = new Command
        (
            () => Run(ConfirmHawseyExchangeAsync),
            () => _discardIndexes.Count == HawseyDiscardCount
        );
    }



    /// <summary>The human discards two cards in a Hawsey exchange.</summary>
    public const int HawseyDiscardCount = 2;



    public ObservableCollection<CardViewModel> HumanCards { get; } = new();



    public ObservableCollection<TrickCardViewModel> TrickCards { get; } = new();



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



    public bool IsGameOverVisible
    {
        get => _isGameOverVisible;
        set => SetProperty(ref _isGameOverVisible, value);
    }



    public string GameOverMessage
    {
        get => _gameOverMessage;
        set => SetProperty(ref _gameOverMessage, value);
    }



    public int NorthSouthScore
    {
        get => _northSouthScore;
        set => SetProperty(ref _northSouthScore, value);
    }



    public int EastWestScore
    {
        get => _eastWestScore;
        set => SetProperty(ref _eastWestScore, value);
    }



    public string TrumpDisplay
    {
        get => _trumpDisplay;
        set => SetProperty(ref _trumpDisplay, value);
    }



    public string BidInfoDisplay
    {
        get => _bidInfoDisplay;
        set => SetProperty(ref _bidInfoDisplay, value);
    }



    public int NorthCardCount
    {
        get => _northCardCount;
        set => SetProperty(ref _northCardCount, value);
    }



    public int EastCardCount
    {
        get => _eastCardCount;
        set => SetProperty(ref _eastCardCount, value);
    }



    public int WestCardCount
    {
        get => _westCardCount;
        set => SetProperty(ref _westCardCount, value);
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
    public ICommand PlaceBidCommand { get; }
    public ICommand SelectTrumpCommand { get; }
    public ICommand PlayCardCommand { get; }



    public event PropertyChangedEventHandler? PropertyChanged;



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
        IsGameOverVisible = false;
        _gameService.StartNewGame();
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
        else if (int.TryParse(bidString, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var amount))
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
        var state = _gameService.CurrentState;

        if (state == null || _discardIndexes.Count != HawseyDiscardCount)
        {
            return;
        }

        var hand = state.Hands[GameSession.HumanPosition];
        var discard = _discardIndexes.Select(i => hand[i]).ToArray();

        if (!_gameService.PerformHumanHawseyExchange(discard))
        {
            return;
        }

        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task AdvanceGameAsync()
    {
        var state = _gameService.CurrentState;

        if (state == null)
        {
            return;
        }

        switch (state.Phase)
        {
            case GamePhase.Bidding:
                await AdvanceBiddingPhaseAsync().ConfigureAwait(true);
                break;

            case GamePhase.TrumpSelection:
                await AdvanceTrumpSelectionPhaseAsync().ConfigureAwait(true);
                break;

            case GamePhase.HawseyExchange:
                await AdvanceHawseyExchangePhaseAsync().ConfigureAwait(true);
                break;

            case GamePhase.TrickPlay:
                await AdvanceTrickPlayPhaseAsync().ConfigureAwait(true);
                break;

            case GamePhase.RoundScoring:
                await _gameService.PauseBeforeNextRoundAsync().ConfigureAwait(true);
                _gameService.StartNextRound();
                await AdvanceGameAsync().ConfigureAwait(true);
                break;

            case GamePhase.GameOver:
                break;
        }
    }



    private async Task AdvanceBiddingPhaseAsync()
    {
        var humanNeedsToBid = await _gameService.AdvanceAiBiddingAsync().ConfigureAwait(true);

        if (humanNeedsToBid)
        {
            StatusMessage = "Your turn to bid";
            IsBiddingVisible = true;
        }
        else
        {
            await AdvanceGameAsync().ConfigureAwait(true);
        }
    }



    private async Task AdvanceTrumpSelectionPhaseAsync()
    {
        var humanSelectsTrump = await _gameService.HandleTrumpSelectionAsync().ConfigureAwait(true);

        if (humanSelectsTrump)
        {
            StatusMessage = "Choose trump suit or Ace High";
            IsTrumpPickerVisible = true;
        }
        else
        {
            await AdvanceGameAsync().ConfigureAwait(true);
        }
    }



    private async Task AdvanceHawseyExchangePhaseAsync()
    {
        var humanExchanges = await _gameService.HandleHawseyExchangeAsync().ConfigureAwait(true);

        if (humanExchanges)
        {
            StatusMessage = "Hawsey! Tap two cards to discard";
        }
        else
        {
            await AdvanceGameAsync().ConfigureAwait(true);
        }
    }



    private async Task AdvanceTrickPlayPhaseAsync()
    {
        var humanPlays = await _gameService.AdvanceAiPlaysAsync().ConfigureAwait(true);

        if (humanPlays)
        {
            StatusMessage = "Your turn to play";
        }
        else if (_gameService.CurrentState?.Phase == GamePhase.RoundScoring)
        {
            // An AI card finished the round: go on to the next deal, as a human
            // card that finishes it does. Game over and a New Game in the meantime
            // (a different phase) stop here.
            await AdvanceGameAsync().ConfigureAwait(true);
        }
    }



    private void OnStateChanged(object? sender, EventArgs e)
    {
        _dispatcher.Post(UpdateFromState);
    }



    private void OnTrickCompleted(object? sender, TrickCompletedEventArgs e)
    {
        _dispatcher.Post(() =>
        {
            StatusMessage = $"{e.Winner} wins the trick!";
        });
    }



    private void OnRoundCompleted(object? sender, EventArgs e)
    {
        _dispatcher.Post(() =>
        {
            var state = _gameService.CurrentState;

            if (state != null)
            {
                StatusMessage = $"Round over! NS: {state.NorthSouthScore} — EW: {state.EastWestScore}";
            }
        });
    }



    private void OnGameOver(object? sender, GameOverEventArgs e)
    {
        _dispatcher.Post(() =>
        {
            var state = _gameService.CurrentState;
            var winnerText = e.Winner == Team.NorthSouth ? "North/South (Your team)" : "East/West";
            GameOverMessage = $"{winnerText} wins!\n\nNS: {state?.NorthSouthScore} — EW: {state?.EastWestScore}";
            IsGameOverVisible = true;
            StatusMessage = "Game Over!";
        });
    }



    private void UpdateFromState()
    {
        var state = _gameService.CurrentState;

        if (state == null)
        {
            return;
        }

        UpdateScoresAndInfo(state);
        UpdateHawseyExchange(state);
        UpdateHumanHand(state);
        UpdateTrickArea(state);
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



    private void UpdateScoresAndInfo(GameState state)
    {
        NorthSouthScore = state.NorthSouthScore;
        EastWestScore = state.EastWestScore;
        NorthCardCount = state.Hands[PlayerPosition.North].Count;
        EastCardCount = state.Hands[PlayerPosition.East].Count;
        WestCardCount = state.Hands[PlayerPosition.West].Count;

        TrumpDisplay = GetTrumpDisplayText(state);
        BidInfoDisplay = GetBidInfoText(state);

        if (state.Phase == GamePhase.Bidding)
        {
            UpdateBidChoices(state);
        }
    }



    /// <summary>
    /// The bid overlay's prompt and buttons, from the engine's bidding progress. Same
    /// rules as the Blazor UI: the minimum legal bid up to 11; the stuck dealer may only
    /// bid the minimum or call Hawsey.
    /// </summary>
    private void UpdateBidChoices(GameState state)
    {
        const int HighestNumberBid = 11;

        var stuck = state.IsNextBidderStuck;
        var highest = stuck ? state.MinimumLegalBid : HighestNumberBid;

        BidOptions.Clear();

        for (var amount = state.MinimumLegalBid; amount <= highest; amount++)
        {
            BidOptions.Add(amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        CanPass = !stuck;
        BidPrompt = GetBidPrompt(state, stuck);
    }



    private static string GetBidPrompt(GameState state, bool stuck)
    {
        if (stuck)
        {
            return $"You're stuck as dealer: bid {state.MinimumLegalBid} or call Hawsey.";
        }

        if (state.HighBidder is { } holder)
        {
            return $"Current high bid: {state.HighBid} by {BidderName(holder)}";
        }

        return $"No bids yet. Minimum: {state.MinimumLegalBid}";
    }



    // The human never faces its own bid (bidding goes round once), so the holder is
    // the partner or an opponent.
    private static string BidderName(PlayerPosition player) =>
        player == PlayerPosition.North ? "North (your partner)" : $"{player} (opponents)";



    private static string GetTrumpDisplayText(GameState state)
    {
        // Until trump is named, the engine's TrumpMode is a placeholder (AceHigh).
        if (state.Phase is GamePhase.Bidding or GamePhase.TrumpSelection)
        {
            return "";
        }

        if (state.TrumpSuit.HasValue)
        {
            var symbol = state.TrumpSuit.Value switch
            {
                Suit.Hearts => "\u2665",
                Suit.Diamonds => "\u2666",
                Suit.Clubs => "\u2663",
                Suit.Spades => "\u2660",
                _ => "?"
            };

            return $"Trump: {symbol}";
        }

        return state.TrumpMode == TrumpMode.AceHigh ? "Ace High" : "";
    }



    private static string GetBidInfoText(GameState state)
    {
        if (state.BiddingResult == null)
        {
            return "";
        }

        var result = state.BiddingResult;

        if (result.IsHawsey)
        {
            return $"{result.Winner} called Hawsey!";
        }

        if (result.IsStuck)
        {
            return $"{result.Winner} stuck at {result.BidAmount}";
        }

        return $"{result.Winner} bid {result.BidAmount}";
    }



    private void UpdateHumanHand(GameState state)
    {
        var legalPlays = state.Phase == GamePhase.TrickPlay && state.NextToAct == GameSession.HumanPosition
            ? state.GetLegalPlays()
            : Array.Empty<Card>();

        HumanCards.Clear();

        var humanHand = state.Hands[GameSession.HumanPosition];

        for (var i = 0; i < humanHand.Count; i++)
        {
            var card = humanHand[i];
            var isLegal = false;

            for (var j = 0; j < legalPlays.Count; j++)
            {
                if (legalPlays[j].Equals(card))
                {
                    isLegal = true;
                    break;
                }
            }

            // In the exchange every card may be picked as a discard.
            HumanCards.Add
            (
                IsHawseyExchangeVisible
                    ? new CardViewModel(card, isLegal: true, isSelected: _discardIndexes.Contains(i))
                    : new CardViewModel(card, isLegal)
            );
        }
    }



    private void UpdateTrickArea(GameState state)
    {
        TrickCards.Clear();

        if (state.CurrentTrick != null)
        {
            for (var i = 0; i < state.CurrentTrick.Plays.Count; i++)
            {
                var play = state.CurrentTrick.Plays[i];
                TrickCards.Add(new TrickCardViewModel(play.Card));
            }
        }
    }



    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
