using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.UI.Blazor.Tests.Unit.Hosting;

/// <summary>
/// What <see cref="HawseyHost.Configure"/> puts into a host's collections.
/// </summary>
public sealed class HawseyHostTests
{
    [Fact]
    public void Configure_when_called_maps_the_app_to_its_element_and_the_head_outlet_after_head()
    {
        var rootComponents = new RootComponentMappingCollection();

        HawseyHost.Configure(rootComponents, new ServiceCollection());

        Assert.Equal
        (
            new[] { (typeof(App), "#app"), (typeof(HeadOutlet), "head::after") },
            rootComponents.Select(m => (m.ComponentType, m.Selector)).ToArray()
        );
    }



    [Fact]
    public void Configure_when_called_registers_one_game_session_per_scope()
    {
        var services = new ServiceCollection();

        HawseyHost.Configure(new RootComponentMappingCollection(), services);

        var registration = Assert.Single(services, d => d.ServiceType == typeof(GameSession));
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
