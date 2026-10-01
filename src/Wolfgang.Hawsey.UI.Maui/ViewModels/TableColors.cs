namespace Wolfgang.Hawsey.UI.Maui.ViewModels;

/// <summary>
/// Colours the view models choose between, from the Blazor UI's site.css. (Colours the
/// XAML picks with a trigger live in Resources/Styles/CardStyles.xaml instead.)
/// </summary>
public static class TableColors
{
    /// <summary>The human's team (--us-blue).</summary>
    public static Color UsBlue { get; } = Color.FromArgb("#4A9EFF");

    /// <summary>The opponents, a bid that went set, a lost score (--them-red).</summary>
    public static Color ThemRed { get; } = Color.FromArgb("#E84A4A");

    /// <summary>A made bid, a score won.</summary>
    public static Color Positive { get; } = Color.FromArgb("#6CF06C");

    /// <summary>The ink of hearts and diamonds on a card face.</summary>
    public static Color CardRed { get; } = Color.FromArgb("#C0202A");

    /// <summary>The ink of clubs and spades on a card face.</summary>
    public static Color CardBlack { get; } = Color.FromArgb("#111111");

    /// <summary>The trump watermark for hearts and diamonds.</summary>
    public static Color RedTrumpMark { get; } = Color.FromArgb("#C41A1A");

    /// <summary>The trump watermark for clubs and spades.</summary>
    public static Color BlackTrumpMark { get; } = Color.FromArgb("#0A0A0A");
}
