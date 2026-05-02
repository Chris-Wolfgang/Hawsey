using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

/// <summary>
/// A deterministic player strategy for testing. Always passes during bidding
/// (unless configured otherwise), picks the first available trump suit,
/// and plays the first legal card.
/// </summary>
public sealed class TestPlayerStrategy : IPlayerStrategy
{
    private readonly Queue<BidAction>? _bidQueue;
    private readonly Suit? _preferredTrump;
    private readonly bool _aceHigh;



    public TestPlayerStrategy(
        Queue<BidAction>? bidQueue = null,
        Suit? preferredTrump = null,
        bool aceHigh = false)
    {
        _bidQueue = bidQueue;
        _preferredTrump = preferredTrump;
        _aceHigh = aceHigh;
    }



    public BidAction DecideBid(GameState state, PlayerPosition player)
    {
        if (_bidQueue != null && _bidQueue.Count > 0)
        {
            return _bidQueue.Dequeue();
        }

        return BidAction.PassBid.Instance;
    }



    public Suit? DecideTrump(GameState state, PlayerPosition player)
    {
        if (_aceHigh)
        {
            return null;
        }

        return _preferredTrump ?? Suit.Hearts;
    }



    public Card DecidePlay(GameState state, PlayerPosition player)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var legalPlays = state.GetLegalPlays();
        return legalPlays[0];
    }



    public void DecideHawseyExchange(
        GameState state,
        PlayerPosition bidder,
        out Card[] cardsToDiscard,
        out Card[] cardsFromPartner)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var bidderHand = state.Hands[bidder];
        cardsToDiscard = new[] { bidderHand[0], bidderHand[1] };

        var partner = bidder.Partner();
        var partnerHand = state.Hands[partner];
        cardsFromPartner = new[] { partnerHand[0], partnerHand[1] };
    }
}
