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



    [GlobalSetup]
    public void Setup()
    {
        _deck = Deck.CreatePinochleDeck();
        _hand = Deck.Shuffle(_deck, new Random(42)).Take(12).ToList();
        _comparer = new CardComparer(Suit.Hearts, Suit.Spades);
        _currentWinner = new Card(Rank.Ten, Suit.Spades);
    }



    [Benchmark]
    public IReadOnlyList<Card> Shuffle() => Deck.Shuffle(_deck, new Random(42));



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
        _runner.RunGame(new FirstLegalCardStrategy(), _rules, PlayerPosition.South, new Random(7));



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
