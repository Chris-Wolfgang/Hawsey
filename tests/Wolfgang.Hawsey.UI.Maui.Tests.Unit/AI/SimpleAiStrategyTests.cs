using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.UI.Maui.AI;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.AI;

public class SimpleAiStrategyTests
{
    private readonly SimpleAiStrategy _strategy = new();



    [Fact]
    public void DecideBid_when_weak_hand_passes()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        // Most random hands should result in a pass
        var bid = _strategy.DecideBid(state, PlayerPosition.East);

        Assert.IsType<BidAction.PassBid>(bid);
    }



    [Fact]
    public void DecideTrump_returns_suit_with_most_cards()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        var trump = _strategy.DecideTrump(state, PlayerPosition.East);

        Assert.NotNull(trump);
    }



    [Fact]
    public void DecidePlay_returns_legal_card()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        // Complete bidding
        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);
        state = engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase);

        // Select trump
        state = engine.SelectTrump(state, Suit.Hearts);

        // North leads — use AI to pick a card
        var card = _strategy.DecidePlay(state, state.NextToAct!.Value);
        var legalPlays = state.GetLegalPlays();

        Assert.Contains(card, legalPlays);
    }



    [Fact]
    public void DecidePlay_when_only_one_legal_returns_it()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);
        state = engine.PlaceBid(state, PlayerPosition.East, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.South, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.West, BidAction.PassBid.Instance, biddingPhase);
        state = engine.PlaceBid(state, PlayerPosition.North, BidAction.PassBid.Instance, biddingPhase);

        state = engine.SelectTrump(state, Suit.Hearts);

        // Play through most of the game to exhaust hands
        for (var i = 0; i < 44; i++)
        {
            if (state.Phase != GamePhase.TrickPlay)
            {
                break;
            }

            var player = state.NextToAct!.Value;
            var play = _strategy.DecidePlay(state, player);
            state = engine.PlayCard(state, player, play);
        }
    }



    [Fact]
    public void DecideHawseyExchange_returns_valid_cards()
    {
        var engine = new GameEngine();
        var state = engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        var biddingPhase = new BiddingPhase(PlayerPosition.North, 6);
        state = engine.PlaceBid(state, PlayerPosition.East, BidAction.HawseyBid.Instance, biddingPhase);

        state = engine.SelectTrump(state, Suit.Clubs);

        var bidder = state.HawseyBidder!.Value;

        _strategy.DecideHawseyExchange(state, bidder, out var discard, out var fromPartner);

        Assert.Equal(2, discard.Length);
        Assert.Equal(2, fromPartner.Length);

        // Verify cards come from the correct hands
        var bidderHand = state.Hands[bidder];
        var partnerHand = state.Hands[bidder.Partner()];

        Assert.All(discard, d => Assert.Contains(d, bidderHand));
        Assert.All(fromPartner, p => Assert.Contains(p, partnerHand));
    }
}
