using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.TrickPlay;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// The public constructors of <see cref="GameState"/> and <see cref="TrickResult"/> refuse
/// a <see langword="null"/> collection or rules object up front, like the engine's other
/// public entry points.
/// </summary>
public class GameStateConstructorTests
{
    private static GameState Create
    (
        Dictionary<PlayerPosition, List<Card>>? hands,
        List<TrickResult>? completedTricks,
        HouseRules? rules
    ) =>
        new GameState
        (
            GamePhase.Bidding,
            PlayerPosition.North,
            hands!,
            trumpSuit: null,
            TrumpMode.AceHigh,
            biddingResult: null,
            completedTricks!,
            currentTrick: null,
            northSouthScore: 0,
            eastWestScore: 0,
            rules!,
            nextToAct: PlayerPosition.East,
            tricksPlayedInRound: 0,
            isHawseyRound: false,
            hawseyBidder: null
        );



    [Fact]
    public void Constructor_when_every_argument_is_set_keeps_them()
    {
        var hands = new Dictionary<PlayerPosition, List<Card>>();
        var completedTricks = new List<TrickResult>();

        var state = Create(hands, completedTricks, HouseRules.Default);

        Assert.Same(hands, state.Hands);
        Assert.Same(completedTricks, state.CompletedTricks);
        Assert.Same(HouseRules.Default, state.Rules);
    }



    [Fact]
    public void Constructor_when_hands_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Create(null, new List<TrickResult>(), HouseRules.Default));

        Assert.Equal("hands", ex.ParamName);
    }



    [Fact]
    public void Constructor_when_completedTricks_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Create(new Dictionary<PlayerPosition, List<Card>>(), null, HouseRules.Default));

        Assert.Equal("completedTricks", ex.ParamName);
    }



    [Fact]
    public void Constructor_when_rules_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Create(new Dictionary<PlayerPosition, List<Card>>(), new List<TrickResult>(), null));

        Assert.Equal("rules", ex.ParamName);
    }



    [Fact]
    public void TrickResult_when_cards_is_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new TrickResult(PlayerPosition.North, null!));

        Assert.Equal("cards", ex.ParamName);
    }
}
