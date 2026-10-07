using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Views;

/// <summary>
/// Auto picks the compact layout where the table would scale below three-quarters, except
/// on a desktop; the human's own choice wins whatever the size or device.
/// </summary>
public class LayoutPickerTests
{
    [Theory]
    [InlineData(915, 412, true)]    // Pixel 7 in landscape: the table at 0.50
    [InlineData(640, 360, true)]    // a small phone in landscape: 0.44
    [InlineData(1280, 800, false)]  // a tablet in landscape: 0.98
    [InlineData(1920, 1080, false)] // a desktop window: 1
    public void UseCompact_when_Auto_picks_from_the_table_scale(double width, double height, bool expected)
    {
        Assert.Equal(expected, LayoutPicker.UseCompact(width, height, LayoutChoice.Auto));
    }



    [Theory]
    [InlineData(915, 412)]
    [InlineData(640, 360)]
    public void UseCompact_when_Auto_on_a_desktop_keeps_the_table_at_any_size(double width, double height)
    {
        Assert.False(LayoutPicker.UseCompact(width, height, LayoutChoice.Auto, onDesktop: true));
    }



    [Fact]
    public void UseCompact_when_Auto_keeps_the_table_at_exactly_the_threshold()
    {
        var width = TableScale.DesignWidth * LayoutPicker.CompactBelowScale;
        var height = TableScale.DesignHeight * LayoutPicker.CompactBelowScale;

        Assert.False(LayoutPicker.UseCompact(width, height, LayoutChoice.Auto));
    }



    [Fact]
    public void UseCompact_when_Auto_before_the_page_has_a_size_keeps_the_table()
    {
        Assert.False(LayoutPicker.UseCompact(0, 0, LayoutChoice.Auto));
    }



    [Theory]
    [InlineData(640, 360)]
    [InlineData(1920, 1080)]
    public void UseCompact_follows_the_humans_choice_whatever_the_size(double width, double height)
    {
        Assert.True(LayoutPicker.UseCompact(width, height, LayoutChoice.Compact));
        Assert.False(LayoutPicker.UseCompact(width, height, LayoutChoice.Table));
        Assert.True(LayoutPicker.UseCompact(width, height, LayoutChoice.Compact, onDesktop: true));
    }
}
