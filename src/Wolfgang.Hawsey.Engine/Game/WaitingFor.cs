namespace Wolfgang.Hawsey.Engine.Game;

/// <summary>
/// What a <see cref="GameSession"/> is waiting for once <see cref="GameSession.AdvanceAsync"/>
/// has played every AI move it can: the move a UI must now ask the human for, or the
/// point where the UI decides what happens next.
/// </summary>
public enum WaitingFor
{
    /// <summary>
    /// Nothing: no game has started, or a newer move (such as a New Game) took over
    /// while this advance was paused. The UI has nothing to show for this call.
    /// </summary>
    Nothing,

    /// <summary>The human bids: <see cref="GameSession.PlaceHumanBid"/>.</summary>
    HumanBid,

    /// <summary>The human, having won the bid, names trump: <see cref="GameSession.SelectTrump"/>.</summary>
    HumanTrump,

    /// <summary>
    /// The human, having bid Hawsey, picks two discards:
    /// <see cref="GameSession.PerformHumanHawseyExchange"/>.
    /// </summary>
    HumanHawseyExchange,

    /// <summary>
    /// The human plays a card: <see cref="GameSession.PlayHumanCard"/>. Never asked for
    /// the last card of a round, which the session plays itself (there is no choice).
    /// </summary>
    HumanCard,

    /// <summary>
    /// The round is scored (<see cref="GameState.RoundScore"/>); the UI starts the next
    /// one with <see cref="GameSession.StartNextRound"/> when it is ready.
    /// </summary>
    NextRound,

    /// <summary>The game is over (<see cref="GameState.Winner"/>).</summary>
    GameOver,
}
