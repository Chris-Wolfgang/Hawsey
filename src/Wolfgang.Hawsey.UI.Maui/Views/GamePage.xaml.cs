using System.Diagnostics.CodeAnalysis;
using Wolfgang.Hawsey.UI.Maui.ViewModels;

namespace Wolfgang.Hawsey.UI.Maui.Views;

public partial class GamePage
{
    private const string DragIndexKey = "HandIndex";
    private const double HoverLift = -12;

    private readonly GameViewModel _viewModel;



    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();
        AbsoluteLayout.SetLayoutBounds(Table, new Rect(0, 0, TableScale.DesignWidth, TableScale.DesignHeight));
        _viewModel = viewModel;
        BindingContext = viewModel;
        TableHost.SizeChanged += (_, _) => OnHostSizeChanged();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, nameof(GameViewModel.IsCompactLayout), StringComparison.Ordinal))
            {
                PlacePanels();
            }
        };
    }



    private void OnHostSizeChanged()
    {
        FitTable(TableHost.Width, TableHost.Height);
        _viewModel.SetPageSize(TableHost.Width, TableHost.Height);
        PlacePanels();
    }



    /// <summary>Scales and centres the table to fit the space it has (<see cref="TableScale"/>).</summary>
    private void FitTable(double width, double height)
    {
        var scale = TableScale.Fit(width, height);
        Table.Scale = scale;
        Table.TranslationX = TableScale.Centre(width, scale, TableScale.DesignWidth);
        Table.TranslationY = TableScale.Centre(height, scale, TableScale.DesignHeight);
    }



    /// <summary>
    /// Puts the panels over the table's area when the table shows, so they sit where they
    /// do on the drawn table, or over the whole page in the compact layout.
    /// </summary>
    private void PlacePanels()
    {
        var bounds = _viewModel.IsCompactLayout
            ? new Rect(0, 0, TableHost.Width, TableHost.Height)
            : TableScale.Bounds(TableHost.Width, TableHost.Height);
        AbsoluteLayout.SetLayoutBounds(Panels, bounds);
    }



    // Dragging a card in the hand onto another moves it there, as in the Blazor UI. The
    // handlers only read the cards' positions; GameViewModel.MoveCard does the move.
    [ExcludeFromCodeCoverage(Justification = "Platform drag-and-drop callback; the move itself is GameViewModel.MoveCard, which is unit tested.")]
    private void OnCardDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CardViewModel card })
        {
            e.Data.Properties[DragIndexKey] = _viewModel.HumanCards.IndexOf(card);
        }
    }



    [ExcludeFromCodeCoverage(Justification = "Platform drag-and-drop callback; the move itself is GameViewModel.MoveCard, which is unit tested.")]
    private void OnCardDrop(object? sender, DropEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CardViewModel target }
            && e.Data.Properties.TryGetValue(DragIndexKey, out var value)
            && value is int from)
        {
            _viewModel.MoveCard(from, _viewModel.HumanCards.IndexOf(target));
        }
    }



    // A card the human can use lifts under the pointer (.hand .card:hover). A card picked
    // as a Hawsey discard is already raised by its trigger, so it is left alone.
    [ExcludeFromCodeCoverage(Justification = "Platform pointer callback: a hover effect only.")]
    private void OnCardPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is GestureRecognizer { Parent: View view, BindingContext: CardViewModel { IsInteractive: true, IsSelected: false } })
        {
            view.TranslationY = HoverLift;
        }
    }



    [ExcludeFromCodeCoverage(Justification = "Platform pointer callback: a hover effect only.")]
    private void OnCardPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is GestureRecognizer { Parent: View view, BindingContext: CardViewModel { IsSelected: false } })
        {
            view.TranslationY = 0;
        }
    }
}
