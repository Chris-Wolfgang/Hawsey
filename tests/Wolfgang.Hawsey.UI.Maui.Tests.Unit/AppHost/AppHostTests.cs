using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;
using Wolfgang.Hawsey.UI.Maui.ViewModels;
using Wolfgang.Hawsey.UI.Maui.Views;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.AppHost;

/// <summary>
/// The app host builds on the plain net10.0 target: the XAML of the app, shell and
/// game page loads, and dependency injection wires the page to its view model and
/// the view model to a single <see cref="GameSession"/>. A broken binding path,
/// resource key or registration fails here instead of on a device.
/// </summary>
[Collection(nameof(ApplicationCurrentCollection))]
public class AppHostTests
{
    /// <summary>Exposes the protected <see cref="App.CreateWindow"/>.</summary>
    private sealed class TestApp : App
    {
        public Window Create() => CreateWindow(null);
    }



    [Fact]
    public void App_loads_its_resources_and_opens_a_window_on_the_shell()
    {
        var app = new TestApp();

        var window = app.Create();

        Assert.IsType<AppShell>(window.Page);
        Assert.True(app.Resources.ContainsKey("SuitToColorConverter"));
        Assert.True(app.Resources.TryGetValue("RedSuitColor", out _));
    }



    [Fact]
    public void GamePage_loads_its_XAML_and_binds_the_view_model()
    {
        var vm = new GameViewModel(TestGameSessions.Unpaced(), new ImmediateDispatcher());

        var page = new GamePage(vm);

        Assert.Same(vm, page.BindingContext);
    }



    [Fact]
    public void CreateMauiApp_wires_the_page_view_model_and_one_game_session()
    {
        using var app = MauiProgram.CreateMauiApp();

        var page = app.Services.GetRequiredService<GamePage>();

        Assert.IsType<GameViewModel>(page.BindingContext);
        Assert.Same(app.Services.GetRequiredService<GameSession>(), app.Services.GetRequiredService<GameSession>());
    }
}
