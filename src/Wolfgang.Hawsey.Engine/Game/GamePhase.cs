namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents the current phase of a Hawsey game.
/// </summary>
public enum GamePhase
{
    /// <summary>Cards are being dealt.</summary>
    Dealing,

    /// <summary>Players are bidding.</summary>
    Bidding,

    /// <summary>The bid winner is selecting trump or ace high.</summary>
    TrumpSelection,

    /// <summary>Hawsey card exchange is in progress.</summary>
    HawseyExchange,

    /// <summary>Tricks are being played.</summary>
    TrickPlay,

    /// <summary>The round has ended and scoring is being applied.</summary>
    RoundScoring,

    /// <summary>The game is over — a team has won.</summary>
    GameOver
}
