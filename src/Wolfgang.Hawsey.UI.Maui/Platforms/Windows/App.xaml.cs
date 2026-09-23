// ReSharper disable once CheckNamespace
// Platforms/ is not a namespace provider, so InspectCode wants the root namespace
// Wolfgang.Hawsey.UI.Maui - but the shared cross-platform App already lives there
// and both compile into the Windows TFM. The MAUI template's .WinUI keeps them apart.
namespace Wolfgang.Hawsey.UI.Maui.WinUI;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }



    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
