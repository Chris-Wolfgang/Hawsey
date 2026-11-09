using System.Globalization;
using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>How a bid is written, with its numbers from the engine, as in the Blazor UI.</summary>
public static class BidText
{
    /// <summary>The Hawsey bid with the points it is worth, for example "Hawsey (24)".</summary>
    public static string Hawsey { get; } =
        $"Hawsey ({GameState.HawseyBidAmount.ToString(CultureInfo.InvariantCulture)})";



    /// <summary>A winning bid: <see cref="Hawsey"/> for a Hawsey bid, otherwise its number of tricks.</summary>
    public static string For(bool isHawsey, int amount) =>
        isHawsey ? Hawsey : amount.ToString(CultureInfo.InvariantCulture);
}
