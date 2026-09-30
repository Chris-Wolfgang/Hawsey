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
        _viewModel = viewModel;
        BindingContext = viewModel;
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
