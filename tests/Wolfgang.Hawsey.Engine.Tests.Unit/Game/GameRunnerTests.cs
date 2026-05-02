using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

public class GameRunnerTests
{
    [Fact]
    public void RunGame_completes_to_game_over()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy(preferredTrump: Suit.Clubs);
        var rules = new HouseRules { PointsToWin = 10 };

        var finalState = runner.RunGame(strategy, rules, PlayerPosition.North, new Random(42));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
    }



    [Fact]
    public void RunGame_when_default_rules_completes()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy(preferredTrump: Suit.Hearts);

        var finalState = runner.RunGame(strategy, HouseRules.Default, PlayerPosition.East, new Random(123));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
        Assert.True
        (
            finalState.NorthSouthScore >= 62 || finalState.EastWestScore >= 62
        );
    }



    [Fact]
    public void RunGame_with_ace_high_completes()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy(aceHigh: true);
        var rules = new HouseRules { PointsToWin = 10 };

        var finalState = runner.RunGame(strategy, rules, PlayerPosition.South, new Random(77));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
    }



    [Fact]
    public void RunGame_with_must_beat_and_must_trump_completes()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy(preferredTrump: Suit.Spades);
        var rules = new HouseRules
        {
            MustBeat = true,
            MustTrump = true,
            PointsToWin = 10
        };

        var finalState = runner.RunGame(strategy, rules, PlayerPosition.West, new Random(55));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
    }



    [Fact]
    public void RunGame_when_null_strategy_throws()
    {
        var runner = new GameRunner();

        Assert.Throws<ArgumentNullException>
        (
            () => runner.RunGame(null!, HouseRules.Default, PlayerPosition.North, new Random(42))
        );
    }



    [Fact]
    public void RunGame_when_null_rules_throws()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy();

        Assert.Throws<ArgumentNullException>
        (
            () => runner.RunGame(strategy, null!, PlayerPosition.North, new Random(42))
        );
    }



    [Fact]
    public void RunGame_when_null_random_throws()
    {
        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy();

        Assert.Throws<ArgumentNullException>
        (
            () => runner.RunGame(strategy, HouseRules.Default, PlayerPosition.North, null!)
        );
    }



    [Fact]
    public void RunGame_with_hawsey_bid_completes()
    {
        var bidQueue = new Queue<BidAction>();
        bidQueue.Enqueue(BidAction.HawseyBid.Instance);

        var runner = new GameRunner();
        var strategy = new TestPlayerStrategy
        (
            bidQueue: bidQueue,
            preferredTrump: Suit.Diamonds
        );
        var rules = new HouseRules { PointsToWin = 10 };

        var finalState = runner.RunGame(strategy, rules, PlayerPosition.North, new Random(42));

        Assert.Equal(GamePhase.GameOver, finalState.Phase);
    }



    [Fact]
    public void RunGame_deterministic_with_same_seed()
    {
        var runner = new GameRunner();
        var strategy1 = new TestPlayerStrategy(preferredTrump: Suit.Hearts);
        var strategy2 = new TestPlayerStrategy(preferredTrump: Suit.Hearts);
        var rules = new HouseRules { PointsToWin = 20 };

        var result1 = runner.RunGame(strategy1, rules, PlayerPosition.North, new Random(42));
        var result2 = runner.RunGame(strategy2, rules, PlayerPosition.North, new Random(42));

        Assert.Equal(result1.NorthSouthScore, result2.NorthSouthScore);
        Assert.Equal(result1.EastWestScore, result2.EastWestScore);
    }
}
