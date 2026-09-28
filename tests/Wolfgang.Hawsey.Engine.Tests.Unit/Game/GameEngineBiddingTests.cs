using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// The bidding rules of <see cref="GameEngine.PlaceBid"/>, which works the bidding out
/// from the <see cref="GameState"/> alone: once round clockwise from the dealer's left,
/// the dealer last, a Hawsey bid ending it at once, and a stuck dealer when nobody bids.
/// North deals in every test here, so East bids first.
/// </summary>
public class GameEngineBiddingTests
{
    private readonly GameEngine _engine = new();



    private GameState Deal(HouseRules? rules = null) =>
        _engine.StartGame(rules ?? HouseRules.Default, PlayerPosition.North, new Random(42));



    private GameState Bid(GameState state, params BidAction[] actions)
    {
        foreach (var action in actions)
        {
            state = _engine.PlaceBid(state, state.NextToAct!.Value, action);
        }

        return state;
    }



    private static BidAction Pass => BidAction.PassBid.Instance;



    [Fact]
    public void Bidding_starts_left_of_the_dealer_and_goes_clockwise()
    {
        var state = Deal();
        Assert.Equal(PlayerPosition.East, state.NextToAct);

        state = Bid(state, Pass);
        Assert.Equal(PlayerPosition.South, state.NextToAct);

        state = Bid(state, Pass);
        Assert.Equal(PlayerPosition.West, state.NextToAct);

        state = Bid(state, Pass);
        Assert.Equal(PlayerPosition.North, state.NextToAct);
        Assert.Null(state.BiddingResult);
    }



    [Fact]
    public void When_all_pass_the_dealer_is_stuck_at_the_minimum()
    {
        var state = Bid(Deal(), Pass, Pass, Pass, Pass);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        var result = state.BiddingResult!;
        Assert.Equal(PlayerPosition.North, result.Winner);
        Assert.Equal(6, result.BidAmount);
        Assert.True(result.IsStuck);
        Assert.False(result.IsHawsey);
        Assert.Equal(PlayerPosition.North, state.NextToAct);
    }



    [Fact]
    public void When_all_pass_the_dealer_is_stuck_at_a_custom_minimum()
    {
        var state = Bid(Deal(new HouseRules { MinimumBid = 8 }), Pass, Pass, Pass, Pass);

        Assert.Equal(8, state.BiddingResult!.BidAmount);
        Assert.True(state.BiddingResult.IsStuck);
    }



    [Fact]
    public void The_highest_bidder_wins_and_names_trump()
    {
        var state = Bid(Deal(), new BidAction.NumberBid(7), new BidAction.NumberBid(8), Pass, Pass);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        var result = state.BiddingResult!;
        Assert.Equal(PlayerPosition.South, result.Winner);
        Assert.Equal(8, result.BidAmount);
        Assert.False(result.IsStuck);
        Assert.Equal(PlayerPosition.South, state.NextToAct);
    }



    [Fact]
    public void The_dealer_bids_last_and_can_win()
    {
        var state = Bid(Deal(), new BidAction.NumberBid(7), Pass, Pass, new BidAction.NumberBid(9));

        Assert.Equal(PlayerPosition.North, state.BiddingResult!.Winner);
        Assert.Equal(9, state.BiddingResult.BidAmount);
    }



    [Fact]
    public void A_Hawsey_bid_ends_the_bidding_at_once()
    {
        var state = Bid(Deal(), BidAction.HawseyBid.Instance);

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
        var result = state.BiddingResult!;
        Assert.Equal(PlayerPosition.East, result.Winner);
        Assert.True(result.IsHawsey);
        Assert.False(result.IsStuck);
        Assert.Equal(24, result.BidAmount);
        Assert.True(state.IsHawseyRound);
        Assert.Equal(PlayerPosition.East, state.HawseyBidder);
    }



    [Fact]
    public void A_Hawsey_bid_after_number_bids_ends_the_bidding_at_once()
    {
        var state = Bid(Deal(), new BidAction.NumberBid(8), BidAction.HawseyBid.Instance);

        Assert.Equal(PlayerPosition.South, state.BiddingResult!.Winner);
        Assert.True(state.BiddingResult.IsHawsey);
    }



    [Fact]
    public void A_bid_of_the_maximum_is_accepted()
    {
        var state = Bid(Deal(), new BidAction.NumberBid(GameState.MaximumBid));

        Assert.Equal(GameState.MaximumBid, state.HighBid);
    }



    [Theory]
    [InlineData(5)]
    [InlineData(13)]
    public void A_number_bid_outside_the_minimum_to_the_maximum_throws(int amount)
    {
        var state = Deal();

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(amount)));
    }



    [Fact]
    public void A_bid_below_a_custom_minimum_throws()
    {
        var state = Deal(new HouseRules { MinimumBid = 8 });

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.East, new BidAction.NumberBid(7)));
    }



    [Fact]
    public void A_bid_that_does_not_beat_the_high_bid_throws()
    {
        var state = Bid(Deal(), new BidAction.NumberBid(8));

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.South, new BidAction.NumberBid(8)));
    }



    [Fact]
    public void A_bid_out_of_turn_throws()
    {
        var state = Deal();

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.South, Pass));
    }



    [Fact]
    public void A_bid_after_bidding_is_over_throws()
    {
        var state = Bid(Deal(), Pass, Pass, Pass, Pass);

        Assert.Throws<InvalidOperationException>(() => _engine.PlaceBid(state, PlayerPosition.North, Pass));
    }



    [Fact]
    public void A_null_action_throws()
    {
        var state = Deal();

        var ex = Assert.Throws<ArgumentNullException>(() => _engine.PlaceBid(state, PlayerPosition.East, null!));

        Assert.Equal("action", ex.ParamName);
    }
}
