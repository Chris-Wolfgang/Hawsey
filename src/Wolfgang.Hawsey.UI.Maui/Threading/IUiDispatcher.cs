namespace Wolfgang.Hawsey.UI.Maui.Threading;

/// <summary>
/// Runs work on the UI thread. <see cref="ViewModels.GameViewModel"/> updates its
/// properties through this, because <see cref="Engine.Game.GameSession"/> raises its
/// events from background continuations.
/// </summary>
/// <remarks>
/// A seam for the unit tests: the app's <see cref="MainThreadDispatcher"/> uses
/// <c>MainThread</c>, which is not implemented on the plain net10.0 target the tests
/// run on, so the tests supply one that runs the action at once.
/// </remarks>
public interface IUiDispatcher
{
    /// <summary>
    /// Queues <paramref name="action"/> to run on the UI thread.
    /// </summary>
    /// <param name="action">The work to run.</param>
    void Post(Action action);
}
