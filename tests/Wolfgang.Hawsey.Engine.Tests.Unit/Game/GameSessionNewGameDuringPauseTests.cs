using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Game;

/// <summary>
/// A New Game started while an AI loop is paused must stop that loop: it may not
/// move, or announce a trick, round or game, into the new game. Each test starts an
/// AI step on a paced service (a pause of 400 ms or more), starts a New Game before
/// the pause can end, and checks the old loop gives up. The Coyote tests explore
/// the same races under every interleaving; these pin each guard directly.
/// </summary>
public class GameSessionNewGameDuringPauseTests
{
    private readonly GameSession _session = TestGameSessions.Paced();



    /// <summary>
    /// Everyone passes, so North (the dealer) is stuck and is next to name trump.
    /// </summary>
    private async Task ReachNorthNamingTrumpAsync()
    {
        _session.StartNewGame();
        Assert.True(await _session.AdvanceAiBiddingAsync());
        Assert.True(_session.PlaceHumanBid(BidAction.PassBid.Instance));
        Assert.False(await _session.AdvanceAiBiddingAsync());
        Assert.Equal(GamePhase.TrumpSelection, _session.CurrentState!.Phase);
        Assert.Equal(PlayerPosition.North, _session.CurrentState.NextToAct);
    }



    [Fact]
    public async Task AdvanceAiBiddingAsync_stops_when_a_new_game_starts_during_its_pause()
    {
        _session.StartNewGame();

        var bidding = _session.AdvanceAiBiddingAsync();
        Assert.False(bidding.IsCompleted, "The AI step should be paused.");
        _session.StartNewGame();

        Assert.False(await bidding);
        Assert.Equal(PlayerPosition.East, _session.CurrentState!.NextToAct);
    }



    [Fact]
    public async Task HandleTrumpSelectionAsync_stops_when_a_new_game_starts_during_its_pause()
    {
        await ReachNorthNamingTrumpAsync();

        var naming = _session.HandleTrumpSelectionAsync();
        Assert.False(naming.IsCompleted, "The AI step should be paused.");
        _session.StartNewGame();

        Assert.False(await naming);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public async Task AdvanceAiPlaysAsync_stops_when_a_new_game_starts_during_its_pause()
    {
        await ReachNorthNamingTrumpAsync();
        Assert.False(await _session.HandleTrumpSelectionAsync());

        var playing = _session.AdvanceAiPlaysAsync();
        Assert.False(playing.IsCompleted, "The AI step should be paused.");
        _session.StartNewGame();

        Assert.False(await playing);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState!.Phase);
    }



    [Fact]
    public async Task AdvanceAiPlaysAsync_does_not_announce_the_old_round_after_a_new_game_in_the_trick_pause()
    {
        await ReachNorthNamingTrumpAsync();
        Assert.False(await _session.HandleTrumpSelectionAsync());

        // North leads and East follows; then the human plays, and West completes
        // the trick. The New Game lands in the pause after that trick.
        Assert.True(await _session.AdvanceAiPlaysAsync());
        Assert.True(_session.PlayHumanCard(_session.CurrentState!.GetLegalPlays()[0]));

        var newGames = 0;
        _session.TrickCompleted += (_, _) =>
        {
            newGames++;
            _session.StartNewGame();
        };
        var rounds = 0;
        _session.RoundCompleted += (_, _) => rounds++;

        Assert.False(await _session.AdvanceAiPlaysAsync());

        Assert.Equal(1, newGames);
        Assert.Equal(0, rounds);
        Assert.Equal(GamePhase.Bidding, _session.CurrentState.Phase);
    }



    [Fact]
    public async Task HandleHawseyExchangeAsync_stops_when_a_new_game_starts_during_its_pause()
    {
        // East calls Hawsey (injected strategy) and names trump; the New Game lands in
        // the pause before East's exchange.
        var session = TestGameSessions.Paced(ai: new TestPlayerStrategy(new Queue<BidAction>([BidAction.HawseyBid.Instance])));
        session.StartNewGame();
        Assert.False(await session.AdvanceAiBiddingAsync());
        Assert.False(await session.HandleTrumpSelectionAsync());
        Assert.Equal(GamePhase.HawseyExchange, session.CurrentState!.Phase);

        var exchanging = session.HandleHawseyExchangeAsync();
        Assert.False(exchanging.IsCompleted, "The AI step should be paused.");
        session.StartNewGame(HouseRules.Default);

        Assert.False(await exchanging);
        Assert.Equal(GamePhase.Bidding, session.CurrentState.Phase);
    }
}
