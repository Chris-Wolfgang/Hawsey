# Architecture Decision Records

This folder records the non-obvious design decisions behind Hawsey: the *why*
that the code and type signatures can't show. See
[ADR-0001](0001-record-architecture-decisions.md) for the practice, and
[`TEMPLATE.md`](TEMPLATE.md) to add a new one. The next number is **0007**.

An ADR can't be changed once **Accepted**. To reverse a decision, write a *new*
ADR that supersedes it, and update the old one's status instead of deleting it.

| # | Title | Status |
|---|-------|--------|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-ui-agnostic-engine-with-immutable-state.md) | A UI-agnostic engine with immutable state transitions | Accepted |
| [0003](0003-bowers-as-effective-suit-and-rank.md) | Model bowers as an effective suit and rank | Accepted |
| [0004](0004-house-rules-as-an-options-object.md) | House rules are an options object, not forks of the engine | Accepted |
| [0005](0005-namespaces-follow-folders.md) | Engine namespaces follow the folder layout | Accepted |
| [0006](0006-pin-assemblyversion.md) | Pin the engine's `<AssemblyVersion>` at `1.0.0.0` | Accepted |
