using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Wolfgang.Hawsey.Engine.Tests.Unit")]

// GameSession's internal constructor (no AI pacing, seeded deal) lets the MAUI
// view-model tests drive whole games synchronously.
[assembly: InternalsVisibleTo("Wolfgang.Hawsey.UI.Maui.Tests.Unit")]
