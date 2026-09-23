using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;

namespace Wolfgang.Hawsey.Engine.AotSmoke;

/// <summary>
/// Native AOT smoke consumer: plays complete games through the published,
/// trimmed engine and exits non-zero if any game fails to finish properly.
/// </summary>
internal static class Program
{
    private const int GameCount = 25;



    private static int Main()
    {
        var runner = new GameRunner();
        var failures = 0;
        var hawseyExchanges = 0;

        for (var seed = 0; seed < GameCount; seed++)
        {
            var rules = new HouseRules { MustBeat = seed % 2 == 0 };
            var strategy = new FirstLegalCardStrategy
            (
                aceHighEveryOtherRound: seed % 3 == 0,
                northCallsHawsey: seed % 4 == 1
            );
            var state = runner.RunGame(strategy, rules, PlayerPosition.South, new Random(seed));
            hawseyExchanges += strategy.HawseyExchanges;

            var topScore = Math.Max(state.NorthSouthScore, state.EastWestScore);
            if (state.Phase != GamePhase.GameOver || topScore < rules.PointsToWin)
            {
                Console.Error.WriteLine($"Game {seed}: ended in {state.Phase} at {state.NorthSouthScore}-{state.EastWestScore}.");
                failures++;
            }
        }

        if (hawseyExchanges == 0)
        {
            Console.Error.WriteLine("No game reached a Hawsey exchange; that path went unexercised.");
            failures++;
        }

        Console.WriteLine($"AOT smoke: {GameCount - failures}/{GameCount} games completed, {hawseyExchanges} Hawsey exchanges.");
        return failures == 0 ? 0 : 1;
    }



    /// <summary>
    /// Passes every bid (so the dealer is stuck at the minimum) unless North is
    /// set to call Hawsey, which drives the Hawsey exchange path. Names hearts or
    /// ace-high, plays the first legal card and exchanges the first two cards.
    /// </summary>
    private sealed class FirstLegalCardStrategy(bool aceHighEveryOtherRound, bool northCallsHawsey) : IPlayerStrategy
    {
        private int _trumpCalls;



        public int HawseyExchanges { get; private set; }



        public BidAction DecideBid(GameState state, PlayerPosition player) =>
            northCallsHawsey && player == PlayerPosition.North
                ? BidAction.HawseyBid.Instance
                : BidAction.PassBid.Instance;



        public Suit? DecideTrump(GameState state, PlayerPosition player)
        {
            _trumpCalls++;
            return aceHighEveryOtherRound && _trumpCalls % 2 == 0 ? null : Suit.Hearts;
        }



        public Card DecidePlay(GameState state, PlayerPosition player) => state.GetLegalPlays()[0];



        public void DecideHawseyExchange
        (
            GameState state,
            PlayerPosition bidder,
            out Card[] cardsToDiscard,
            out Card[] cardsFromPartner
        )
        {
            HawseyExchanges++;
            var bidderHand = state.Hands[bidder];
            var partnerHand = state.Hands[bidder.Partner()];
            cardsToDiscard = [bidderHand[0], bidderHand[1]];
            cardsFromPartner = [partnerHand[0], partnerHand[1]];
        }
    }
}
