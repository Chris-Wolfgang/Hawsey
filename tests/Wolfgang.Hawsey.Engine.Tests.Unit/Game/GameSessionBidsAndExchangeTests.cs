using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// <see cref="GameSession.Bids"/> (each seat's bid this round, for a UI's bid badges)
/// and <see cref="GameSession.PerformHumanHawseyExchange"/> (the human picks the two
/// discards; the partner gives their two best). Both used to live only in the Blazor
/// UI's own service.
/// </summary>
public class GameSessionBidsAndExchangeTests
{
    /// <summary>East (the first bidder, North dealing) bids 8; everyone else passes.</summary>
    private sealed class EastBidsEight : PassingAi
    {
        public override BidAction DecideBid(GameState state, PlayerPosition player) =>
            player == PlayerPosition.East ? new BidAction.NumberBid(8) : BidAction.PassBid.Instance;
    }



    private static async Task<GameSession> AtHumanHawseyExchangeAsync()
    {
        var session = TestGameSessions.Unpaced();
        session.StartNewGame();
        Assert.True(await session.AdvanceAiBiddingAsync());
        Assert.True(session.PlaceHumanBid(BidAction.HawseyBid.Instance));
        Assert.True(await session.HandleTrumpSelectionAsync());
        Assert.True(session.SelectTrump(Suit.Spades));
        Assert.True(await session.HandleHawseyExchangeAsync());
        return session;
    }



    [Fact]
    public async Task Bids_lists_each_bid_of_the_round_in_order()
    {
        var session = TestGameSessions.Unpaced(ai: new EastBidsEight());
        session.StartNewGame();
        Assert.Empty(session.Bids);

        Assert.True(await session.AdvanceAiBiddingAsync());
        Assert.True(session.PlaceHumanBid(new BidAction.NumberBid(9)));
        await session.AdvanceAiBiddingAsync();

        var bids = session.Bids;
        Assert.Equal
        (
            [PlayerPosition.East, PlayerPosition.South, PlayerPosition.West, PlayerPosition.North],
            bids.Select(b => b.Player)
        );
        Assert.Equal(8, Assert.IsType<BidAction.NumberBid>(bids[0].Action).Amount);
        Assert.Equal(9, Assert.IsType<BidAction.NumberBid>(bids[1].Action).Amount);
        Assert.IsType<BidAction.PassBid>(bids[2].Action);
        Assert.IsType<BidAction.PassBid>(bids[3].Action);
    }



    [Fact]
    public async Task Bids_is_a_snapshot_and_a_refused_bid_is_not_listed()
    {
        var session = TestGameSessions.Unpaced(ai: new EastBidsEight());
        session.StartNewGame();
        Assert.True(await session.AdvanceAiBiddingAsync());
        var snapshot = session.Bids;

        Assert.False(session.PlaceHumanBid(new BidAction.NumberBid(7)));
        Assert.True(session.PlaceHumanBid(BidAction.PassBid.Instance));

        Assert.Single(snapshot);
        Assert.Equal(2, session.Bids.Count);
    }



    [Fact]
    public async Task Bids_starts_empty_for_each_new_deal()
    {
        var session = TestGameSessions.Unpaced(ai: new EastBidsEight());
        session.StartNewGame();
        Assert.True(await session.AdvanceAiBiddingAsync());
        Assert.NotEmpty(session.Bids);

        session.StartNewGame();

        Assert.Empty(session.Bids);
    }



    [Fact]
    public async Task PerformHumanHawseyExchange_discards_the_humans_cards_and_takes_the_partners_best()
    {
        var session = await AtHumanHawseyExchangeAsync();
        var state = session.CurrentState!;
        var hand = state.Hands[GameSession.HumanPosition];
        Card[] discard = [hand[0], hand[1]];
        new SimpleAiStrategy().DecideHawseyExchange(state, GameSession.HumanPosition, out _, out var partnersBest);

        Assert.True(session.PerformHumanHawseyExchange(discard));

        var after = session.CurrentState!;
        var newHand = after.Hands[GameSession.HumanPosition];
        Assert.Equal(GamePhase.TrickPlay, after.Phase);
        Assert.Equal(12, newHand.Count);
        Assert.All(partnersBest, c => Assert.Contains(c, newHand));
    }



    [Fact]
    public async Task PerformHumanHawseyExchange_refuses_a_discard_that_is_not_two_held_cards()
    {
        var session = await AtHumanHawseyExchangeAsync();
        var before = session.CurrentState!;
        var hand = before.Hands[GameSession.HumanPosition];
        var notHeld = Deck.CreatePinochleDeck().First(c => !hand.Contains(c));

        Assert.False(session.PerformHumanHawseyExchange([hand[0]]));
        Assert.False(session.PerformHumanHawseyExchange([hand[0], notHeld]));

        // A card the human holds one copy of can't be discarded twice.
        var single = hand.First(c => hand.Count(x => x == c) == 1);
        Assert.False(session.PerformHumanHawseyExchange([single, single]));

        Assert.Equal(GamePhase.HawseyExchange, session.CurrentState!.Phase);
    }



    [Fact]
    public void PerformHumanHawseyExchange_outside_the_humans_exchange_does_nothing()
    {
        var session = TestGameSessions.Unpaced();
        session.StartNewGame();

        Assert.False(session.PerformHumanHawseyExchange([new Card(Rank.Ace, Suit.Spades), new Card(Rank.King, Suit.Spades)]));
    }



    [Fact]
    public void PerformHumanHawseyExchange_with_a_null_discard_throws()
    {
        var session = TestGameSessions.Unpaced();

        Assert.Equal("discard", Assert.Throws<ArgumentNullException>(() => session.PerformHumanHawseyExchange(null!)).ParamName);
    }
}
