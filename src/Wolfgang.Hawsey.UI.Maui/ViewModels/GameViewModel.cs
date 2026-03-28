#pragma warning disable AsyncFixer01
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

        NewGameCommand = new Command(async () => await StartNewGameAsync());
        PlaceBidCommand = new Command<string>(async (s) => await PlaceBidAsync(s));
        SelectTrumpCommand = new Command<string>(async (s) => await SelectTrumpAsync(s));
        PlayCardCommand = new Command<CardViewModel>(async (c) => await PlayCardAsync(c));
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
        await AdvanceGameAsync();
    }



    private async Task PlaceBidAsync(string bidString)
    {
        if (bidString == "pass")
        {
            _gameService.PlaceHumanBid(BidAction.PassBid.Instance);
        }
        else if (bidString == "hawsey")
        {
            _gameService.PlaceHumanBid(BidAction.HawseyBid.Instance);
        }
        else if (int.TryParse(bidString, out var amount))
        {
            _gameService.PlaceHumanBid(new BidAction.NumberBid(amount));
        }

        IsBiddingVisible = false;
        await AdvanceGameAsync();
    }



    private async Task SelectTrumpAsync(string suitString)
    {
        Suit? trump = suitString switch
        {
            "hearts" => Suit.Hearts,
            "diamonds" => Suit.Diamonds,
            "clubs" => Suit.Clubs,
            "spades" => Suit.Spades,
            _ => null // ace high
        };

        IsTrumpPickerVisible = false;
        _gameService.SelectTrump(trump);
        await AdvanceGameAsync();
    }



    private async Task PlayCardAsync(CardViewModel? cardVm)
    {
        if (cardVm == null || !cardVm.IsLegal)
        {
            return;
        }

        _gameService.PlayHumanCard(cardVm.Card);
        await AdvanceGameAsync();
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
                var humanNeedsToBid = await _gameService.AdvanceAiBiddingAsync();

                if (humanNeedsToBid)
                {
                    StatusMessage = "Your turn to bid";
                    IsBiddingVisible = true;
                }
                else
                {
                    await AdvanceGameAsync();
                }

                break;

            case GamePhase.TrumpSelection:
                var humanSelectsTrump = await _gameService.HandleTrumpSelectionAsync();

                if (humanSelectsTrump)
                {
                    StatusMessage = "Choose trump suit or Ace High";
                    IsTrumpPickerVisible = true;
                }
                else
                {
                    await AdvanceGameAsync();
                }

                break;

            case GamePhase.HawseyExchange:
                var humanExchanges = await _gameService.HandleHawseyExchangeAsync();

                if (humanExchanges)
                {
                    StatusMessage = "Hawsey! Select cards to exchange";
                    // TODO: show exchange UI
                }
                else
                {
                    await AdvanceGameAsync();
                }

                break;

            case GamePhase.TrickPlay:
                var humanPlays = await _gameService.AdvanceAiPlaysAsync();

                if (humanPlays)
                {
                    StatusMessage = "Your turn to play";
                }

                break;

            case GamePhase.RoundScoring:
                await Task.Delay(1500);
                _gameService.StartNextRound();
                await AdvanceGameAsync();
                break;

            case GamePhase.GameOver:
                break;
        }
    }



    private void OnStateChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateFromState);
    }



    private void OnTrickCompleted(PlayerPosition winner)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"{winner} wins the trick!";
        });
    }



    private void OnRoundCompleted()
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



    private void OnGameOver(Team winner)
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

        NorthSouthScore = state.NorthSouthScore;
        EastWestScore = state.EastWestScore;
        NorthCardCount = state.Hands[PlayerPosition.North].Count;
        EastCardCount = state.Hands[PlayerPosition.East].Count;
        WestCardCount = state.Hands[PlayerPosition.West].Count;

        // Update trump display
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
            TrumpDisplay = $"Trump: {symbol}";
        }
        else if (state.TrumpMode == TrumpMode.AceHigh)
        {
            TrumpDisplay = "Ace High";
        }
        else
        {
            TrumpDisplay = "";
        }

        // Update bid info
        if (state.BiddingResult != null)
        {
            var result = state.BiddingResult;
            BidInfoDisplay = result.IsHawsey
                ? $"{result.Winner} called Hawsey!"
                : result.IsStuck
                    ? $"{result.Winner} stuck at {result.BidAmount}"
                    : $"{result.Winner} bid {result.BidAmount}";
        }
        else
        {
            BidInfoDisplay = "";
        }

        // Update human hand
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

        // Update trick area
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



public class TrickCardViewModel
{
    public TrickCardViewModel(Card card, PlayerPosition player)
    {
        Card = card;
        Player = player;
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
    public PlayerPosition Player { get; }
    public string RankText { get; }
    public string SuitSymbol { get; }
    public Color SuitColor { get; }
}
