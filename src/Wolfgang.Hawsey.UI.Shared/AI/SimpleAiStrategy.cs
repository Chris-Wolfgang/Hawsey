using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Shared.AI;

/// <summary>
/// AI strategy: bids based on hand strength (length + jacks + aces),
/// avoids trumping its own partner, and uses played-card tracking to
/// prefer leading guaranteed winners.
/// </summary>
public class SimpleAiStrategy : IPlayerStrategy
{
    public BidAction DecideBid(GameState state, PlayerPosition player)
    {
        ArgumentNullException.ThrowIfNull(state);

        var score = BestSuitScore(state.Hands[player], out _);

        if (score < state.Rules.MinimumBid)
        {
            return BidAction.PassBid.Instance;
        }

        return new BidAction.NumberBid(Math.Min(score, 11));
    }



    public Suit? DecideTrump(GameState state, PlayerPosition player)
    {
        ArgumentNullException.ThrowIfNull(state);
        BestSuitScore(state.Hands[player], out var bestSuit);
        return bestSuit;
    }



    public Card DecidePlay(GameState state, PlayerPosition player)
    {
        ArgumentNullException.ThrowIfNull(state);

        var legalPlays = state.GetLegalPlays();
        if (legalPlays.Count == 1) return legalPlays[0];

        var hand = state.Hands[player];
        var played = BuildPlayedCounts(state);

        // Leading: prefer a guaranteed winner if one is available.
        if (state.CurrentTrick == null || state.CurrentTrick.Plays.Count == 0)
        {
            Card? sureWinner = null;
            for (var i = 0; i < legalPlays.Count; i++)
            {
                if (IsSureWinnerWhenLed(legalPlays[i], hand, played, state.TrumpSuit))
                {
                    if (!sureWinner.HasValue ||
                        CardRanking.GetEffectiveRank(legalPlays[i], state.TrumpSuit) >
                        CardRanking.GetEffectiveRank(sureWinner.Value, state.TrumpSuit))
                    {
                        sureWinner = legalPlays[i];
                    }
                }
            }
            if (sureWinner.HasValue) return sureWinner.Value;
            return GetHighestCard(legalPlays, state.TrumpSuit);
        }

        // Following: if partner is currently winning, dump the lowest card.
        var partner = player.Partner();
        if (CurrentWinningPlayer(state.CurrentTrick, state.TrumpSuit) == partner)
        {
            return GetLowestCard(legalPlays, state.TrumpSuit);
        }

        // Otherwise try to play the lowest card that beats the current winner.
        var currentWinner = state.CurrentTrick.GetCurrentWinner();
        if (currentWinner.HasValue && state.CurrentTrick.LedSuit.HasValue)
        {
            var comparer = new CardComparer(state.TrumpSuit, state.CurrentTrick.LedSuit.Value);
            Card? lowestWinner = null;

            for (var i = 0; i < legalPlays.Count; i++)
            {
                if (comparer.Compare(legalPlays[i], currentWinner.Value) > 0
                    && (!lowestWinner.HasValue ||
                        comparer.Compare(legalPlays[i], lowestWinner.Value) < 0))
                {
                    lowestWinner = legalPlays[i];
                }
            }

            if (lowestWinner.HasValue) return lowestWinner.Value;
        }

        return GetLowestCard(legalPlays, state.TrumpSuit);
    }



    public void DecideHawseyExchange(
        GameState state,
        PlayerPosition bidder,
        out Card[] cardsToDiscard,
        out Card[] cardsFromPartner)
    {
        ArgumentNullException.ThrowIfNull(state);

        var bidderHand = state.Hands[bidder];

        var sorted = bidderHand
            .OrderBy(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenBy(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsToDiscard = new[] { sorted[0], sorted[1] };

        var partner = bidder.Partner();
        var partnerHand = state.Hands[partner];

        var partnerSorted = partnerHand
            .OrderByDescending(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenByDescending(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsFromPartner = new[] { partnerSorted[0], partnerSorted[1] };
    }



    /// <summary>
    /// Estimated team-tricks for the best suit as trump:
    /// length-of-trump-suit + jacks-in-trump-suit + aces-outside-trump-suit, minus 1 for caution.
    /// </summary>
    private static int BestSuitScore(IReadOnlyList<Card> hand, out Suit bestSuit)
    {
        var lengthBySuit = new int[4];
        var jacksBySuit = new int[4];
        var acesBySuit = new int[4];
        var totalAces = 0;

        for (var i = 0; i < hand.Count; i++)
        {
            var s = (int)hand[i].Suit;
            lengthBySuit[s]++;
            if (hand[i].Rank == Rank.Jack) jacksBySuit[s]++;
            if (hand[i].Rank == Rank.Ace)
            {
                acesBySuit[s]++;
                totalAces++;
            }
        }

        bestSuit = Suit.Hearts;
        var bestScore = -1;

        for (var s = 0; s < 4; s++)
        {
            var score = lengthBySuit[s] + jacksBySuit[s] + (totalAces - acesBySuit[s]) - 1;
            if (score > bestScore)
            {
                bestScore = score;
                bestSuit = (Suit)s;
            }
        }

        return bestScore;
    }



    private static Dictionary<Card, int> BuildPlayedCounts(GameState state)
    {
        var counts = new Dictionary<Card, int>();

        for (var t = 0; t < state.CompletedTricks.Count; t++)
        {
            var trick = state.CompletedTricks[t];
            for (var p = 0; p < trick.Cards.Count; p++)
            {
                Add(counts, trick.Cards[p].Card);
            }
        }

        if (state.CurrentTrick is not null)
        {
            var plays = state.CurrentTrick.Plays;
            for (var p = 0; p < plays.Count; p++)
            {
                Add(counts, plays[p].Card);
            }
        }

        return counts;
    }



    private static void Add(Dictionary<Card, int> counts, Card card)
    {
        counts.TryGetValue(card, out var n);
        counts[card] = n + 1;
    }



    private static PlayerPosition? CurrentWinningPlayer(Trick trick, Suit? trumpSuit)
    {
        var plays = trick.Plays;
        if (plays.Count == 0 || trick.LedSuit is null) return null;

        var comparer = new CardComparer(trumpSuit, trick.LedSuit.Value);
        var winner = plays[0];
        for (var i = 1; i < plays.Count; i++)
        {
            if (comparer.Compare(plays[i].Card, winner.Card) > 0)
            {
                winner = plays[i];
            }
        }
        return winner.Player;
    }



    private static bool IsSureWinnerWhenLed
    (
        Card card,
        IReadOnlyList<Card> hand,
        Dictionary<Card, int> played,
        Suit? trumpSuit
    )
    {
        const int copiesPerCard = 2;
        const int trumpDeckSize = 12;

        for (var r = (int)card.Rank + 1; r <= (int)Rank.Ace; r++)
        {
            var higher = new Card((Rank)r, card.Suit);
            played.TryGetValue(higher, out var inPlayed);
            var inHand = CountIn(hand, higher);
            if (copiesPerCard - inPlayed - inHand > 0) return false;
        }

        if (trumpSuit is { } t && card.Suit != t)
        {
            var trumpsPlayed = 0;
            foreach (var kv in played)
            {
                if (kv.Key.Suit == t) trumpsPlayed += kv.Value;
            }
            var trumpsInHand = 0;
            for (var i = 0; i < hand.Count; i++)
            {
                if (hand[i].Suit == t) trumpsInHand++;
            }
            if (trumpDeckSize - trumpsPlayed - trumpsInHand > 0) return false;
        }

        return true;
    }



    private static int CountIn(IReadOnlyList<Card> hand, Card target)
    {
        var n = 0;
        for (var i = 0; i < hand.Count; i++)
        {
            if (hand[i] == target) n++;
        }
        return n;
    }



    private static Card GetHighestCard(IReadOnlyList<Card> cards, Suit? trumpSuit)
    {
        var highest = cards[0];
        for (var i = 1; i < cards.Count; i++)
        {
            if (CardRanking.GetEffectiveRank(cards[i], trumpSuit) >
                CardRanking.GetEffectiveRank(highest, trumpSuit))
            {
                highest = cards[i];
            }
        }
        return highest;
    }



    private static Card GetLowestCard(IReadOnlyList<Card> cards, Suit? trumpSuit)
    {
        var lowest = cards[0];
        for (var i = 1; i < cards.Count; i++)
        {
            var currentIsTrump = CardRanking.IsTrump(cards[i], trumpSuit);
            var lowestIsTrump = CardRanking.IsTrump(lowest, trumpSuit);

            var preferAsLowest =
                (!currentIsTrump && lowestIsTrump)
                || (currentIsTrump == lowestIsTrump
                    && CardRanking.GetEffectiveRank(cards[i], trumpSuit) <
                       CardRanking.GetEffectiveRank(lowest, trumpSuit));

            if (preferAsLowest)
            {
                lowest = cards[i];
            }
        }
        return lowest;
    }
}
