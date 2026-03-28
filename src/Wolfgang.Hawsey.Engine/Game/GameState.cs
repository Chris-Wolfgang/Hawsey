namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// An immutable snapshot of the entire game state at a point in time.
/// </summary>
public sealed class GameState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameState"/> class.
    /// </summary>
    public GameState
    (
        GamePhase phase,
        PlayerPosition dealer,
        Dictionary<PlayerPosition, List<Card>> hands,
        Suit? trumpSuit,
        TrumpMode trumpMode,
        BiddingResult? biddingResult,
        List<TrickResult> completedTricks,
        Trick? currentTrick,
        int northSouthScore,
        int eastWestScore,
        HouseRules rules,
        PlayerPosition? nextToAct,
        int tricksPlayedInRound,
        bool isHawseyRound,
        PlayerPosition? hawseyBidder
    )
    {
        Phase = phase;
        Dealer = dealer;
        Hands = hands;
        TrumpSuit = trumpSuit;
        TrumpMode = trumpMode;
        BiddingResult = biddingResult;
        CompletedTricks = completedTricks;
        CurrentTrick = currentTrick;
        NorthSouthScore = northSouthScore;
        EastWestScore = eastWestScore;
        Rules = rules;
        NextToAct = nextToAct;
        TricksPlayedInRound = tricksPlayedInRound;
        IsHawseyRound = isHawseyRound;
        HawseyBidder = hawseyBidder;
    }



    /// <summary>
    /// Gets the current game phase.
    /// </summary>
    public GamePhase Phase { get; }



    /// <summary>
    /// Gets the current dealer's position.
    /// </summary>
    public PlayerPosition Dealer { get; }



    /// <summary>
    /// Gets the hands for each player.
    /// </summary>
    public Dictionary<PlayerPosition, List<Card>> Hands { get; }



    /// <summary>
    /// Gets the trump suit, or <c>null</c> for Ace high mode.
    /// </summary>
    public Suit? TrumpSuit { get; }



    /// <summary>
    /// Gets the trump mode.
    /// </summary>
    public TrumpMode TrumpMode { get; }



    /// <summary>
    /// Gets the bidding result, or <c>null</c> if bidding has not completed.
    /// </summary>
    public BiddingResult? BiddingResult { get; }



    /// <summary>
    /// Gets the tricks completed in the current round.
    /// </summary>
    public List<TrickResult> CompletedTricks { get; }



    /// <summary>
    /// Gets the current trick in progress, or <c>null</c>.
    /// </summary>
    public Trick? CurrentTrick { get; }



    /// <summary>
    /// Gets the current North/South score.
    /// </summary>
    public int NorthSouthScore { get; }



    /// <summary>
    /// Gets the current East/West score.
    /// </summary>
    public int EastWestScore { get; }



    /// <summary>
    /// Gets the house rules in effect.
    /// </summary>
    public HouseRules Rules { get; }



    /// <summary>
    /// Gets the player who should act next, or <c>null</c> if no action is needed.
    /// </summary>
    public PlayerPosition? NextToAct { get; }



    /// <summary>
    /// Gets the number of tricks played in the current round.
    /// </summary>
    public int TricksPlayedInRound { get; }



    /// <summary>
    /// Gets whether this is a Hawsey round.
    /// </summary>
    public bool IsHawseyRound { get; }



    /// <summary>
    /// Gets the Hawsey bidder's position, or <c>null</c> if not a Hawsey round.
    /// </summary>
    public PlayerPosition? HawseyBidder { get; }



    /// <summary>
    /// Gets the legal cards the current player can play, or an empty list if not in trick play.
    /// </summary>
    /// <returns>A list of legal card plays.</returns>
    public IReadOnlyList<Card> GetLegalPlays()
    {
        if (Phase != GamePhase.TrickPlay || !NextToAct.HasValue)
        {
            return Array.Empty<Card>();
        }

        var hand = Hands[NextToAct.Value];

        return FollowSuitValidator.GetLegalPlays
        (
            hand,
            CurrentTrick?.LedSuit,
            TrumpSuit,
            Rules,
            CurrentTrick?.GetCurrentWinner()
        );
    }
}
