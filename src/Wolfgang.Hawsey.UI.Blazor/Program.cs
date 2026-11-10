using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Wolfgang.Hawsey.UI.Blazor;

/// <summary>
/// The WebAssembly entry point: creates the host, configures it and runs it.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Starts the app in the browser.
    /// </summary>
    /// <param name="args">The command-line arguments the WebAssembly runtime passes in.</param>
    /// <returns>A task that completes when the host stops.</returns>
    // Browser-only: outside a browser WebAssemblyHostBuilder.CreateDefault throws
    // PlatformNotSupportedException, so no test can run these lines. What they configure
    // lives in HawseyHost.Configure, which is tested.
    [ExcludeFromCodeCoverage(Justification = "Browser-only host entry point; its configuration is in HawseyHost.Configure, which is tested.")]
    private static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        HawseyHost.Configure(builder.RootComponents, builder.Services);

        return builder.Build().RunAsync();
    }
}
