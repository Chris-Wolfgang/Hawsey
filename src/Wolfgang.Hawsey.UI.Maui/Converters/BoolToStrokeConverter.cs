using System.Globalization;

namespace Wolfgang.Hawsey.UI.Maui.Converters;

public class BoolToStrokeConverter : IValueConverter
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
