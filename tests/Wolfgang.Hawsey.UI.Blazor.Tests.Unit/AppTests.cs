using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.UI.Blazor.Tests.Unit;

/// <summary>
/// The app's router: which page each address shows, and inside which layout.
/// </summary>
public sealed class AppTests : BunitContext
{
    public AppTests()
    {
        Services.AddScoped<GameSession>();
    }



    [Fact]
    public void Render_when_at_the_root_shows_the_game_page_in_the_main_layout()
    {
        var app = Render<App>();

        Assert.NotEmpty(app.FindAll(".statusbar"));
        Assert.Contains
        (
            app.FindAll("button"),
            b => b.TextContent.Contains("NEW GAME", StringComparison.Ordinal)
        );
    }



    [Fact]
    public void Render_when_the_address_matches_no_page_shows_the_not_found_page()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/no-such-page");

        var app = Render<App>();

        Assert.Equal
        (
            "Page not found.",
            app.Find("[role=alert]").TextContent.Trim()
        );
    }
}
