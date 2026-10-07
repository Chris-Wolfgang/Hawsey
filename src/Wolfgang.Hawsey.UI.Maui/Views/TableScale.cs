namespace Wolfgang.Hawsey.UI.Maui.Views;

/// <summary>
/// Fits the game table to the screen. The table is laid out once, at a fixed design size
/// (the desktop window it was drawn for, like the Blazor page), and then scaled down as a
/// whole to fit a smaller screen, such as a phone in landscape, and centred. It is never
/// scaled up, so a large window shows it at its drawn size.
/// </summary>
public static class TableScale
{
    /// <summary>The width the table is laid out at, in device-independent units.</summary>
    public const double DesignWidth = 1264;

    /// <summary>The height the table is laid out at, in device-independent units.</summary>
    public const double DesignHeight = 820;



    /// <summary>
    /// The scale that fits the whole table in <paramref name="width"/> by
    /// <paramref name="height"/>: at most 1, and 1 before the page has a size.
    /// </summary>
    public static double Fit(double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            return 1;
        }

        return Math.Min(1, Math.Min(width / DesignWidth, height / DesignHeight));
    }



    /// <summary>
    /// The offset that centres a scaled table of <paramref name="designSize"/> in
    /// <paramref name="available"/> space (negative when the page is not yet sized).
    /// </summary>
    public static double Centre(double available, double scale, double designSize) =>
        (available - (designSize * scale)) / 2;



    /// <summary>
    /// Where the scaled, centred table sits in <paramref name="width"/> by
    /// <paramref name="height"/>, in unscaled units: the area the page's panels are
    /// placed over, so they sit where they would on the drawn table, at full size.
    /// </summary>
    public static Rect Bounds(double width, double height)
    {
        var scale = Fit(width, height);
        return new Rect
        (
            Centre(width, scale, DesignWidth),
            Centre(height, scale, DesignHeight),
            DesignWidth * scale,
            DesignHeight * scale
        );
    }
}
