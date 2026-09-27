using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Wolfgang.Hawsey.UI.Maui.Converters;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.AppHost;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Converters;

/// <summary>
/// The value converters the pages bind through. The colour converters prefer the
/// app's resource colours and fall back to fixed colours when there is no
/// <see cref="Application.Current"/>, so both paths are pinned.
/// </summary>
[Collection(nameof(ApplicationCurrentCollection))]
public sealed class ConverterTests : IDisposable
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;



    public void Dispose()
    {
        Application.Current = null;
    }



    private static object? Convert(IValueConverter converter, object? value) =>
        converter.Convert(value, typeof(object), parameter: null, Culture);



    [Fact]
    public void BoolToOpacity_dims_cards_that_cannot_be_played()
    {
        var converter = new BoolToOpacityConverter();

        Assert.Equal(1.0, Convert(converter, true));
        Assert.Equal(0.5, Convert(converter, false));
        Assert.Equal(0.5, Convert(converter, null));
    }



    [Fact]
    public void Colour_converters_without_an_app_use_their_fallback_colours()
    {
        Application.Current = null;

        Assert.Equal(Colors.Green, Convert(new BoolToStrokeConverter(), true));
        Assert.Equal(Colors.Gray, Convert(new BoolToStrokeConverter(), false));
    }



    [Fact]
    public void Colour_converters_use_the_apps_resource_colours()
    {
        var app = new App();
        Application.Current = app;

        Assert.Equal(app.Resources["LegalPlayHighlight"], Convert(new BoolToStrokeConverter(), true));
        Assert.Equal(app.Resources["CardBorder"], Convert(new BoolToStrokeConverter(), false));
    }



    [Fact]
    public void No_converter_converts_back()
    {
        IValueConverter[] converters =
        [
            new BoolToOpacityConverter(),
            new BoolToStrokeConverter(),
        ];

        Assert.All
        (
            converters,
            c => Assert.Throws<NotSupportedException>(() => c.ConvertBack("x", typeof(object), null, Culture))
        );
    }
}
