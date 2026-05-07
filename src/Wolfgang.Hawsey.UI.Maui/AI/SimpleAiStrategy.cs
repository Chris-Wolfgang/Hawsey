using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.AI;

/// <summary>
/// A basic AI player strategy for Hawsey.
/// </summary>
public class SimpleAiStrategy : IPlayerStrategy
{
    public BidAction DecideBid(GameState state, PlayerPosition player)
    {
        // Simple AI: always pass. The stuck dealer mechanic ensures the game progresses.
        // A more advanced AI would evaluate hand strength and track the current highest bid.
        return BidAction.PassBid.Instance;
    }



    public Suit? DecideTrump(GameState state, PlayerPosition player)
    {
        ArgumentNullException.ThrowIfNull(state);

        var hand = state.Hands[player];
        var suitCounts = new int[4];

        for (var i = 0; i < hand.Count; i++)
        {
            suitCounts[(int)hand[i].Suit]++;
        }

        // Pick the suit with the most cards
        var bestSuit = Suit.Hearts;
        var bestCount = 0;

        for (var s = 0; s < 4; s++)
        {
            if (suitCounts[s] > bestCount)
            {
                bestCount = suitCounts[s];
                bestSuit = (Suit)s;
            }
        }

        return bestSuit;
    }



    public Card DecidePlay(GameState state, PlayerPosition player)
    {
        ArgumentNullException.ThrowIfNull(state);

        var legalPlays = state.GetLegalPlays();

        if (legalPlays.Count == 1)
        {
            return legalPlays[0];
        }

        // If leading, play highest card
        if (state.CurrentTrick == null || state.CurrentTrick.Plays.Count == 0)
        {
            return GetHighestCard(legalPlays, state.TrumpSuit);
        }

        // If following, try to play lowest winning card
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

            if (lowestWinner.HasValue)
            {
                return lowestWinner.Value;
            }
        }

        // Can't win, play lowest card
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

        // Discard two lowest non-trump cards
        var sorted = bidderHand
            .OrderBy(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenBy(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsToDiscard = new[] { sorted[0], sorted[1] };

        // Give partner's two highest cards
        var partner = bidder.Partner();
        var partnerHand = state.Hands[partner];

        var partnerSorted = partnerHand
            .OrderByDescending(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenByDescending(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsFromPartner = new[] { partnerSorted[0], partnerSorted[1] };
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
            // Prefer non-trump low cards
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
