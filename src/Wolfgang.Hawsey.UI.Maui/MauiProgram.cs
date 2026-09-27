using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.UI.Maui.Threading;
using Wolfgang.Hawsey.UI.Maui.ViewModels;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IUiDispatcher, MainThreadDispatcher>();
        builder.Services.AddSingleton<GameSession>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<GamePage>();

        return builder.Build();
    }
}
