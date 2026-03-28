namespace Wolfgang.Hawsey.UI.Maui.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }



    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
