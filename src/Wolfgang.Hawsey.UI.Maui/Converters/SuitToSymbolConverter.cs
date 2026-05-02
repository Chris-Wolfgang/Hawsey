using System.Globalization;
using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.UI.Maui.Converters;

public class SuitToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "\u2665",
                Suit.Diamonds => "\u2666",
                Suit.Clubs => "\u2663",
                Suit.Spades => "\u2660",
                _ => "?"
            };
        }

        return "?";
    }



    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
