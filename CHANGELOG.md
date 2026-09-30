# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.1.0] - 2026-09-30

### Added

- A Blazor WebAssembly UI (`Wolfgang.Hawsey.UI.Blazor`, with its components in `Wolfgang.Hawsey.UI.Shared`) running on the engine's `GameSession`, so the game logic, AI and scoring are all the engine's. During bidding it shows who holds the high bid ("Current high bid: 8 by East (opponents)"), not just the amount. (#883)
- The AI players now bid on hand strength (trump length, jacks, and aces outside trump), outbid only when they can beat the high bid, bid just the minimum as the stuck dealer, name the best-scoring trump suit, lead cards nothing left can beat, and don't overtake a winning partner. This is the AI from the Blazor UI, now in the engine so every UI plays the same way. It also counts the two left bowers as trump when judging a sure winner. (#879)
- The engine now declares `IsAotCompatible` and `IsTrimmable` on net8.0+, and CI publishes and runs a Native AOT consumer so trimmed apps are verified to play complete games. (#828)
- `GameState.HighBid` and `GameState.HighBidder` report the bidding so far (the bid to beat, and who holds it). A UI can show "current high bid: 8 by West", and a strategy can make a legal bid. (#878)
- The engine now includes `GameSession`, a live game between one human (South) and three AI seats, with paced AI moves, protection against stale or out-of-turn moves, and events for state changes, tricks, rounds and game over; and `SimpleAiStrategy`, the AI player it uses. Every Hawsey UI uses these instead of its own copy. (#859)
- New `GameState.Winner`: the team that won once the game is over, or `null` while it continues. When both teams reach `PointsToWin` in the same round, the team that held the bid wins. (#873)
- Engine types are grouped into per-area namespaces: `Wolfgang.Hawsey.Engine.Cards`, `.Bidding`, `.Game`, `.Players`, `.Rules`, `.Scoring`, `.Strategy` and `.TrickPlay`. (#824)
- `GameSession.AdvanceAsync()` plays the AI seats until the game waits for the human or for the next round, and returns what it is waiting for (`WaitingFor`). The Blazor UI drives the game with it instead of its own copy of the loop, and the human's forced last card of a round is played automatically. (#923)
- `GameSession.Bids` lists the bids made so far this round (`PlacedBid`: who bid and what), so a UI can show each seat's bid. `GameSession.PerformHumanHawseyExchange(discard)` makes the human's Hawsey exchange from the two cards the human discards, with the partner giving their two best for the trump named. (#882)
- `GameEngine.PlaceBid(state, player, action)` reads the bidding so far from the `GameState` (`HighBid`, `HighBidder`, `NextToAct`), so bidding needs no separate object to track, and replaying from any earlier state works. (#894)
- The engine tells a UI what to show at the end of a trick and a round: `GameState.TableCards` and `GameState.TableWinningCard` (the trick on the table, including the one just won until the next lead) and `GameState.RoundScore` (the round just scored), with `RoundScore.DeltaFor(team)` and `RoundScore.TricksFor(team)`. The Blazor UI uses them instead of its own copies of the rules. (#912)

### Fixed

- The Blazor app no longer requests two files that don't exist: it has a favicon, and it no longer links a scoped-CSS bundle the app never generates. (#911)
- `FollowSuitValidator.GetLegalPlays` (and so `GameState.GetLegalPlays`) always returns a new list. When every card was legal it used to return the hand itself, the game state's own list, which a caller could cast back to `List<Card>` and change. (#884)
- `GameEngine.PlayCard` no longer adds the card to the trick held by the state passed in. The input state is left unchanged, as `GameState` documents, so replaying from an earlier state works. (#873)
- The engine rejects values that could never work where they come in: a number bid above `GameState.MaximumBid` (12), a `HouseRules.MinimumBid` outside 1-12, a `HouseRules.PointsToWin` below 1, and an undefined `Suit` passed to `GameEngine.SelectTrump`. (#886)

### Internal

- The Blazor UI follows the repository's naming rules and uses `Router.NotFoundPage` (a `NotFound` page component) instead of the obsolete `NotFound` render fragment. (#907)
- The netstandard2.0 build of the engine is now reproducible: PolySharp generates only the IsExternalInit polyfill the engine uses, instead of every polyfill in a run-dependent order. (#840)
- Tidied engine doc references and casts, and removed dead bidding state; no behaviour change. (#825)
- The engine has one scoring path: `RoundScore` scores each round and `GameState.Winner` names the winner. Unused types, including a second scoring path that disagreed on who wins a tie, were removed before the first release. (#891)
- `GameSession.PlayHumanCard` works out the legal plays once, inside the engine, instead of checking legality itself and then having the engine check again. (#893)
- The engine's version attributes are audited and pinned by tests: FileVersion and InformationalVersion follow `<Version>`, and AssemblyVersion stays at 1.0.0.0 as ADR 0006 records. (#852)
- The .NET MAUI app (Android, Windows, iOS, Mac Catalyst) is not part of this release: it continues on the `feature/maui` branch until it is ready. This release ships the engine and the Blazor web app. (#931)

