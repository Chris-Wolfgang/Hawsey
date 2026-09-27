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
| 2026-09-27 | 90.06% | 88 | `GameSession` and `SimpleAiStrategy` moved into the engine; their behaviour tests added (#859) |
| 2026-09-27 | 90.43% | 88 | The Blazor UI's AI becomes the engine's (#879); bidding progress on `GameState` (#878); session bids and the human Hawsey exchange (#882) |

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

## `GameSession` and `SimpleAiStrategy` (2026-09-27)

Moving the session and the AI into the engine (#859, ADR 0007) added their mutants to
the run. It dropped the score to 80.85%, because their tests only checked that the AI's
moves were legal. The behaviour tests (`SimpleAiStrategyBehaviourTests`,
`GameSessionEventTests`, and the paced checks in `GameSessionNewGameDuringPauseTests`)
brought it back to 90.06%. The remaining survivors in these two files are accepted:

| Mutation | Why it survives |
|---|---|
| `ConfigureAwait(false)` becomes `true` | Equivalent: the engine and its tests run without a synchronization context. |
| `_generation++` becomes `--` | Equivalent: any change to the counter invalidates a stale AI loop. |
| `\|\|` and `&&` variants of the "the game moved on during the pause" guards | Equivalent in every reachable state: a New Game's state is always in the Bidding phase, so each guard's first two terms can't disagree. The Coyote tests cover these races. |
| `CompletedTricks.Count > 0` becomes `>= 0` | Equivalent: after any play, either the current trick holds a card or a trick has just completed. |
| `>` becomes `>=` and `<` becomes `<=` in the AI's rank comparisons | Equivalent: the only equal ranks are the two copies of one card. |
| The game-over winner expression | Replaced by the engine's own `GameState.Winner` (#862). |

## The engine AI from the Blazor UI (2026-09-27)

Run on `main` at 699e064, after #878, #879, #880, #882 and #883: **90.43%**, with 579 killed,
59 survived, 4 without coverage and 16 timeouts. #879's four behaviour tests had
already been added. Most survivors fall into the categories above: exception-message
strings, the `GameSession` pause guards, generation and `ConfigureAwait` variants, the
removed null guards in `GameEngine` and `GameRunner`, and the `Trick.GetCurrentWinner`
tie. The rest were **genuine gaps**, killed by the tests in #897. A scoped run over
`SimpleAiStrategy.cs` and `GameState.cs` took those two files from 21 survivors to 10:

| Location | Mutation | Killing test (#897) |
|---|---|---|
| `GameState.Winner`: `EastWestScore >= Rules.PointsToWin` | `>=` becomes `>` | East/West finishing exactly on the target |
| `GameState.Winner`: the "not game over" early return | Block removed | A not-yet-over state past the target |
| `SimpleAiStrategy` sure-winner check: trumps still out | Arithmetic and equality mutations | Every other trump gone, so a side queen is a sure winner |
| `SimpleAiStrategy` sure-winner check: higher copies still out, and the lead choice | `< 0`, `!= target`, `&&` becomes `\|\|` | A lower sure winner over a higher card that can be beaten |
| `SimpleAiStrategy.WinningPlay`: `>` | `>=`: a tying copy takes the trick | An opponent ties the partner's card |

The 10 that remain are equivalent, and accepted:

| Location | Mutation | Why it is equivalent |
|---|---|---|
| `GameState.Winner`: the final `eastWestReached ? EastWest : null` | Always `EastWest` | At game over one team has always reached the target. |
| `SimpleAiStrategy`: the rank comparisons that pick the highest, lowest or cheapest winning card | `>` / `<` become `>=` / `<=` | The only equal ranks are the two copies of one card. |
| `SimpleAiStrategy.DecidePlay`: the one-legal-card shortcut | Block removed | The rest of the method picks that card anyway. |
| `SimpleAiStrategy` trump choice: `bestScore` starts at -1 | Becomes +1 | A dealt 12-card hand always has a suit that scores above 1. |
| `SimpleAiStrategy.CountInHand` (the count, and its use in the sure-winner check) | `count--`, the increment removed, `+ CountInHand` | The count only decides a card when the AI holds the other copy of a higher card, and that card is then itself a sure winner that outranks it and gets led instead. |
