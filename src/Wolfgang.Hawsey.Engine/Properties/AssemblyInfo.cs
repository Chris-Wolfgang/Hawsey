using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Wolfgang.Hawsey.Engine.Tests.Unit")]

// GameSession's internal constructor (no AI pacing, seeded deal) lets the MAUI
// view-model tests drive whole games synchronously. That test project lives with the
// MAUI app on the dev/maui branch; the line stays here so merging main into dev/maui
// keeps it.
[assembly: InternalsVisibleTo("Wolfgang.Hawsey.UI.Maui.Tests.Unit")]
