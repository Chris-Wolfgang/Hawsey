# Mutation testing

The engine's test suite is mutation-tested with [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/).
The configuration lives in `tests/Wolfgang.Hawsey.Engine.Tests.Unit/stryker-config.json`,
and `.github/workflows/stryker.yaml` runs it on PRs that touch code and weekly. The run
fails below `thresholds.break`. That floor is ratcheted up as the score rises, and is
never lowered.

```bash
cd tests/Wolfgang.Hawsey.Engine.Tests.Unit
dotnet tool restore && dotnet stryker
```

## Score history

| Date | Score | Break | Change |
|---|---:|---:|---|
| 2026-09-22 | 82.30% | 75 | Gate introduced (#71) |
| 2026-09-25 | 92.42% | 88 | Survivors triaged and killed (#36) |

## Triage of the surviving mutants (2026-09-25)

A surviving mutant is a code change that no test notices. Each was either killed with
a test or classified as below. The #36 pass added tests for round scoring (an FsCheck
property that replays real rounds), the points-to-win boundary, Hawsey-round state,
dealing, card legality ties, must-trump, and the phase and turn guards.

The remaining survivors are **accepted**. Re-triage any new one that appears.

### Equivalent mutants (the mutated program behaves identically)

| Location | Mutation | Why it is equivalent |
|---|---|---|
| `Deck.Shuffle`: `i > 0` → `i >= 0` | One extra loop iteration | At `i = 0`, `random.Next(1)` is always 0, so the card is swapped with itself. |
| `FollowSuitValidator.GetLegalPlays`: remove the empty-hand early return | Falls through | Every later branch returns the same empty hand. |
| `Trick.GetCurrentWinner`: `> 0` → `>= 0` | A later tying card replaces the winner | Only the two identical copies of a card tie in a pinochle deck, and the method returns the `Card` *value*, which is the same either way. `GetResult`, which returns the *player*, is covered and kills its version of this mutant. |
| `GameEngine.StartGame` / `StartNextRound`: remove the `random` null guard | Guard skipped | `Deck.Shuffle` throws the same `ArgumentNullException` for the same parameter name on the next line. The guard is kept for a clear stack trace. |
| `GameRunner.RunGame`: remove the `rules` / `random` null guards | Guard skipped | `GameEngine.StartGame` throws the same exception for the same parameter, before any other work. |

### Exception-message string mutations (20)

Stryker replaces a message such as `"Bidding is already complete."` with `""`. Tests
assert the exception **type** and, where it matters, the **parameter name**, not the
message text. The wording isn't part of the engine's contract, and pinning it would
make every rewording a test change. These are accepted by policy.
