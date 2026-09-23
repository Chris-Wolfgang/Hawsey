# ADR 0002 — A UI-agnostic engine that returns a new GameState per move

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
`netstandard2.0` and `net10.0`. Each `GameEngine` operation (`PlaceBid`,
`SelectTrump`, `ExchangeHawseyCards`, `PlayCard`, `StartNextRound`) takes the
current `GameState` and returns a **new** `GameState`. A front end never changes
game state itself: it calls the engine and keeps the result.

The engine is **not fully immutable**, and this ADR doesn't claim it is:

- `PlayCard` plays onto the `Trick` object held by the incoming state
  (`state.CurrentTrick.Play(...)`) before building the returned state, so the
  previous `GameState` and the new one share that trick.
- `PlaceBid` advances a caller-owned, mutable `BiddingPhase`.
- `Hands` and `CompletedTricks` are exposed as `Dictionary<,>` / `List<>`. Callers
  must treat them as read-only (documented on `GameState`).

Decisions reach the engine in two ways. A UI calls the `GameEngine` methods
directly when it's the human's turn and asks its AI for the others (the MAUI
`GameService` works like this). `GameRunner` drives a whole game through the
`IPlayerStrategy` seam, which is how simulations and tests plug in players.

## Alternatives considered

- **A mutable `Game` object with events** — convenient for data binding, but
  replay, undo and "what if" AI search would all need defensive copies, and
  mutation order becomes a hidden source of bugs.
- **Rules inside each UI's view model** — the fastest start, but the rules would
  exist once per UI and drift apart.

## Consequences

- The only safe snapshot is the latest `GameState`. A retained older state can
  change under you (its shared `Trick`), so replay has to come from a move log,
  not from saved states.
- Every UI keeps only presentation and turn pacing (for example the delay that
  keeps a finished trick visible), and every UI plays by the same rules.
- Making `Trick`, `BiddingPhase` and the collections copy-on-write would make
  states true snapshots. That change would be a new ADR superseding this one.
