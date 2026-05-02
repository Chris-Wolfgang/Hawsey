using System.Globalization;
using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.Converters;

public class SuitToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Suit suit)
        {
            return suit.IsRed()
                ? Application.Current?.Resources["RedSuitColor"] as Color ?? Colors.Red
                : Application.Current?.Resources["BlackSuitColor"] as Color ?? Colors.Black;
        }

        return Colors.Black;
    }



    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
