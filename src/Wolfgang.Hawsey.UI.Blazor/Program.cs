using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Wolfgang.Hawsey.UI.Blazor;
using Wolfgang.Hawsey.Engine.Game;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The engine's game session: the same game logic the MAUI app uses (ADR 0007).
builder.Services.AddScoped<GameSession>();

await builder.Build().RunAsync().ConfigureAwait(false);
