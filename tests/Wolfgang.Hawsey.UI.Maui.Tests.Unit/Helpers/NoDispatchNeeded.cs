using Microsoft.Maui.Dispatching;

namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.Helpers;

/// <summary>
/// MAUI's dispatcher for tests that lay out a page. A binding passes a property change
/// on through the dispatcher, and plain net10.0 has none, so a page whose bound view
/// model changes throws. This one tells the binding the test is already on the right
/// thread, so the change applies at once; it dispatches nothing itself.
/// </summary>
internal sealed class NoDispatchNeeded : IDispatcher, IDispatcherProvider
{
    public bool IsDispatchRequired => false;



    /// <summary>Makes this the dispatcher for every thread.</summary>
    public static void Install() => DispatcherProvider.SetCurrent(new NoDispatchNeeded());



    public IDispatcher GetForCurrentThread() => this;



    public bool Dispatch(Action action) => throw new NotSupportedException();



    public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();



    public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
}
