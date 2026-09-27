using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// Every <see cref="GameEngine"/> transition returns a new <see cref="GameState"/> and
/// leaves its input exactly as it was (#870). <c>PlayCard</c> used to append the card
/// to the input state's trick (#861), which broke replaying from an earlier state.
/// </summary>
public class TransitionImmutabilityPropertyTests
{
    private static readonly PlayerPosition[] Dealers =
        [PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West];

    private readonly GameEngine _engine = new();



    /// <summary>
    /// Everything a transition could change in its input, copied out of the state as
    /// text (records would need IsExternalInit on the .NET Framework test targets).
    /// </summary>
    private static string Take(GameState state) =>
        string.Join
        (
            " / ",
            state.Phase,
            state.NextToAct,
            state.NorthSouthScore,
            state.EastWestScore,
            string.Join("|", Dealers.Select(p => $"{p}:{string.Join(",", state.Hands[p])}")),
            state.CurrentTrick == null ? "-" : string.Join(",", state.CurrentTrick.Plays.Select(p => $"{p.Player}={p.Card}")),
            string.Join("|", state.CompletedTricks.Select(t => $"{t.Winner}:{string.Join(",", t.Cards.Select(c => c.Card))}"))
        );



    [FuzzProperty]
    public void No_transition_in_a_round_changes_its_input_state(int seed, int dealer, bool hawsey)
    {
        var state = _engine.StartGame(HouseRules.Default, Dealers[Math.Abs(dealer % Dealers.Length)], new Random(seed));
        var strategy = hawsey
            ? new TestPlayerStrategy(new Queue<BidAction>([BidAction.HawseyBid.Instance]))
            : new TestPlayerStrategy();
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (state.Phase is not (GamePhase.RoundScoring or GamePhase.GameOver))
        {
            var before = Take(state);
            GameState next;

            if (state.Phase == GamePhase.Bidding)
            {
                var bidder = biddingPhase.GetNextBidder()!.Value;
                next = _engine.PlaceBid(state, bidder, strategy.DecideBid(state, bidder), biddingPhase);
            }
            else if (state.Phase == GamePhase.TrumpSelection)
            {
                next = _engine.SelectTrump(state, strategy.DecideTrump(state, state.NextToAct!.Value));
            }
            else if (state.Phase == GamePhase.HawseyExchange)
            {
                strategy.DecideHawseyExchange(state, state.HawseyBidder!.Value, out var discard, out var fromPartner);
                next = _engine.ExchangeHawseyCards(state, discard, fromPartner);
            }
            else
            {
                var player = state.NextToAct!.Value;
                next = _engine.PlayCard(state, player, strategy.DecidePlay(state, player));
            }

            Assert.NotSame(state, next);
            Assert.Equal(before, Take(state));
            state = next;
        }

        if (state.Phase == GamePhase.RoundScoring)
        {
            var before = Take(state);
            _engine.StartNextRound(state, new Random(seed));
            Assert.Equal(before, Take(state));
        }
    }



    [Fact]
    public void PlayCard_twice_from_the_same_state_gives_the_same_result()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (state.Phase == GamePhase.Bidding)
        {
            state = _engine.PlaceBid(state, biddingPhase.GetNextBidder()!.Value, BidAction.PassBid.Instance, biddingPhase);
        }

        state = _engine.SelectTrump(state, Suit.Hearts);
        var leader = state.NextToAct!.Value;
        var card = state.GetLegalPlays()[0];

        var first = _engine.PlayCard(state, leader, card);
        var second = _engine.PlayCard(state, leader, card);

        Assert.Empty(state.CurrentTrick!.Plays);
        Assert.Single(first.CurrentTrick!.Plays);
        Assert.Single(second.CurrentTrick!.Plays);
        Assert.NotSame(first.CurrentTrick, second.CurrentTrick);
    }
}
