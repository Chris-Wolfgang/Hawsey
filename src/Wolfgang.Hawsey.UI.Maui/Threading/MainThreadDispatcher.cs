using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Hawsey.UI.Maui.Threading;

/// <summary>
/// The app's <see cref="IUiDispatcher"/>: queues work on the main thread.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "One-line adapter to MainThread, which is not implemented on the net10.0 target the unit tests run on; the tests substitute their own IUiDispatcher.")]
public sealed class MainThreadDispatcher : IUiDispatcher
{
    /// <inheritdoc/>
    public void Post(Action action) => MainThread.BeginInvokeOnMainThread(action);
}
