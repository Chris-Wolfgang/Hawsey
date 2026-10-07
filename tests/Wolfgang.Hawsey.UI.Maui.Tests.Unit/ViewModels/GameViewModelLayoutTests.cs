using System.ComponentModel;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.ViewModels;

/// <summary>
/// The page's layout: picked from the page's size until the human flips it, and kept
/// through later resizes once they have.
/// </summary>
public class GameViewModelLayoutTests
{
    private readonly GameViewModel _vm = new(TestGameSessions.Unpaced(), new ImmediateDispatcher());



    [Fact]
    public void IsCompactLayout_before_the_page_has_a_size_is_false()
    {
        Assert.False(_vm.IsCompactLayout);
        Assert.True(_vm.IsTableLayout);
    }



    [Theory]
    [InlineData(915, 412, true)]
    [InlineData(1280, 800, false)]
    public void SetPageSize_picks_the_layout_from_the_size(double width, double height, bool compact)
    {
        _vm.SetPageSize(width, height);

        Assert.Equal(compact, _vm.IsCompactLayout);
        Assert.Equal(!compact, _vm.IsTableLayout);
    }



    [Fact]
    public void ToggleLayoutCommand_on_a_phone_shows_the_table_and_keeps_it_through_a_resize()
    {
        _vm.SetPageSize(915, 412);

        _vm.ToggleLayoutCommand.Execute(null);
        _vm.SetPageSize(900, 400);

        Assert.True(_vm.IsTableLayout);
    }



    [Fact]
    public void ToggleLayoutCommand_on_a_tablet_shows_the_compact_layout_and_keeps_it_through_a_resize()
    {
        _vm.SetPageSize(1280, 800);

        _vm.ToggleLayoutCommand.Execute(null);
        _vm.SetPageSize(1920, 1080);

        Assert.True(_vm.IsCompactLayout);
    }



    [Fact]
    public void ToggleLayoutCommand_twice_returns_to_the_first_layout()
    {
        _vm.SetPageSize(915, 412);

        _vm.ToggleLayoutCommand.Execute(null);
        _vm.ToggleLayoutCommand.Execute(null);

        Assert.True(_vm.IsCompactLayout);
    }



    [Fact]
    public void IsCompactLayout_when_it_changes_raises_both_layout_properties()
    {
        var raised = new List<string?>();
        ((INotifyPropertyChanged)_vm).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _vm.SetPageSize(915, 412);

        Assert.Equal
        (
            new[] { nameof(GameViewModel.IsCompactLayout), nameof(GameViewModel.IsTableLayout) },
            raised
        );
    }



    [Fact]
    public void SetPageSize_when_the_layout_does_not_change_raises_nothing()
    {
        _vm.SetPageSize(915, 412);
        var raised = new List<string?>();
        ((INotifyPropertyChanged)_vm).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _vm.SetPageSize(900, 400);

        Assert.Empty(raised);
    }
}
