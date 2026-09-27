namespace Wolfgang.Hawsey.UI.Maui.Tests.Unit.AppHost;

/// <summary>
/// Tests that construct an <see cref="App"/> or read <see cref="Microsoft.Maui.Controls.Application.Current"/>
/// share that static, so they run one at a time.
/// </summary>
[CollectionDefinition(nameof(ApplicationCurrentCollection), DisableParallelization = true)]
public sealed class ApplicationCurrentCollection
{
}
