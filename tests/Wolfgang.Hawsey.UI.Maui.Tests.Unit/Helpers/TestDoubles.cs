using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.UI.Maui.Threading;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;

/// <summary>
/// Runs dispatched work at once, on the calling thread.
/// </summary>
internal sealed class ImmediateDispatcher : IUiDispatcher
{
    public int PostCount { get; private set; }



    public void Post(Action action)
    {
        PostCount++;
        action();
    }
}



/// <summary>
/// A <see cref="GameSession"/> with no pauses between AI moves and a seeded deal, so
/// a command runs the game forward synchronously and repeatably. The AI seats default
/// to <see cref="PassingAi"/>, so the human's bid decides each scenario; pass
/// <c>ai</c> to play against the real <see cref="SimpleAiStrategy"/>.
/// </summary>
internal static class TestGameSessions
{
    public static GameSession Unpaced(int seed = 42, IPlayerStrategy? ai = null) =>
        new(aiPacing: false, randomFactory: () => new Random(seed), aiStrategy: ai ?? new PassingAi());
}



/// <summary>
/// <see cref="SimpleAiStrategy"/> that never bids: every other decision is the real
/// AI's. For tests whose scenario needs the human's bid to stand, or everyone to pass
/// so the dealer is stuck.
/// </summary>
internal class PassingAi : IPlayerStrategy
{
    private readonly SimpleAiStrategy _ai = new();



    public virtual BidAction DecideBid(GameState state, PlayerPosition player) => BidAction.PassBid.Instance;



    public Suit? DecideTrump(GameState state, PlayerPosition player) => _ai.DecideTrump(state, player);



    public Card DecidePlay(GameState state, PlayerPosition player) => _ai.DecidePlay(state, player);



    public void DecideHawseyExchange(GameState state, PlayerPosition bidder, out Card[] cardsToDiscard, out Card[] cardsFromPartner) =>
        _ai.DecideHawseyExchange(state, bidder, out cardsToDiscard, out cardsFromPartner);
}

