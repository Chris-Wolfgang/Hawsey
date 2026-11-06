using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.UI.Shared.Tests.Unit.Pages;

/// <summary>
/// <see cref="SimpleAiStrategy"/> with the bidding scripted, so a test decides who wins
/// the bid. Every other decision is the real AI's. The default script passes.
/// </summary>
internal sealed class ScriptedAi : IPlayerStrategy
{
    private readonly SimpleAiStrategy _ai = new();
    private readonly Func<GameState, PlayerPosition, BidAction> _bid;



    public ScriptedAi(Func<GameState, PlayerPosition, BidAction>? bid = null)
    {
        _bid = bid ?? ((_, _) => BidAction.PassBid.Instance);
    }



    public BidAction DecideBid(GameState state, PlayerPosition player) => _bid(state, player);



    public Suit? DecideTrump(GameState state, PlayerPosition player) => _ai.DecideTrump(state, player);



    public Card DecidePlay(GameState state, PlayerPosition player) => _ai.DecidePlay(state, player);



    public void DecideHawseyExchange(GameState state, PlayerPosition bidder, out Card[] cardsToDiscard, out Card[] cardsFromPartner) =>
        _ai.DecideHawseyExchange(state, bidder, out cardsToDiscard, out cardsFromPartner);
}
