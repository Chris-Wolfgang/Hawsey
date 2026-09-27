using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

/// <summary>
/// Game sessions for tests, with a seeded deal so games repeat. The AI seats default
/// to <see cref="PassingAi"/>, so a scenario's bidding is the human's to decide; pass
/// <c>ai</c> to use another strategy (such as the real <see cref="SimpleAiStrategy"/>).
/// </summary>
internal static class TestGameSessions
{
    /// <summary>
    /// No pauses between AI moves: every AI loop completes before its task is returned.
    /// </summary>
    public static GameSession Unpaced(int seed = 42, IPlayerStrategy? ai = null) =>
        new(aiPacing: false, randomFactory: () => new Random(seed), aiStrategy: ai ?? new PassingAi());



    /// <summary>
    /// Paced like the app (real pauses between AI moves). For tests that start a New
    /// Game while an AI loop is paused.
    /// </summary>
    public static GameSession Paced(int seed = 42, IPlayerStrategy? ai = null) =>
        new(aiPacing: true, randomFactory: () => new Random(seed), aiStrategy: ai ?? new PassingAi());
}



/// <summary>
/// <see cref="SimpleAiStrategy"/> that never bids: every other decision is the real
/// AI's. For tests whose scenario needs the human's bid to stand, or everyone to pass
/// so the dealer is stuck.
/// </summary>
internal sealed class PassingAi : IPlayerStrategy
{
    private readonly SimpleAiStrategy _ai = new();



    public BidAction DecideBid(GameState state, PlayerPosition player) => BidAction.PassBid.Instance;



    public Suit? DecideTrump(GameState state, PlayerPosition player) => _ai.DecideTrump(state, player);



    public Card DecidePlay(GameState state, PlayerPosition player) => _ai.DecidePlay(state, player);



    public void DecideHawseyExchange(GameState state, PlayerPosition bidder, out Card[] cardsToDiscard, out Card[] cardsFromPartner) =>
        _ai.DecideHawseyExchange(state, bidder, out cardsToDiscard, out cardsFromPartner);
}
