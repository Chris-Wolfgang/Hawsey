using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.AppHost;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Views;

/// <summary>
/// The table is laid out at its design size and scaled down, never up, to fit the page,
/// then centred.
/// </summary>
[Collection(nameof(ApplicationCurrentCollection))]
public class TableScaleTests
{
    [Theory]
    [InlineData(TableScale.DesignWidth, TableScale.DesignHeight, 1.0)]
    [InlineData(TableScale.DesignWidth * 2, TableScale.DesignHeight * 2, 1.0)]
    [InlineData(TableScale.DesignWidth / 2, TableScale.DesignHeight, 0.5)]
    [InlineData(TableScale.DesignWidth, TableScale.DesignHeight / 4, 0.25)]
    public void Fit_scales_down_to_the_tighter_side_and_never_up(double width, double height, double expected)
    {
        Assert.Equal(expected, TableScale.Fit(width, height), precision: 6);
    }



    [Theory]
    [InlineData(0, 400)]
    [InlineData(400, 0)]
    [InlineData(-1, -1)]
    public void Fit_before_the_page_has_a_size_is_one(double width, double height)
    {
        Assert.Equal(1.0, TableScale.Fit(width, height));
    }



    [Fact]
    public void Centre_splits_the_spare_space_evenly()
    {
        Assert.Equal(100, TableScale.Centre(available: 832, scale: 0.5, designSize: 1264));
    }



    [Fact]
    public void GamePage_scales_and_centres_the_table_when_it_is_sized()
    {
        // The page's XAML reads the app's resources.
        Application.Current = new App();
        var page = new GamePage(new GameViewModel(TestGameSessions.Unpaced(), new ImmediateDispatcher()));
        var host = Assert.IsType<AbsoluteLayout>(page.Content);
        var table = Assert.IsAssignableFrom<VisualElement>(host.Children[0]);

        // The table's host is given a phone in landscape: half the design height, and
        // more than half its width.
        IView view = host;
        view.Measure(900, TableScale.DesignHeight / 2);
        view.Arrange(new Rect(0, 0, 900, TableScale.DesignHeight / 2));

        Assert.Equal(0.5, table.Scale, precision: 6);
        Assert.Equal((900 - (TableScale.DesignWidth / 2)) / 2, table.TranslationX, precision: 6);
        Assert.Equal(0, table.TranslationY, precision: 6);
        Assert.Equal(new Rect(0, 0, TableScale.DesignWidth, TableScale.DesignHeight), AbsoluteLayout.GetLayoutBounds(table));
    }
}
