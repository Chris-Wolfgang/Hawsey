using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Strategy;

/// <summary>
/// The AI player every Hawsey UI uses for its computer seats (through
/// <see cref="Game.GameSession"/>). It bids on hand strength (trump length, jacks in
/// trump and aces outside it), names the suit that scores best, leads a card nothing
/// left can beat when it has one, lets its partner's winning card stand, and otherwise
/// wins as cheaply as it can or throws its lowest card.
/// </summary>
public class SimpleAiStrategy : IPlayerStrategy
{
    private const int HighestAiBid = 11;

    private static readonly Suit[] Suits = [Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades];

    private static readonly Rank[] Ranks = [Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace];



    /// <inheritdoc/>
    /// <remarks>
    /// Passes when the hand is worth less than the minimum bid, or when its bid
    /// (at most 11) wouldn't beat <see cref="GameState.HighBid"/>. As the dealer with
    /// every other player passed, it bids just the minimum: there is nothing to outbid.
    /// </remarks>
    public BidAction DecideBid(GameState state, PlayerPosition player)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var score = BestSuitScore(state.Hands[player], out _);

        if (score < state.Rules.MinimumBid)
        {
            return BidAction.PassBid.Instance;
        }

        // The dealer bids last, so a dealer facing no bid is the stuck dealer.
        if (player == state.Dealer && state.HighBid == 0)
        {
            return new BidAction.NumberBid(state.Rules.MinimumBid);
        }

        var bid = Math.Min(score, HighestAiBid);

        return bid > state.HighBid
            ? new BidAction.NumberBid(bid)
            : BidAction.PassBid.Instance;
    }



    /// <inheritdoc/>
    public Suit? DecideTrump(GameState state, PlayerPosition player)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        BestSuitScore(state.Hands[player], out var bestSuit);

        return bestSuit;
    }



    /// <inheritdoc/>
    public Card DecidePlay(GameState state, PlayerPosition player)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var legalPlays = state.GetLegalPlays();

        if (legalPlays.Count == 1)
        {
            return legalPlays[0];
        }

        // Leading: prefer a card nothing left in play can beat.
        if (state.CurrentTrick == null || state.CurrentTrick.Plays.Count == 0)
        {
            return ChooseLead(state, state.Hands[player], legalPlays);
        }

        // Following. The trick has a card, so it has a led suit and a winner.
        var comparer = new CardComparer(state.TrumpSuit, state.CurrentTrick.LedSuit!.Value);
        var winning = WinningPlay(state.CurrentTrick, comparer);

        // If the partner is winning, don't overtake: throw low. Otherwise win as
        // cheaply as possible, or throw low.
        return winning.Player == player.Partner()
            ? GetLowestCard(legalPlays, state.TrumpSuit)
            : LowestCardThatBeats(winning.Card, comparer, legalPlays) ?? GetLowestCard(legalPlays, state.TrumpSuit);
    }



    private static Card ChooseLead(GameState state, IReadOnlyList<Card> hand, IReadOnlyList<Card> legalPlays)
    {
        var played = CountPlayedCards(state);
        Card? sureWinner = null;

        for (var i = 0; i < legalPlays.Count; i++)
        {
            if (IsSureWinnerWhenLed(legalPlays[i], hand, played, state.TrumpSuit)
                && (!sureWinner.HasValue
                    || CardRanking.GetEffectiveRank(legalPlays[i], state.TrumpSuit) > CardRanking.GetEffectiveRank(sureWinner.Value, state.TrumpSuit)))
            {
                sureWinner = legalPlays[i];
            }
        }

        return sureWinner ?? GetHighestCard(legalPlays, state.TrumpSuit);
    }



    private static Card? LowestCardThatBeats(Card winner, CardComparer comparer, IReadOnlyList<Card> legalPlays)
    {
        Card? lowestWinner = null;

        for (var i = 0; i < legalPlays.Count; i++)
        {
            if (comparer.Compare(legalPlays[i], winner) > 0
                && (!lowestWinner.HasValue || comparer.Compare(legalPlays[i], lowestWinner.Value) < 0))
            {
                lowestWinner = legalPlays[i];
            }
        }

        return lowestWinner;
    }



    /// <inheritdoc/>
    public void DecideHawseyExchange
    (
        GameState state,
        PlayerPosition bidder,
        out Card[] cardsToDiscard,
        out Card[] cardsFromPartner
    )
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        // Discard the two lowest side cards.
        var sorted = state
            .Hands[bidder]
            .OrderBy(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenBy(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsToDiscard = [sorted[0], sorted[1]];

        // Take the partner's two best cards: trumps first, highest first.
        var partnerSorted = state
            .Hands[bidder.Partner()]
            .OrderByDescending(c => CardRanking.IsTrump(c, state.TrumpSuit) ? 1 : 0)
            .ThenByDescending(c => CardRanking.GetEffectiveRank(c, state.TrumpSuit))
            .ToList();

        cardsFromPartner = [partnerSorted[0], partnerSorted[1]];
    }



    /// <summary>
    /// The estimated tricks the team takes with the best suit as trump: that suit's
    /// length, plus its jacks, plus the aces in the other suits, minus one for caution.
    /// </summary>
    private static int BestSuitScore(IReadOnlyList<Card> hand, out Suit bestSuit)
    {
        var lengthBySuit = new int[4];
        var jacksBySuit = new int[4];
        var acesBySuit = new int[4];
        var totalAces = 0;

        for (var i = 0; i < hand.Count; i++)
        {
            var suit = (int)hand[i].Suit;
            lengthBySuit[suit]++;

            if (hand[i].Rank == Rank.Jack)
            {
                jacksBySuit[suit]++;
            }

            if (hand[i].Rank == Rank.Ace)
            {
                acesBySuit[suit]++;
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



    /// <summary>
    /// How many copies of each card have been played this round. Called when leading,
    /// so the current trick is empty and only the finished tricks count.
    /// </summary>
    private static Dictionary<Card, int> CountPlayedCards(GameState state)
    {
        var counts = new Dictionary<Card, int>();

        for (var t = 0; t < state.CompletedTricks.Count; t++)
        {
            var cards = state.CompletedTricks[t].Cards;

            for (var p = 0; p < cards.Count; p++)
            {
                AddOne(counts, cards[p].Card);
            }
        }

        return counts;
    }



    private static void AddOne(Dictionary<Card, int> counts, Card card)
    {
        counts.TryGetValue(card, out var count);
        counts[card] = count + 1;
    }



    /// <summary>
    /// The play currently winning <paramref name="trick"/>, which must hold at least one
    /// card. The first of two equal cards keeps the lead.
    /// </summary>
    private static PlayedCard WinningPlay(Trick trick, CardComparer comparer)
    {
        var winner = trick.Plays[0];

        for (var i = 1; i < trick.Plays.Count; i++)
        {
            if (comparer.Compare(trick.Plays[i].Card, winner.Card) > 0)
            {
                winner = trick.Plays[i];
            }
        }

        return winner;
    }



    /// <summary>
    /// Whether <paramref name="card"/>, led now, must win the trick: no copy of a higher
    /// card in its effective suit is still out (bowers included), and, for a side card,
    /// no trump is still out to ruff it.
    /// </summary>
    private static bool IsSureWinnerWhenLed(Card card, IReadOnlyList<Card> hand, Dictionary<Card, int> played, Suit? trumpSuit)
    {
        const int CopiesPerCard = 2;

        var rank = CardRanking.GetEffectiveRank(card, trumpSuit);
        var suit = CardRanking.GetEffectiveSuit(card, trumpSuit);

        foreach (var candidateSuit in Suits)
        {
            foreach (var candidateRank in Ranks)
            {
                var candidate = new Card(candidateRank, candidateSuit);

                if (CardRanking.GetEffectiveSuit(candidate, trumpSuit) != suit
                    || CardRanking.GetEffectiveRank(candidate, trumpSuit) <= rank)
                {
                    continue;
                }

                played.TryGetValue(candidate, out var playedCopies);

                if (CopiesPerCard - playedCopies - CountInHand(hand, candidate) > 0)
                {
                    return false;
                }
            }
        }

        if (trumpSuit is { } trump && suit != trump)
        {
            // 14 with a suit named: its 12 cards plus the two left bowers.
            var trumpsInDeck = CopiesPerCard * Suits
                .SelectMany(s => Ranks.Select(r => new Card(r, s)))
                .Count(c => CardRanking.GetEffectiveSuit(c, trumpSuit) == trump);
            var trumpsGone = played
                .Where(kv => CardRanking.GetEffectiveSuit(kv.Key, trumpSuit) == trump)
                .Sum(kv => kv.Value);
            var trumpsHeld = hand.Count(c => CardRanking.GetEffectiveSuit(c, trumpSuit) == trump);

            if (trumpsInDeck - trumpsGone - trumpsHeld > 0)
            {
                return false;
            }
        }

        return true;
    }



    private static int CountInHand(IReadOnlyList<Card> hand, Card target)
    {
        var count = 0;

        for (var i = 0; i < hand.Count; i++)
        {
            if (hand[i] == target)
            {
                count++;
            }
        }

        return count;
    }



    private static Card GetHighestCard(IReadOnlyList<Card> cards, Suit? trumpSuit)
    {
        var highest = cards[0];

        for (var i = 1; i < cards.Count; i++)
        {
            if (CardRanking.GetEffectiveRank(cards[i], trumpSuit) > CardRanking.GetEffectiveRank(highest, trumpSuit))
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
            // Prefer throwing a side card over a trump, then the lower rank.
            var currentIsTrump = CardRanking.IsTrump(cards[i], trumpSuit);
            var lowestIsTrump = CardRanking.IsTrump(lowest, trumpSuit);

            var preferAsLowest =
                (!currentIsTrump && lowestIsTrump)
                || (currentIsTrump == lowestIsTrump
                    && CardRanking.GetEffectiveRank(cards[i], trumpSuit) < CardRanking.GetEffectiveRank(lowest, trumpSuit));

            if (preferAsLowest)
            {
                lowest = cards[i];
            }
        }

        return lowest;
    }
}
