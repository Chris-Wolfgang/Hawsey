# ADR 0003 — Model bowers as an effective suit and rank

- **Status:** Accepted
- **Date:** 2026-09-22 (records a decision made at the engine's creation)
- **Deciders:** Chris Wolfgang

## Context

Hawsey is played with a pinochle deck (9 through ace, two copies of each card)
and has **bowers**. The jack of trump (the right bower) is the highest card. The
jack of the other suit of the same colour (the left bower) is second highest and
**counts as trump**, not as its printed suit. Both follow-suit checks and
trick-winner checks depend on this. Comparing a card's printed `Suit` or `Rank`
gives wrong answers for exactly these two cards, and the bug is easy to miss:
an early AI thought the ace of trump beat everything.

## Decision

`CardRanking` is the **single source of truth**. Given the trump suit (or `null`
for ace-high), it returns an *effective suit* (the left bower reports the trump
suit) and an *effective rank*. Ordinary cards keep their rank. The right bower
is `RightBowerRank` (16) and the left bower is `LeftBowerRank` (15), both above
the ace. `CardComparer`, `FollowSuitValidator`, `Trick` and every AI compare
through `CardRanking`, never through `Card.Suit` or `Card.Rank` directly.

## Alternatives considered

- **Special-case the two jacks in each rule** — spreads the rule across every
  comparison, and missing one special case is exactly the bug above.
- **Re-label the left bower's suit in the dealt hand** — breaks display (the card
  still shows its printed suit) and has to be undone between rounds.

## Consequences

- Anything that decides **legality or strength** (follow-suit checks, beating
  the current winner, trick winners, and AI estimates of whether a card will
  win) must ask `CardRanking`. Reading raw `Card.Suit` or `Card.Rank` there is a
  review red flag. Reading the printed suit is still correct where it's what's
  meant, for example counting printed suits in a hand to choose trump
  (`SimpleAiStrategy.DecideTrump`) or showing a card.
- FsCheck properties pin the invariants: the left bower is trump and ranks just
  below the right, and `CardComparer` is antisymmetric.
