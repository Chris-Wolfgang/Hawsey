using System.Runtime.CompilerServices;
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
        Assert.True(app.Resources.ContainsKey("BoolToStrokeConverter"));
        Assert.True(app.Resources.TryGetValue("LegalPlayHighlight", out _));
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



    [Fact]
    public void CreateMauiApp_when_a_page_is_created_again_gives_it_the_same_view_model()
    {
        // The page's XAML reads the app's resources, and its bindings need a dispatcher.
        NoDispatchNeeded.Install();
        Application.Current = new App();

        using var app = MauiProgram.CreateMauiApp();

        var first = app.Services.GetRequiredService<GamePage>();
        var second = app.Services.GetRequiredService<GamePage>();

        Assert.NotSame(first, second);
        Assert.Same(first.BindingContext, second.BindingContext);
        Assert.Same(app.Services.GetRequiredService<GameViewModel>(), first.BindingContext);
    }



    [Fact]
    public void GamePage_when_no_longer_referenced_is_not_kept_alive_by_the_view_model()
    {
        // The page's XAML reads the app's resources, and its bindings need a dispatcher.
        NoDispatchNeeded.Install();
        Application.Current = new App();

        var vm = new GameViewModel(TestGameSessions.Unpaced(), new ImmediateDispatcher());
        var page = CreateAndDropPage(vm);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(page.IsAlive);
        GC.KeepAlive(vm);
    }



    // A separate method, so no local in the test keeps the page reachable.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateAndDropPage(GameViewModel vm) => new(new GamePage(vm));
}
