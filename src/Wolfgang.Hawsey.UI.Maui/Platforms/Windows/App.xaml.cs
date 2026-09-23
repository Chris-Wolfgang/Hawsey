namespace Wolfgang.Hawsey.UI.Maui.Platforms.Windows;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }



    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
