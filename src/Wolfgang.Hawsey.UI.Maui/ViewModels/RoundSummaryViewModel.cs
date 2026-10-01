using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// The panel shown when a round is scored: who bid what and whether they made it, and
/// each team's tricks, points this round and total. At game over it names the winner
/// and offers a new game instead of the next round. The same summary as the Blazor UI.
/// </summary>
public sealed class RoundSummaryViewModel
{
    private RoundSummaryViewModel
    (
        bool isGameOver,
        string title,
        bool isTitleUs,
        string bidTeamLabel,
        string bidText,
        bool made,
        int usTricks,
        int themTricks,
        int usDelta,
        int themDelta,
        int usTotal,
        int themTotal
    )
    {
        IsGameOver = isGameOver;
        Title = title;
        IsTitleUs = isTitleUs;
        BidTeamLabel = bidTeamLabel;
        BidText = bidText;
        Made = made;
        UsTricks = usTricks;
        ThemTricks = themTricks;
        UsDelta = usDelta;
        ThemDelta = themDelta;
        UsTotal = usTotal;
        ThemTotal = themTotal;
    }



    public bool IsGameOver { get; }



    /// <summary>"Round complete", or "US wins the game!" / "THEM wins the game!".</summary>
    public string Title { get; }



    /// <summary>Whether the title names the human's team (it shows in their colour).</summary>
    public bool IsTitleUs { get; }



    /// <summary>"US" or "THEM": the team that held the bid.</summary>
    public string BidTeamLabel { get; }



    public bool IsBidTeamUs => string.Equals(BidTeamLabel, "US", StringComparison.Ordinal);



    /// <summary>The bid: a number of tricks, or "Hawsey (24)".</summary>
    public string BidText { get; }



    /// <summary>Whether the bidding team made its bid (otherwise it went set).</summary>
    public bool Made { get; }



    public string ResultText => Made ? "made it" : "went set";



    /// <summary>The bidding team's label colour: blue for US, red for THEM.</summary>
    public Color BidTeamColor => IsBidTeamUs ? TableColors.UsBlue : TableColors.ThemRed;



    /// <summary>Green for "made it", red for "went set".</summary>
    public Color ResultColor => Made ? TableColors.Positive : TableColors.ThemRed;



    public Color UsDeltaColor => UsDelta < 0 ? TableColors.ThemRed : TableColors.Positive;
    public Color ThemDeltaColor => ThemDelta < 0 ? TableColors.ThemRed : TableColors.Positive;



    public int UsTricks { get; }
    public int ThemTricks { get; }
    public int UsDelta { get; }
    public int ThemDelta { get; }
    public string UsDeltaText => FormatDelta(UsDelta);
    public string ThemDeltaText => FormatDelta(ThemDelta);
    public int UsTotal { get; }
    public int ThemTotal { get; }



    /// <summary>
    /// The summary of the round just scored, from the engine's own
    /// <see cref="GameState.RoundScore"/> and <see cref="GameState.Winner"/>: nothing is
    /// recounted here.
    /// </summary>
    public static RoundSummaryViewModel From(GameState state, bool isGameOver)
    {
        ArgumentNullException.ThrowIfNull(state);

        var score = state.RoundScore;
        var bidTeam = score?.BiddingTeam ?? Team.NorthSouth;
        var isHawsey = score?.IsHawsey ?? false;
        var bidText = isHawsey
            ? "Hawsey (24)"
            : (score?.BidAmount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture);

        var usWon = state.Winner == Team.NorthSouth;
        var winnerLabel = usWon ? "US" : "THEM";
        var title = isGameOver && state.Winner is not null ? $"{winnerLabel} wins the game!" : "Round complete";

        return new RoundSummaryViewModel
        (
            isGameOver,
            title,
            isGameOver && usWon,
            bidTeam == Team.NorthSouth ? "US" : "THEM",
            bidText,
            score is { BiddingTeamDelta: > 0 },
            score?.TricksFor(Team.NorthSouth) ?? 0,
            score?.TricksFor(Team.EastWest) ?? 0,
            score?.DeltaFor(Team.NorthSouth) ?? 0,
            score?.DeltaFor(Team.EastWest) ?? 0,
            state.NorthSouthScore,
            state.EastWestScore
        );
    }



    private static string FormatDelta(int n) =>
        n > 0 ? $"+{n}" : n.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
