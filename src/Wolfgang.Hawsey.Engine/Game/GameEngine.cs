using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// The core game engine that manages state transitions for a Hawsey game.
/// Each method takes the current state and returns a new state.
/// </summary>
[SuppressMessage("Major Code Smell", "S2325:Methods and properties that don't access instance data should be static",
    Justification = "GameEngine is intentionally an instance class so callers can hold a single engine reference and so future extension (e.g. injected logger, metrics, timing) can be added without churning every call site. Every public method conceptually belongs to a long-lived engine instance even when the current implementation is stateless.")]
public sealed class GameEngine
{
    /// <summary>
    /// Starts a new game by dealing the first hand.
    /// </summary>
    /// <param name="rules">The house rules for this game.</param>
    /// <param name="firstDealer">The first dealer's position.</param>
    /// <param name="random">The random number generator for shuffling.</param>
    /// <returns>A new game state ready for bidding.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> or <paramref name="random"/> is <c>null</c>.</exception>
    public GameState StartGame(HouseRules rules, PlayerPosition firstDealer, Random random)
    {
        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var hands = Deal(firstDealer, random);

        return new GameState
        (
            phase: GamePhase.Bidding,
            dealer: firstDealer,
            hands: hands,
            trumpSuit: null,
            trumpMode: TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks: new List<TrickResult>(),
            currentTrick: null,
            northSouthScore: 0,
            eastWestScore: 0,
            rules: rules,
            nextToAct: firstDealer.NextClockwise(),
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );
    }



    /// <summary>
    /// Places a bid in the current bidding phase.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="player">The player placing the bid.</param>
    /// <param name="action">The bid action.</param>
    /// <param name="biddingPhase">The bidding phase tracker.</param>
    /// <returns>The updated game state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="biddingPhase"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the game is not in the bidding phase or it is not the specified player's turn.</exception>
    public GameState PlaceBid
    (
        GameState state,
        PlayerPosition player,
        BidAction action,
        BiddingPhase biddingPhase
    )
    {
        ValidatePhase(state, GamePhase.Bidding);
        ValidatePlayer(state, player);

        if (biddingPhase == null)
        {
            throw new ArgumentNullException(nameof(biddingPhase));
        }

        biddingPhase.PlaceBid(player, action);

        if (biddingPhase.IsComplete)
        {
            var result = biddingPhase.GetResult();

            return new GameState
            (
                phase: GamePhase.TrumpSelection,
                dealer: state.Dealer,
                hands: state.Hands,
                trumpSuit: null,
                trumpMode: TrumpMode.AceHigh,
                biddingResult: result,
                completedTricks: state.CompletedTricks,
                currentTrick: null,
                northSouthScore: state.NorthSouthScore,
                eastWestScore: state.EastWestScore,
                rules: state.Rules,
                nextToAct: result.Winner,
                tricksPlayedInRound: 0,
                isHawseyRound: result.IsHawsey,
                hawseyBidder: result.IsHawsey ? result.Winner : (PlayerPosition?)null
            );
        }

        return new GameState
        (
            phase: GamePhase.Bidding,
            dealer: state.Dealer,
            hands: state.Hands,
            trumpSuit: null,
            trumpMode: TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks: state.CompletedTricks,
            currentTrick: null,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: biddingPhase.GetNextBidder(),
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );
    }



    /// <summary>
    /// Selects the trump suit or declares Ace high.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high.</param>
    /// <returns>The updated game state.</returns>
    public GameState SelectTrump(GameState state, Suit? trumpSuit)
    {
        ValidatePhase(state, GamePhase.TrumpSelection);

        var trumpMode = trumpSuit.HasValue ? TrumpMode.Suited : TrumpMode.AceHigh;

        if (state.IsHawseyRound)
        {
            return new GameState
            (
                phase: GamePhase.HawseyExchange,
                dealer: state.Dealer,
                hands: state.Hands,
                trumpSuit: trumpSuit,
                trumpMode: trumpMode,
                biddingResult: state.BiddingResult,
                completedTricks: state.CompletedTricks,
                currentTrick: null,
                northSouthScore: state.NorthSouthScore,
                eastWestScore: state.EastWestScore,
                rules: state.Rules,
                nextToAct: state.BiddingResult!.Winner,
                tricksPlayedInRound: 0,
                isHawseyRound: true,
                hawseyBidder: state.HawseyBidder
            );
        }

        var expectedPlays = 4;
        var trick = new Trick(trumpSuit, expectedPlays);

        return new GameState
        (
            phase: GamePhase.TrickPlay,
            dealer: state.Dealer,
            hands: state.Hands,
            trumpSuit: trumpSuit,
            trumpMode: trumpMode,
            biddingResult: state.BiddingResult,
            completedTricks: new List<TrickResult>(),
            currentTrick: trick,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: state.BiddingResult!.Winner,
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );
    }



    /// <summary>
    /// Performs the Hawsey card exchange. The bidder discards 2 cards and
    /// receives 2 cards from their partner.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="cardsToDiscard">The 2 cards the bidder discards.</param>
    /// <param name="cardsFromPartner">The 2 cards the partner gives.</param>
    /// <returns>The updated game state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cardsToDiscard"/> or <paramref name="cardsFromPartner"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the game is not in the Hawsey exchange phase, the wrong number of cards is provided, or a card is not in the expected hand.</exception>
    public GameState ExchangeHawseyCards
    (
        GameState state,
        Card[] cardsToDiscard,
        Card[] cardsFromPartner
    )
    {
        ValidatePhase(state, GamePhase.HawseyExchange);
        ValidateHawseyExchangeCards(cardsToDiscard, cardsFromPartner);

        var bidder = state.HawseyBidder!.Value;
        var hands = PerformHawseyExchange(state, bidder, cardsToDiscard, cardsFromPartner);
        var trick = new Trick(state.TrumpSuit, expectedPlays: 3);

        return new GameState
        (
            phase: GamePhase.TrickPlay,
            dealer: state.Dealer,
            hands: hands,
            trumpSuit: state.TrumpSuit,
            trumpMode: state.TrumpMode,
            biddingResult: state.BiddingResult,
            completedTricks: new List<TrickResult>(),
            currentTrick: trick,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: bidder,
            tricksPlayedInRound: 0,
            isHawseyRound: true,
            hawseyBidder: state.HawseyBidder
        );
    }



    /// <summary>
    /// Plays a card in the current trick.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="player">The player playing the card.</param>
    /// <param name="card">The card to play.</param>
    /// <returns>The updated game state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the game is not in the trick play phase, it is not the specified player's turn, or the card is not a legal play.</exception>
    public GameState PlayCard(GameState state, PlayerPosition player, Card card)
    {
        ValidatePhase(state, GamePhase.TrickPlay);
        ValidatePlayer(state, player);
        ValidateCardIsLegal(state, card);

        var hands = RemoveCardFromHand(state.Hands, player, card);
        var trick = state.CurrentTrick!;
        trick.Play(player, card);

        if (!trick.IsComplete)
        {
            return CreateTrickInProgressState(state, hands, trick, GetNextPlayer(player, state));
        }

        return CompleteTrick(state, hands, trick);
    }



    /// <summary>
    /// Starts a new round after scoring, advancing the dealer.
    /// </summary>
    /// <param name="state">The current game state (must be in RoundScoring phase).</param>
    /// <param name="random">The random number generator for shuffling.</param>
    /// <returns>The updated game state ready for bidding.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the game is not in the round scoring phase.</exception>
    public GameState StartNextRound(GameState state, Random random)
    {
        ValidatePhase(state, GamePhase.RoundScoring);

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var nextDealer = state.Dealer.NextClockwise();
        var hands = Deal(nextDealer, random);

        return new GameState
        (
            phase: GamePhase.Bidding,
            dealer: nextDealer,
            hands: hands,
            trumpSuit: null,
            trumpMode: TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks: new List<TrickResult>(),
            currentTrick: null,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: nextDealer.NextClockwise(),
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );
    }



    private GameState ScoreRound
    (
        GameState state,
        IDictionary<PlayerPosition, List<Card>> hands,
        List<TrickResult> completedTricks,
        int tricksPlayed
    )
    {
        var biddingResult = state.BiddingResult!;
        var biddingTeam = biddingResult.Winner.GetTeam();
        var roundScore = CreateRoundScore(completedTricks, biddingTeam, biddingResult.BidAmount, state.IsHawseyRound);
        var (nsScore, ewScore) = CalculateNewScores(state, biddingTeam, roundScore);
        var phase = DeterminePostRoundPhase(nsScore, ewScore, state.Rules.PointsToWin);

        return new GameState
        (
            phase: phase,
            dealer: state.Dealer,
            hands: hands,
            trumpSuit: state.TrumpSuit,
            trumpMode: state.TrumpMode,
            biddingResult: state.BiddingResult,
            completedTricks: completedTricks,
            currentTrick: null,
            northSouthScore: nsScore,
            eastWestScore: ewScore,
            rules: state.Rules,
            nextToAct: null,
            tricksPlayedInRound: tricksPlayed,
            isHawseyRound: state.IsHawseyRound,
            hawseyBidder: state.HawseyBidder
        );
    }



    private static void ValidateHawseyExchangeCards(Card[] cardsToDiscard, Card[] cardsFromPartner)
    {
        if (cardsToDiscard == null)
        {
            throw new ArgumentNullException(nameof(cardsToDiscard));
        }

        if (cardsFromPartner == null)
        {
            throw new ArgumentNullException(nameof(cardsFromPartner));
        }

        if (cardsToDiscard.Length != 2)
        {
            throw new InvalidOperationException("Must discard exactly 2 cards.");
        }

        if (cardsFromPartner.Length != 2)
        {
            throw new InvalidOperationException("Partner must give exactly 2 cards.");
        }
    }



    private static Dictionary<PlayerPosition, List<Card>> PerformHawseyExchange
    (
        GameState state,
        PlayerPosition bidder,
        Card[] cardsToDiscard,
        Card[] cardsFromPartner
    )
    {
        var partner = bidder.Partner();
        var bidderHand = new List<Card>(state.Hands[bidder]);
        var partnerHand = new List<Card>(state.Hands[partner]);

        RemoveCardsFromHand(bidderHand, cardsToDiscard, "bidder's");
        RemoveCardsFromHand(partnerHand, cardsFromPartner, "partner's");

        bidderHand.AddRange(cardsFromPartner);

        return new Dictionary<PlayerPosition, List<Card>>(state.Hands)
        {
            [bidder] = bidderHand,
            [partner] = partnerHand
        };
    }



    private static void RemoveCardsFromHand(List<Card> hand, Card[] cards, string handOwner)
    {
        foreach (var card in cards)
        {
            var index = hand.IndexOf(card);

            if (index < 0)
            {
                throw new InvalidOperationException($"Card {card} is not in the {handOwner} hand.");
            }

            hand.RemoveAt(index);
        }
    }



    private static void ValidateCardIsLegal(GameState state, Card card)
    {
        var legalPlays = state.GetLegalPlays();

        if (!ContainsCard(legalPlays, card))
        {
            throw new InvalidOperationException($"Card {card} is not a legal play.");
        }
    }



    private static Dictionary<PlayerPosition, List<Card>> RemoveCardFromHand
    (
        IDictionary<PlayerPosition, List<Card>> hands,
        PlayerPosition player,
        Card card
    )
    {
        var result = new Dictionary<PlayerPosition, List<Card>>(hands);
        var hand = new List<Card>(result[player]);
        hand.Remove(card);
        result[player] = hand;

        return result;
    }



    private static GameState CreateTrickInProgressState
    (
        GameState state,
        IDictionary<PlayerPosition, List<Card>> hands,
        Trick trick,
        PlayerPosition nextToAct
    )
    {
        return new GameState
        (
            phase: GamePhase.TrickPlay,
            dealer: state.Dealer,
            hands: hands,
            trumpSuit: state.TrumpSuit,
            trumpMode: state.TrumpMode,
            biddingResult: state.BiddingResult,
            completedTricks: state.CompletedTricks,
            currentTrick: trick,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: nextToAct,
            tricksPlayedInRound: state.TricksPlayedInRound,
            isHawseyRound: state.IsHawseyRound,
            hawseyBidder: state.HawseyBidder
        );
    }



    private GameState CompleteTrick
    (
        GameState state,
        IDictionary<PlayerPosition, List<Card>> hands,
        Trick trick
    )
    {
        var trickResult = trick.GetResult();
        var completedTricks = new List<TrickResult>(state.CompletedTricks) { trickResult };
        var tricksPlayed = state.TricksPlayedInRound + 1;

        if (tricksPlayed >= 12)
        {
            return ScoreRound(state, hands, completedTricks, tricksPlayed);
        }

        var expectedPlays = state.IsHawseyRound ? 3 : 4;
        var nextTrick = new Trick(state.TrumpSuit, expectedPlays);

        return new GameState
        (
            phase: GamePhase.TrickPlay,
            dealer: state.Dealer,
            hands: hands,
            trumpSuit: state.TrumpSuit,
            trumpMode: state.TrumpMode,
            biddingResult: state.BiddingResult,
            completedTricks: completedTricks,
            currentTrick: nextTrick,
            northSouthScore: state.NorthSouthScore,
            eastWestScore: state.EastWestScore,
            rules: state.Rules,
            nextToAct: trickResult.Winner,
            tricksPlayedInRound: tricksPlayed,
            isHawseyRound: state.IsHawseyRound,
            hawseyBidder: state.HawseyBidder
        );
    }



    private static RoundScore CreateRoundScore
    (
        List<TrickResult> completedTricks,
        Team biddingTeam,
        int bidAmount,
        bool isHawseyRound
    )
    {
        var biddingTeamTricks = 0;
        var defendingTeamTricks = 0;

        foreach (var trick in completedTricks)
        {
            if (trick.Winner.GetTeam() == biddingTeam)
            {
                biddingTeamTricks++;
            }
            else
            {
                defendingTeamTricks++;
            }
        }

        return new RoundScore
        (
            biddingTeam,
            bidAmount,
            biddingTeamTricks,
            defendingTeamTricks,
            isHawsey: isHawseyRound
        );
    }



    private static (int NorthSouthScore, int EastWestScore) CalculateNewScores
    (
        GameState state,
        Team biddingTeam,
        RoundScore roundScore
    )
    {
        var nsScore = state.NorthSouthScore;
        var ewScore = state.EastWestScore;

        if (biddingTeam == Team.NorthSouth)
        {
            nsScore += roundScore.BiddingTeamDelta;
            ewScore += roundScore.DefendingTeamDelta;
        }
        else
        {
            ewScore += roundScore.BiddingTeamDelta;
            nsScore += roundScore.DefendingTeamDelta;
        }

        return (nsScore, ewScore);
    }



    private static GamePhase DeterminePostRoundPhase(int nsScore, int ewScore, int pointsToWin)
    {
        var nsReached = nsScore >= pointsToWin;
        var ewReached = ewScore >= pointsToWin;

        if (nsReached || ewReached)
        {
            return GamePhase.GameOver;
        }

        return GamePhase.RoundScoring;
    }



    private static Dictionary<PlayerPosition, List<Card>> Deal(PlayerPosition dealer, Random random)
    {
        var deck = Deck.Shuffle(Deck.CreatePinochleDeck(), random);

        var hands = new Dictionary<PlayerPosition, List<Card>>
        {
            [PlayerPosition.North] = new List<Card>(12),
            [PlayerPosition.East] = new List<Card>(12),
            [PlayerPosition.South] = new List<Card>(12),
            [PlayerPosition.West] = new List<Card>(12)
        };

        // Deal 2 at a time, starting left of dealer, going clockwise
        var positions = new PlayerPosition[4];
        var pos = dealer.NextClockwise();

        for (var i = 0; i < 4; i++)
        {
            positions[i] = pos;
            pos = pos.NextClockwise();
        }

        var cardIndex = 0;

        // 6 rounds of dealing 2 cards to each player
        for (var round = 0; round < 6; round++)
        {
            for (var p = 0; p < 4; p++)
            {
                hands[positions[p]].Add(deck[cardIndex++]);
                hands[positions[p]].Add(deck[cardIndex++]);
            }
        }

        return hands;
    }



    private static PlayerPosition GetNextPlayer(PlayerPosition current, GameState state)
    {
        if (state.IsHawseyRound)
        {
            // In Hawsey, the partner sits out
            var bidder = state.HawseyBidder!.Value;
            var partner = bidder.Partner();
            var next = current.NextClockwise();

            if (next == partner)
            {
                next = next.NextClockwise();
            }

            return next;
        }

        return current.NextClockwise();
    }



    private static bool ContainsCard(IReadOnlyList<Card> cards, Card target)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            if (cards[i].Equals(target))
            {
                return true;
            }
        }

        return false;
    }



    private static void ValidatePhase(GameState state, GamePhase expected)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (state.Phase != expected)
        {
            throw new InvalidOperationException
            (
                $"Expected game phase {expected}, but current phase is {state.Phase}."
            );
        }
    }



    private static void ValidatePlayer(GameState state, PlayerPosition player)
    {
        if (state.NextToAct != player)
        {
            throw new InvalidOperationException
            (
                $"It is not {player}'s turn. Expected {state.NextToAct}."
            );
        }
    }
}
