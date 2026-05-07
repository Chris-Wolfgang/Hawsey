using Wolfgang.Hawsey.Engine;
using Wolfgang.Hawsey.UI.Maui.AI;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Services;

/// <summary>
/// Integration tests covering the engine + AI strategy interaction
/// (the same flow patterns the MAUI <c>GameService</c> orchestrates),
/// without taking any MAUI dependency.
/// </summary>
public class EngineAiIntegrationTests
{
    private readonly GameEngine _engine = new();
    private readonly SimpleAiStrategy _ai = new();



    [Fact]
    public void Full_bidding_round_with_ai_completes()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (!biddingPhase.IsComplete)
        {
            var bidder = biddingPhase.GetNextBidder()!.Value;
            var action = _ai.DecideBid(state, bidder);
            state = _engine.PlaceBid(state, bidder, action, biddingPhase);
        }

        Assert.Equal(GamePhase.TrumpSelection, state.Phase);
    }



    [Fact]
    public void Full_round_with_ai_completes()
    {
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));

        // Bidding
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        while (!biddingPhase.IsComplete)
        {
            var bidder = biddingPhase.GetNextBidder()!.Value;
            var action = _ai.DecideBid(state, bidder);
            state = _engine.PlaceBid(state, bidder, action, biddingPhase);
        }

        // Trump selection
        var trump = _ai.DecideTrump(state, state.NextToAct!.Value);
        state = _engine.SelectTrump(state, trump);

        // Trick play
        while (state.Phase == GamePhase.TrickPlay)
        {
            var player = state.NextToAct!.Value;
            var card = _ai.DecidePlay(state, player);
            state = _engine.PlayCard(state, player, card);
        }

        Assert.True
        (
            state.Phase == GamePhase.RoundScoring || state.Phase == GamePhase.GameOver
        );
    }



    [Fact]
    public void Multiple_rounds_with_ai_reach_game_over()
    {
        var random = new Random(42);
        var rules = new HouseRules { PointsToWin = 15 };
        var state = _engine.StartGame(rules, PlayerPosition.North, random);

        var maxRounds = 50;

        for (var round = 0; round < maxRounds && state.Phase != GamePhase.GameOver; round++)
        {
            // Bidding
            var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

            while (!biddingPhase.IsComplete)
            {
                var bidder = biddingPhase.GetNextBidder()!.Value;
                var action = _ai.DecideBid(state, bidder);
                state = _engine.PlaceBid(state, bidder, action, biddingPhase);
            }

            // Trump
            var trump = _ai.DecideTrump(state, state.NextToAct!.Value);
            state = _engine.SelectTrump(state, trump);

            // Tricks
            while (state.Phase == GamePhase.TrickPlay)
            {
                var player = state.NextToAct!.Value;
                var card = _ai.DecidePlay(state, player);
                state = _engine.PlayCard(state, player, card);
            }

            if (state.Phase == GamePhase.RoundScoring)
            {
                state = _engine.StartNextRound(state, random);
            }
        }

        Assert.Equal(GamePhase.GameOver, state.Phase);
    }



    [Fact]
    public void Human_interleaved_with_ai_works()
    {
        // Simulate a game where human is South and AI plays the other 3
        var state = _engine.StartGame(HouseRules.Default, PlayerPosition.North, new Random(42));
        var biddingPhase = new BiddingPhase(state.Dealer, state.Rules.MinimumBid);

        // Process bidding — human always passes
        while (!biddingPhase.IsComplete)
        {
            var bidder = biddingPhase.GetNextBidder()!.Value;
            var action = bidder == PlayerPosition.South
                ? BidAction.PassBid.Instance
                : _ai.DecideBid(state, bidder);
            state = _engine.PlaceBid(state, bidder, action, biddingPhase);
        }

        // Trump selection (whoever won, let AI/human handle)
        var trumpPlayer = state.NextToAct!.Value;
        var trumpSuit = _ai.DecideTrump(state, trumpPlayer);
        state = _engine.SelectTrump(state, trumpSuit);

        // Play tricks — human plays first legal card, AI uses strategy
        while (state.Phase == GamePhase.TrickPlay)
        {
            var player = state.NextToAct!.Value;

            if (player == PlayerPosition.South)
            {
                var legalPlays = state.GetLegalPlays();
                state = _engine.PlayCard(state, player, legalPlays[0]);
            }
            else
            {
                var card = _ai.DecidePlay(state, player);
                state = _engine.PlayCard(state, player, card);
            }
        }

        Assert.True
        (
            state.Phase == GamePhase.RoundScoring || state.Phase == GamePhase.GameOver
        );
    }
}
