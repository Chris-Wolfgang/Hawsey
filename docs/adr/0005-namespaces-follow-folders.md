# ADR 0005 — Engine namespaces follow the folder layout

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** Chris Wolfgang

## Context

The engine's files were split into folders (`Cards/`, `Bidding/`, `Game/`,
`Players/`, `Rules/`, `Scoring/`, `Strategy/`, `TrickPlay/`), but every type stayed
in the flat `Wolfgang.Hawsey.Engine` namespace. InspectCode raised
`CheckNamespace` on every file (30 code-scanning alerts). The flat namespace also
made the blanket `using Wolfgang.Hawsey.Engine;` in the tests redundant, which
raised 17 more alerts.

## Decision

Each type lives in `Wolfgang.Hawsey.Engine.<Folder>`, and each consumer imports
exactly the namespaces it uses (PR #824). There is one deliberate exception: the
MAUI Windows platform `App` stays in the template's `Wolfgang.Hawsey.UI.Maui.WinUI`,
with a single `// ReSharper disable once CheckNamespace` and a comment. InspectCode
wants the root namespace there, but the shared cross-platform `App` already lives
in it and both compile into the Windows TFM.

## Alternatives considered

- **Keep the flat namespace; mark folders as non-namespace-providers** — no
  consumer churn, but the folders would stop meaning anything to tooling.
- **Suppress `CheckNamespace` repository-wide** — hides the one case where the
  rule is right and new files that drift.

## Consequences

- This is a source-breaking change for engine consumers (their `using` lines
  change). It was accepted because the engine is pre-1.0 and unpublished, and it
  is recorded as a `breaking` changelog fragment.
- A new type goes in the folder that matches its namespace. `feature/blazor-ui`
  and any future UI import the sub-namespaces.
