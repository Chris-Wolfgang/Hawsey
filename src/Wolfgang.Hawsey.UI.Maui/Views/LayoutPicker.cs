namespace Wolfgang.Hawsey.UI.Maui.Views;

/// <summary>
/// Picks the game page's layout. The table is drawn for a desktop window and scaled down
/// to fit (<see cref="TableScale"/>); on a phone in landscape that leaves its cards and
/// buttons too small to tap, so below <see cref="CompactBelowScale"/> the page shows the
/// compact layout instead. The human can pick either layout on any device.
/// </summary>
public static class LayoutPicker
{
    /// <summary>
    /// The table scale below which <see cref="LayoutChoice.Auto"/> picks the compact
    /// layout. A phone in landscape scales the table to about half; a tablet keeps it
    /// at nine-tenths or more.
    /// </summary>
    public const double CompactBelowScale = 0.75;



    /// <summary>
    /// Whether the page shows the compact layout in <paramref name="width"/> by
    /// <paramref name="height"/> for the human's <paramref name="choice"/>. Before the
    /// page has a size, <see cref="LayoutChoice.Auto"/> keeps the table.
    /// </summary>
    public static bool UseCompact(double width, double height, LayoutChoice choice) => choice switch
    {
        LayoutChoice.Table => false,
        LayoutChoice.Compact => true,
        _ => TableScale.Fit(width, height) < CompactBelowScale,
    };
}
