using System.Globalization;

namespace Wolfgang.Hawsey.UI.Maui.Converters;

/// <summary>A hand card's border: the app's <c>LegalPlayHighlight</c> colour for a playable card, <c>CardBorder</c> otherwise. One-way.</summary>
public sealed class BoolToStrokeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return Application.Current?.Resources["LegalPlayHighlight"] as Color ?? Colors.Green;
        }

        return Application.Current?.Resources["CardBorder"] as Color ?? Colors.Gray;
    }



    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
