namespace Wolfgang.Hawsey.UI.Maui;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }



    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Big enough on a desktop for the whole table (the Blazor page fills the browser);
        // phones ignore the size.
        return new Window(new AppShell()) { Width = 1280, Height = 860 };
    }
}
