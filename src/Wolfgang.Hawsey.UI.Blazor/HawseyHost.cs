using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.UI.Blazor;

/// <summary>
/// What the WebAssembly host is made of: its root components and its services.
/// <c>Program.cs</c> only creates the host and runs it, which needs a browser; this part
/// needs nothing but the two collections, so tests check it directly.
/// </summary>
internal static class HawseyHost
{
    /// <summary>
    /// Adds the app's root components and registers its services.
    /// </summary>
    /// <param name="rootComponents">The host's root component mappings.</param>
    /// <param name="services">The host's service collection.</param>
    internal static void Configure(RootComponentMappingCollection rootComponents, IServiceCollection services)
    {
        rootComponents.Add<App>("#app");
        rootComponents.Add<HeadOutlet>("head::after");

        // The engine's game session: the same game logic every Hawsey UI uses (ADR 0007).
        services.AddScoped<GameSession>();
    }
}
