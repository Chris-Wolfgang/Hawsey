using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Wolfgang.Hawsey.Engine.Tests.Unit")]

// The game page's bUnit tests use GameSession's internal constructor too (no AI pacing,
// seeded deal).
[assembly: InternalsVisibleTo("Wolfgang.Hawsey.UI.Shared.Tests.Unit")]

// GameSession's internal constructor (no AI pacing, seeded deal) lets the MAUI
// view-model tests drive whole games synchronously. That test project lives with the
// MAUI app on the dev/maui branch; the line stays here so merging main into dev/maui
// keeps it.
[assembly: InternalsVisibleTo("Wolfgang.Hawsey.UI.Maui.Tests.Unit")]
