# ADR 0004 — House rules are an options object, not forks of the engine

- **Status:** Accepted
- **Date:** 2026-09-22 (records a decision made at the engine's creation)
- **Deciders:** Chris Wolfgang

## Context

Families play Hawsey differently. The common variants are: must you beat the
current winner even when it's your partner's card (**MustBeat**)? Must you trump
when you can't follow suit (**MustTrump**)? What is the minimum bid? How many
points win the game? The table this project is built for plays MustBeat: if you
can follow suit you must beat the winning card, but trumping when void is
optional.

## Decision

Variants are properties on an immutable `HouseRules` (`init`-only, with
`HouseRules.Default`). The rules travel inside `GameState`, and the rule code
(`FollowSuitValidator`, bidding, scoring) reads them. The engine's defaults are
the most permissive (`MustBeat = false`, `MustTrump = false`, minimum bid 6, 62
points to win). Each front end chooses its table's rules. The Blazor UI starts
games with `new HouseRules { MustBeat = true }`; the MAUI XAML UI currently uses
`HouseRules.Default`.

## Alternatives considered

- **Hard-code one ruleset** — simplest, but it's wrong for most tables, and the
  first variant request would force a fork.
- **A strategy-pattern class per rule** — more extensible than needed for a
  handful of yes/no and numeric switches.

## Consequences

- A new variant is a new `HouseRules` property plus its branch in the rule code
  and tests. Existing callers keep their behaviour because the default is permissive.
- The defaults are a compatibility promise. Changing one is a breaking change that
  needs its own ADR.
