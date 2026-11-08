using Microsoft.Maui.Dispatching;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;

/// <summary>The test dispatcher only says no dispatch is needed; anything else is a test bug.</summary>
public class NoDispatchNeededTests
{
    private readonly NoDispatchNeeded _dispatcher = new();



    [Fact]
    public void IsDispatchRequired_when_read_is_false_and_the_dispatcher_serves_every_thread()
    {
        Assert.False(_dispatcher.IsDispatchRequired);
        Assert.Same(_dispatcher, _dispatcher.GetForCurrentThread());
    }



    [Fact]
    public void Dispatch_when_called_throws()
    {
        Assert.Throws<NotSupportedException>(() => _dispatcher.Dispatch(() => { }));
        Assert.Throws<NotSupportedException>(() => _dispatcher.DispatchDelayed(TimeSpan.Zero, () => { }));
        Assert.Throws<NotSupportedException>(() => ((IDispatcher)_dispatcher).CreateTimer());
    }
}
