# ADR 0002 — A UI-agnostic engine with immutable state transitions

- **Status:** Accepted
- **Date:** 2026-09-22 (records a decision made at the engine's creation)
- **Deciders:** Chris Wolfgang

## Context

Hawsey has, or plans, several front ends: a MAUI XAML app, plus (on
`feature/blazor-ui`) a Blazor WebAssembly app and a MAUI Blazor Hybrid (Android) host. AI
players, replays and whole-game simulation (`GameRunner`) need the same rules
without a UI. If rules live in a view model, every front end re-implements them
and they drift apart.

## Decision

All game rules live in `Wolfgang.Hawsey.Engine`, a UI-free library that targets
`netstandard2.0` and `net10.0`. The engine is a **pure state machine**: each
`GameEngine` operation (`PlaceBid`, `SelectTrump`, `ExchangeHawseyCards`,
`PlayCard`, `StartNextRound`) takes a `GameState` and returns a **new** one,
never changing its input. Decisions come in through the `IPlayerStrategy` seam,
so a human UI, an AI and a test double are interchangeable.

## Alternatives considered

- **A mutable `Game` object with events** — convenient for data binding, but
  replay, undo and "what if" AI search would all need defensive copies, and
  mutation order becomes a hidden source of bugs.
- **Rules inside each UI's view model** — the fastest start, but the rules would
  exist once per UI and drift apart.

## Consequences

- A front end holds the current `GameState` and swaps in the returned one. Old
  states remain valid snapshots, which gives replay and logging for free.
- Every transition allocates a new state. That is irrelevant at card-game speed.
- For zero-copy construction, `GameState` exposes `Dictionary<,>` and `List<>`.
  Callers must treat them as read-only; this is documented on the type. Moving
  to read-only abstractions is a candidate for a superseding ADR.
- Each UI keeps only presentation and turn pacing (for example the delay that
  keeps a finished trick visible).
