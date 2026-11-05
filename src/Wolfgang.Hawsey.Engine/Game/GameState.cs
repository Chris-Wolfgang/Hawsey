using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Scoring;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Game;

/// <summary>
/// A snapshot of the entire game state at a point in time. Each method on
/// <see cref="GameEngine"/> returns a freshly-constructed instance rather than mutating
/// the previous state, so callers should treat the exposed <see cref="Hands"/> and
/// <see cref="CompletedTricks"/> collections as read-only — mutating them is unsupported
/// and can corrupt subsequent engine transitions.
/// </summary>
// MA0016: GameState exposes Dictionary<,> and List<> on its public surface so the engine
// can pass the collections it constructs internally without copying or wrapping. Switching
// to abstractions would require lossy conversions because IReadOnlyDictionary<TKey, TValue>
// is invariant in TValue (Dictionary<P, List<Card>> is not assignable to
// IReadOnlyDictionary<P, IReadOnlyList<Card>>). Callers must treat these collections as
// read-only per the type's XML doc.
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design", "MA0016:Prefer using collection abstraction instead of implementation",
    Justification = "Engine returns concrete types; callers must treat collections as read-only per the type's XML doc.")]
public sealed class GameState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameState"/> class. Each parameter
    /// sets the property of the same name.
    /// </summary>
    /// <param name="phase">The current game phase (<see cref="Phase"/>).</param>
    /// <param name="dealer">The current dealer's position (<see cref="Dealer"/>).</param>
    /// <param name="hands">The hands for each player (<see cref="Hands"/>).</param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode (<see cref="TrumpSuit"/>).</param>
    /// <param name="trumpMode">The trump mode (<see cref="TrumpMode"/>).</param>
    /// <param name="biddingResult">The bidding result, or <c>null</c> if bidding has not completed (<see cref="BiddingResult"/>).</param>
    /// <param name="completedTricks">The tricks completed in the current round (<see cref="CompletedTricks"/>).</param>
    /// <param name="currentTrick">The current trick in progress, or <c>null</c> (<see cref="CurrentTrick"/>).</param>
    /// <param name="northSouthScore">The current North/South score (<see cref="NorthSouthScore"/>).</param>
    /// <param name="eastWestScore">The current East/West score (<see cref="EastWestScore"/>).</param>
    /// <param name="rules">The house rules in effect (<see cref="Rules"/>).</param>
    /// <param name="nextToAct">The player who should act next, or <c>null</c> if no action is needed (<see cref="NextToAct"/>).</param>
    /// <param name="tricksPlayedInRound">The number of tricks played in the current round (<see cref="TricksPlayedInRound"/>).</param>
    /// <param name="isHawseyRound">Whether this is a Hawsey round (<see cref="IsHawseyRound"/>).</param>
    /// <param name="hawseyBidder">The Hawsey bidder's position, or <c>null</c> if not a Hawsey round (<see cref="HawseyBidder"/>).</param>
    /// <param name="highBid">The highest number bid so far while bidding, or <c>0</c> when nobody has bid (<see cref="HighBid"/>).</param>
    /// <param name="highBidder">The player holding <paramref name="highBid"/>, or <c>null</c> when nobody has bid (<see cref="HighBidder"/>).</param>
    /// <param name="roundScore">The round the engine has just scored, or <c>null</c> (<see cref="RoundScore"/>).</param>
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
        PlayerPosition? hawseyBidder,
        int highBid = 0,
        PlayerPosition? highBidder = null,
        RoundScore? roundScore = null
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
        HighBid = highBid;
        HighBidder = highBidder;
        RoundScore = roundScore;
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
    /// Gets the team that won, once <see cref="Phase"/> is <see cref="GamePhase.GameOver"/>;
    /// <c>null</c> while the game continues. A team wins by reaching
    /// <see cref="HouseRules.PointsToWin"/>. When both teams reach it in the same round,
    /// the team that held the bid in that round wins.
    /// </summary>
    public Team? Winner
    {
        get
        {
            if (Phase != GamePhase.GameOver)
            {
                return null;
            }

            var northSouthReached = NorthSouthScore >= Rules.PointsToWin;
            var eastWestReached = EastWestScore >= Rules.PointsToWin;

            if (northSouthReached && eastWestReached)
            {
                return BiddingResult?.Winner.GetTeam();
            }

            if (northSouthReached)
            {
                return Team.NorthSouth;
            }

            return eastWestReached ? Team.EastWest : null;
        }
    }



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
    /// Gets the highest number bid so far while <see cref="Phase"/> is
    /// <see cref="GamePhase.Bidding"/>, or <c>0</c> when nobody has bid yet. A bid must
    /// beat it. Once bidding ends, <see cref="BiddingResult"/> holds the winning bid.
    /// </summary>
    public int HighBid { get; }



    /// <summary>
    /// Gets the player holding <see cref="HighBid"/> while bidding, or <c>null</c> when
    /// nobody has bid yet.
    /// </summary>
    public PlayerPosition? HighBidder { get; }



    /// <summary>
    /// The highest number bid: all twelve tricks.
    /// </summary>
    public const int MaximumBid = 12;



    /// <summary>
    /// The number of tricks in a round: each player's twelve cards.
    /// </summary>
    public const int TricksPerRound = 12;



    /// <summary>
    /// The points a Hawsey bid is worth: the bidding team gains them by taking all
    /// <see cref="TricksPerRound"/> tricks, and loses them otherwise.
    /// </summary>
    public const int HawseyBidAmount = 24;



    /// <summary>
    /// Gets the lowest number bid the next bidder may make: the house minimum, or one
    /// more than <see cref="HighBid"/>.
    /// </summary>
    public int MinimumLegalBid => Math.Max(Rules.MinimumBid, HighBid + 1);



    /// <summary>
    /// Gets a value indicating whether the next bidder is the stuck dealer: bidding is
    /// open, the dealer is next (the dealer bids last), and nobody has bid.
    /// </summary>
    public bool IsNextBidderStuck => Phase == GamePhase.Bidding && NextToAct == Dealer && HighBid == 0;



    /// <summary>
    /// Gets the round the engine has just scored: set once the last trick of a round is
    /// played (in the <see cref="GamePhase.RoundScoring"/> and <see cref="GamePhase.GameOver"/>
    /// phases), <see langword="null"/> otherwise. A UI shows the round's result from it
    /// instead of recounting the tricks.
    /// </summary>
    public RoundScore? RoundScore { get; }



    /// <summary>
    /// Gets the cards to show on the table, in the order they were played: the current
    /// trick's cards, or, from the moment a trick is won until the next card is led, the
    /// trick just won (including the last trick of a round). Empty before the first lead
    /// of a round.
    /// </summary>
    public IReadOnlyList<PlayedCard> TableCards
    {
        get
        {
            if (CurrentTrick is { Plays.Count: > 0 } current)
            {
                return current.Plays;
            }

            return CompletedTricks.Count > 0
                ? CompletedTricks[CompletedTricks.Count - 1].Cards
                : Array.Empty<PlayedCard>();
        }
    }



    /// <summary>
    /// Gets the card winning the trick on the table (<see cref="TableCards"/>): the
    /// current trick's winning card so far, or the card that won the trick just
    /// completed. <see langword="null"/> when the table is empty.
    /// </summary>
    public Card? TableWinningCard
    {
        get
        {
            if (CurrentTrick is { Plays.Count: > 0 } current)
            {
                return current.GetCurrentWinner();
            }

            if (CompletedTricks.Count == 0)
            {
                return null;
            }

            var won = CompletedTricks[CompletedTricks.Count - 1];

            // The winner played exactly one card in the trick.
            var i = 0;

            while (won.Cards[i].Player != won.Winner)
            {
                i++;
            }

            return won.Cards[i].Card;
        }
    }



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
