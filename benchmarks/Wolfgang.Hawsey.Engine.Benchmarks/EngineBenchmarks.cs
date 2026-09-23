using BenchmarkDotNet.Attributes;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Benchmarks;

/// <summary>
/// The engine's hot operations: dealing, legal-move generation, trick
/// comparison and a whole simulated game. Every input is seeded, so each run
/// measures identical work.
/// </summary>
[MemoryDiagnoser]
public class EngineBenchmarks
{
    private readonly GameRunner _runner = new();
    private readonly HouseRules _rules = new() { MustBeat = true };
    private IReadOnlyList<Card> _deck = [];
    private List<Card> _hand = [];
    private CardComparer _comparer = new(Suit.Hearts, Suit.Spades);
    private Card _currentWinner;
    private Random _shuffleRandom = Seeded(42);



    /// <summary>
    /// Every benchmark input is dealt from a fixed seed so each run measures identical
    /// work. S2245 (use a cryptographic RNG) does not apply: benchmark dealing is not
    /// security-sensitive and must be reproducible, so it is suppressed at this one
    /// construction site.
    /// </summary>
    private static Random Seeded(int seed)
    {
#pragma warning disable S2245
        return new Random(seed);
#pragma warning restore S2245
    }



    [GlobalSetup]
    public void Setup()
    {
        _deck = Deck.CreatePinochleDeck();
        _hand = Deck.Shuffle(_deck, Seeded(42)).Take(12).ToList();
        _comparer = new CardComparer(Suit.Hearts, Suit.Spades);
        _currentWinner = new Card(Rank.Ten, Suit.Spades);
        _shuffleRandom = Seeded(42);
    }



    /// <summary>
    /// Shuffle only: the RNG lives in benchmark state (seeded once in setup), so
    /// constructing it is not timed or counted as an allocation.
    /// </summary>
    [Benchmark]
    public IReadOnlyList<Card> Shuffle() => Deck.Shuffle(_deck, _shuffleRandom);



    [Benchmark]
    public IReadOnlyList<Card> GetLegalPlays() =>
        FollowSuitValidator.GetLegalPlays(_hand, Suit.Spades, Suit.Hearts, _rules, _currentWinner);



    [Benchmark]
    public List<Card> SortHandByStrength()
    {
        var sorted = new List<Card>(_hand);
        sorted.Sort(_comparer);
        return sorted;
    }



    [Benchmark]
    public GameState PlayFullGame() =>
        _runner.RunGame(new FirstLegalCardStrategy(), _rules, PlayerPosition.South, Seeded(7));



    /// <summary>Passes every bid, names hearts, plays the first legal card.</summary>
    private sealed class FirstLegalCardStrategy : IPlayerStrategy
    {
        public BidAction DecideBid(GameState state, PlayerPosition player) => BidAction.PassBid.Instance;



        public Suit? DecideTrump(GameState state, PlayerPosition player) => Suit.Hearts;



        public Card DecidePlay(GameState state, PlayerPosition player) => state.GetLegalPlays()[0];



        public void DecideHawseyExchange
        (
            GameState state,
            PlayerPosition bidder,
            out Card[] cardsToDiscard,
            out Card[] cardsFromPartner
        )
        {
            var bidderHand = state.Hands[bidder];
            var partnerHand = state.Hands[bidder.Partner()];
            cardsToDiscard = [bidderHand[0], bidderHand[1]];
            cardsFromPartner = [partnerHand[0], partnerHand[1]];
        }
    }
}
