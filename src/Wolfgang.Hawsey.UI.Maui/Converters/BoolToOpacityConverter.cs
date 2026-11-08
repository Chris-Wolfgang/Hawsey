using System.Globalization;

namespace Wolfgang.Hawsey.UI.Maui.Converters;

/// <summary>Dims a hand card that does nothing when tapped: 1 for <see langword="true"/>, 0.55 otherwise. One-way.</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return 1.0;
        }

        return 0.55;
    }



    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
