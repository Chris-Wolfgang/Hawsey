# ADR 0007 — The game session and AI live in the engine; each UI is unit-tested on a plain `net10.0` target

- **Status:** Accepted
- **Date:** 2026-09-27
- **Deciders:** Chris Wolfgang

## Context

The engine (ADR 0002) knows the rules, and `GameRunner` plays whole AI-only games.
A UI needs two more pieces of game logic:

- an AI player (`SimpleAiStrategy`);
- a live human-vs-AI session (`GameService`). It paces the AI moves, serialises human
  and AI moves on one turn, stops a stale AI loop after New Game, and raises events.

Both lived inside the MAUI app. The unfinished Blazor UI (`feature/blazor-ui`) had
grown its own copies in its `UI.Shared` project. Neither piece depends on a UI
framework.

Separately, the MAUI app's own logic (the view models) had never run under a test
(#129). The app targets only platform TFMs (`net10.0-android`, `-windows`, `-ios`,
`-maccatalyst`), so a `net10.0` test project couldn't reference it.

## Decision

**Game logic lives in the engine, and every UI uses it from there.**
- `SimpleAiStrategy` moves to `Wolfgang.Hawsey.Engine.Strategy`.
- The live session moves to `Wolfgang.Hawsey.Engine.Game` as **`GameSession`**, with
  its event args.
- Both ship in the NuGet package, and there is no separate shared library.
- Their tests are engine unit tests, run on every engine test TFM.

**Each UI is unit-tested on its own terms.** For MAUI, we follow Microsoft's
documented pattern:
- The app also targets plain **`net10.0`**, with `OutputType` `Exe` only on the
  platform TFMs. On that target the app builds as a library, which
  `tests/Wolfgang.Hawsey.UI.Maui.Tests.Unit` references.
- The tests cover the view models, the converters, and the app host: the app, shell
  and page XAML load, and dependency injection wires the page to its view model.
- `MainThread` isn't implemented on `net10.0`, so the view model posts UI updates
  through a small `IUiDispatcher`. The app's implementation uses `MainThread`; the
  tests run the action at once.
- `GameSession` has an internal constructor that turns pacing off and seeds the deal.
  The engine's `InternalsVisibleTo` covers the engine tests and the MAUI tests, so
  both can play whole games synchronously and repeatably.

## Alternatives considered

- **A separate shared library for the session, AI and view models.** Tried first,
  in the first version of #859. It put MAUI presentation logic (the view models) in
  a library meant for game logic, and added a second home for game logic next to
  the engine.
- **Keep linking source files into test projects.** A linked file compiles into
  every project that links it, so each UI would carry its own copy.

## Consequences

- `GameSession` and `SimpleAiStrategy` are public engine API. Adding them is a MINOR
  bump. They must build for `netstandard2.0`, which is why `MemberNotNullWhen` is
  added to the engine's PolySharp list.
- The Blazor UI can delete its `UI.Shared` copies and use the engine's `GameSession`.
- The MAUI app's `net10.0` target is built by every pr.yaml stage. The stages already
  run `dotnet workload restore` because of the platform TFMs.
- The Coyote concurrency tests rewrite the copy of the engine DLL in their own
  output folder as well as their test assembly, because `GameSession` now lives in
  the engine. The engine's own build output isn't touched.
