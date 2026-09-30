using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Views;

public partial class SeatView
{
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create
    (
        nameof(Placement),
        typeof(SeatPlacement),
        typeof(SeatView),
        SeatPlacement.Bottom,
        propertyChanged: (bindable, _, _) => ((SeatView)bindable).ApplyPlacement()
    );



    public SeatView()
    {
        InitializeComponent();
        ApplyPlacement();
    }



    /// <summary>Where the seat sits, which decides its layout and team colour.</summary>
    public SeatPlacement Placement
    {
        get => (SeatPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }



    // The offsets are site.css's: the puck 54px and the badge 28px (above / below) or
    // 90px (beside) out from the seat, on the table side.
    private void ApplyPlacement()
    {
        var placement = Placement;
        var isUs = placement is SeatPlacement.Top or SeatPlacement.Bottom;
        var teamColor = isUs ? TableColors.UsBlue : TableColors.ThemRed;

        Plate.Stroke = teamColor;
        Avatar.Stroke = teamColor;
        NameLabel.TextColor = teamColor;

        RowHand.IsVisible = placement == SeatPlacement.Top;
        ColumnHand.IsVisible = placement is SeatPlacement.Left or SeatPlacement.Right;

        switch (placement)
        {
            case SeatPlacement.Top:
                Place(Puck, LayoutOptions.Center, LayoutOptions.End, 0, 54);
                Place(Badge, LayoutOptions.Center, LayoutOptions.End, 0, 28);
                break;

            case SeatPlacement.Bottom:
                Place(Puck, LayoutOptions.Center, LayoutOptions.Start, 0, -54);
                Place(Badge, LayoutOptions.Center, LayoutOptions.Start, 0, -28);
                break;

            case SeatPlacement.Left:
                Place(Puck, LayoutOptions.End, LayoutOptions.Center, 54, 0);
                Place(Badge, LayoutOptions.End, LayoutOptions.Center, 90, 0);
                break;

            default:
                Place(Puck, LayoutOptions.Start, LayoutOptions.Center, -54, 0);
                Place(Badge, LayoutOptions.Start, LayoutOptions.Center, -90, 0);
                break;
        }
    }



    private static void Place(View view, LayoutOptions horizontal, LayoutOptions vertical, double x, double y)
    {
        view.HorizontalOptions = horizontal;
        view.VerticalOptions = vertical;
        view.TranslationX = x;
        view.TranslationY = y;
    }
}
