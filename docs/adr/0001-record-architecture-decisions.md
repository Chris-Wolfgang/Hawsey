# ADR 0001 — Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** Chris Wolfgang

## Context

Hawsey carries design choices the code alone doesn't explain: why the engine
never mutates state, why bowers are modelled as synthetic ranks, why house rules
are an options object instead of hard-coded play. Without a record, the reasoning
gets lost after the PR merges, and the next change re-argues the trade-off.

## Decision

We will keep short **Architecture Decision Records** in `docs/adr/`, one file per
non-obvious decision, using [Michael Nygard's format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
(see [`TEMPLATE.md`](TEMPLATE.md)). An Accepted ADR is immutable. A reversal is a
*new* ADR that supersedes it, and the old one is marked `Superseded by` rather than
deleted. A new decision lands **in the same PR** as the code that implements it,
so the ADR is reviewed with the code.

## Alternatives considered

- **Wiki or external docs** — not versioned with the code or reviewed in PRs, so they drift.
- **Long XML-doc comments** — good for *how to use*, poor for *why* and for rejected alternatives.
- **Nothing** — the rationale keeps disappearing, which is what this ADR is meant to stop.

## Consequences

Each non-obvious decision now costs a short document. In return,
[`index.md`](index.md) gives one list of what was decided and whether it still holds.
