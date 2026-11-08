using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.UI.Maui.Threading;
using Wolfgang.Hawsey.UI.Maui.ViewModels;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui;

/// <summary>Builds the app: the shell, and the game page wired to one game session and view model.</summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<IUiDispatcher, MainThreadDispatcher>();
        builder.Services.AddSingleton<GameSession>();

        // One view model for the one session: it subscribes to the session's StateChanged
        // for good, so a view model per page would keep every old page's alive. A page
        // holds the view model only through its bindings, which hold it weakly.
        builder.Services.AddSingleton<GameViewModel>();
        builder.Services.AddTransient<GamePage>();

        return builder.Build();
    }
}
