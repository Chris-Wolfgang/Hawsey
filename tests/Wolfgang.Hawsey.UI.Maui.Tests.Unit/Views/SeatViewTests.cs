using Microsoft.Maui.Controls;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.AppHost;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Views;

/// <summary>A seat's placement picks which face-down hand shows and the team colour.</summary>
[Collection(nameof(ApplicationCurrentCollection))]
public sealed class SeatViewTests : IDisposable
{
    public void Dispose()
    {
        Application.Current = null;
    }



    [Theory]
    [InlineData(SeatPlacement.Top, true, false, true)]
    [InlineData(SeatPlacement.Bottom, false, false, true)]
    [InlineData(SeatPlacement.Left, false, true, false)]
    [InlineData(SeatPlacement.Right, false, true, false)]
    public void Placement_when_set_shows_the_seats_hand_and_team_colour(SeatPlacement placement, bool rowHand, bool columnHand, bool isUs)
    {
        // The view's XAML reads the app's resources, and its bindings need a dispatcher.
        NoDispatchNeeded.Install();
        Application.Current = new App();

        var seat = new SeatView { Placement = placement };

        Assert.Equal(placement, seat.Placement);
        Assert.Equal(rowHand, Assert.IsAssignableFrom<VisualElement>(seat.FindByName("RowHand")).IsVisible);
        Assert.Equal(columnHand, Assert.IsAssignableFrom<VisualElement>(seat.FindByName("ColumnHand")).IsVisible);
        Assert.Equal(isUs ? TableColors.UsBlue : TableColors.ThemRed, Assert.IsType<Label>(seat.FindByName("NameLabel")).TextColor);
    }
}
