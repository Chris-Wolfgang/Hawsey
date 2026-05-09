using Microsoft.Extensions.Logging;
using Wolfgang.Hawsey.UI.Shared.Services;

namespace Wolfgang.Hawsey.UI.MauiHybrid;

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
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Same scoped service the WASM project registers — shared by the Razor pages.
        builder.Services.AddScoped<GameService>();

        return builder.Build();
    }
}
