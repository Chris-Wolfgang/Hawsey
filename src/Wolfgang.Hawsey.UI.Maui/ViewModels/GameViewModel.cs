#pragma warning disable AsyncFixer01
#pragma warning disable AsyncFixer03
#pragma warning disable VSTHRD101
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.UI.Maui.Services;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly GameService _gameService;
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



    public GameViewModel(GameService gameService)
    {
        _gameService = gameService;
        _gameService.StateChanged += OnStateChanged;
        _gameService.TrickCompleted += OnTrickCompleted;
        _gameService.RoundCompleted += OnRoundCompleted;
        _gameService.GameOver += OnGameOver;

        // Command takes Action/Action<T>; an `async () => await ...` lambda would be
        // async-void (MA0147). Discard the Task instead — MAUI's Command pattern is
        // already fire-and-forget for the caller.
        NewGameCommand = new Command(() => _ = StartNewGameAsync());
        PlaceBidCommand = new Command<string>(s => _ = PlaceBidAsync(s));
        SelectTrumpCommand = new Command<string>(s => _ = SelectTrumpAsync(s));
        PlayCardCommand = new Command<CardViewModel>(c => _ = PlayCardAsync(c));
    }



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



    public ICommand NewGameCommand { get; }
    public ICommand PlaceBidCommand { get; }
    public ICommand SelectTrumpCommand { get; }
    public ICommand PlayCardCommand { get; }



    public event PropertyChangedEventHandler? PropertyChanged;



    private async Task StartNewGameAsync()
    {
        IsGameOverVisible = false;
        _gameService.StartNewGame();
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task PlaceBidAsync(string bidString)
    {
        if (string.Equals(bidString, "pass", StringComparison.Ordinal))
        {
            _gameService.PlaceHumanBid(BidAction.PassBid.Instance);
        }
        else if (string.Equals(bidString, "hawsey", StringComparison.Ordinal))
        {
            _gameService.PlaceHumanBid(BidAction.HawseyBid.Instance);
        }
        else if (int.TryParse(bidString, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            _gameService.PlaceHumanBid(new BidAction.NumberBid(amount));
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

        IsTrumpPickerVisible = false;
        _gameService.SelectTrump(trump);
        await AdvanceGameAsync().ConfigureAwait(true);
    }



    private async Task PlayCardAsync(CardViewModel? cardVm)
    {
        if (cardVm == null || !cardVm.IsLegal)
        {
            return;
        }

        _gameService.PlayHumanCard(cardVm.Card);
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
                await Task.Delay(1500).ConfigureAwait(true);
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
            StatusMessage = "Hawsey! Select cards to exchange";
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
    }



    private void OnStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateFromState);
    }



    private void OnTrickCompleted(object? sender, PlayerPosition winner)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"{winner} wins the trick!";
        });
    }



    private void OnRoundCompleted(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var state = _gameService.CurrentState;

            if (state != null)
            {
                StatusMessage = $"Round over! NS: {state.NorthSouthScore} — EW: {state.EastWestScore}";
            }
        });
    }



    private void OnGameOver(object? sender, Team winner)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var state = _gameService.CurrentState;
            var winnerText = winner == Team.NorthSouth ? "North/South (Your team)" : "East/West";
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
        UpdateHumanHand(state);
        UpdateTrickArea(state);
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
    }



    private static string GetTrumpDisplayText(GameState state)
    {
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
        var legalPlays = state.Phase == GamePhase.TrickPlay && state.NextToAct == GameService.HumanPosition
            ? state.GetLegalPlays()
            : Array.Empty<Card>();

        HumanCards.Clear();

        var humanHand = state.Hands[GameService.HumanPosition];

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

            HumanCards.Add(new CardViewModel(card, isLegal));
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
                TrickCards.Add(new TrickCardViewModel(play.Card, play.Player));
            }
        }
    }



    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
