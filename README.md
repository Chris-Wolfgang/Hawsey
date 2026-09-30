# Hawsey

A C# implementation of **Hawsey**, a four-player team trick-taking card game played with a pinochle deck. Hawsey is popular in Pennsylvania Dutch communities and shares ancestry with pinochle, schafkopf, and similar bidding/trick games.

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.Hawsey.Engine.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.Hawsey.Engine/)
[![Downloads](https://img.shields.io/nuget/dt/Wolfgang.Hawsey.Engine.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.Hawsey.Engine/)
[![PR build](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/Hawsey/pr.yaml?event=pull_request&label=PR%20build&logo=github)](https://github.com/Chris-Wolfgang/Hawsey/actions/workflows/pr.yaml)
[![release](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/Hawsey/release.yaml?event=release&label=release&logo=github)](https://github.com/Chris-Wolfgang/Hawsey/actions/workflows/release.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/Hawsey)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/Chris-Wolfgang/Hawsey/badge)](https://scorecard.dev/viewer/?uri=github.com/Chris-Wolfgang/Hawsey)
[![Coverage](https://img.shields.io/endpoint?url=https://Chris-Wolfgang.github.io/Hawsey/versions/latest/coverage/badge.json&logo=github)](https://Chris-Wolfgang.github.io/Hawsey/versions/latest/coverage/)

---

## 📦 Installation

```bash
dotnet add package Wolfgang.Hawsey.Engine
```

**NuGet Package:** [Wolfgang.Hawsey.Engine](https://www.nuget.org/packages/Wolfgang.Hawsey.Engine)

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/Hawsey](https://github.com/Chris-Wolfgang/Hawsey)
- **API Documentation:** https://Chris-Wolfgang.github.io/Hawsey/
- **Formatting Guide:** [docs/README-FORMATTING.md](docs/README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)
- **Code of Conduct:** [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
- **Security Policy:** [SECURITY.md](SECURITY.md)
- **Threat Model:** [docs/THREAT-MODEL.md](docs/THREAT-MODEL.md)
- **Architecture Decisions:** [docs/adr/](docs/adr/index.md)

---

## 🚀 Quick Start

```csharp
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;

var engine = new GameEngine();
var rules = HouseRules.Default;
var random = new Random();

// Start a game with the dealer in the South seat
var state = engine.StartGame(rules, PlayerPosition.South, random);

// state.Phase == GamePhase.Bidding
// state.Hands holds 12 cards for each of the four players
```

From there, `PlaceBid`, `SelectTrump`, `ExchangeHawseyCards` and `PlayCard` on `GameEngine` each take the current `GameState` and return the next one.

To play a whole game programmatically (for example AI against AI), implement `IPlayerStrategy` and pass it to `GameRunner.RunGame`:

```csharp
using Wolfgang.Hawsey.Engine.Strategy;

var runner = new GameRunner();
var final = runner.RunGame(myStrategy, HouseRules.Default, PlayerPosition.South, new Random());

// final.Phase == GamePhase.GameOver
```

One strategy instance makes the decisions for every seat; each method receives the `PlayerPosition` it is deciding for.

---

## ✨ Features

| Area | Types |
|------|-------|
| Pinochle deck | `Card`, `Rank`, `Suit`, `Deck` (create and shuffle), `CardRanking`, `CardComparer` |
| Bidding | `BidAction` (`PassBid`, `NumberBid`, `HawseyBid`), `BiddingResult`, `PlacedBid` |
| Trick play | `Trick`, `PlayedCard`, `FollowSuitValidator`, `TrickResult` |
| House rules | `HouseRules` (`MustBeat`, `MustTrump`, `MinimumBid`, `PointsToWin`), `TrumpMode` (`Suited` or `AceHigh`) |
| Players and teams | `PlayerPosition`, `Team` |
| Scoring | `RoundScore` |
| Game flow | `GameEngine`, `GameState`, `GamePhase` |
| Strategy hooks | `IPlayerStrategy`, `GameRunner` |

The engine does no I/O. On `net10.0` it is marked trim- and Native AOT-compatible (`IsTrimmable`, `IsAotCompatible`); `examples/Wolfgang.Hawsey.Engine.AotSmoke` plays complete games through a Native AOT build of it.

---

## 🃏 About the Game

Hawsey is a **4-player partnership trick-taking game** dealt from a 48-card pinochle deck (two each of 9, 10, J, Q, K, A in all four suits). Each player gets 12 cards. Two teams of two sit across from each other: North/South and East/West. Each hand has three phases:

1. **Bidding.** Players pass or bid the number of tricks their team will take. The minimum bid is set by the house rules (default 6).
2. **Trump selection.** The winning bidder names a trump suit, or plays **ace high** (no trump).
3. **Trick play.** Players follow suit when they can. The trick goes to the highest trump or, if no trump was played, the highest card of the suit led.

With a trump suit, the jack of trump (the right bower) is the highest card. The jack of the other suit of the same colour (the left bower) is second highest and counts as trump. Ace-high rounds have no bowers.

**Scoring** counts tricks. Each trick is worth one point. If the bidding team takes at least its bid, it scores the tricks it took; if not, it loses the bid amount (it is **bucked**). The defending team scores the tricks it took. **The first team to reach 62 points wins** (`HouseRules.PointsToWin`).

### The Hawsey bid

The game's namesake bid: the bidder plays alone to take all 12 tricks. The bidder discards two cards and takes two cards from their partner, and the partner sits out the round. Taking all 12 tricks scores 24 points; missing loses 24.

### House rules

`HouseRules` covers the variants the engine supports: must-beat, must-trump, the minimum bid, and the points needed to win.

---

## 🧩 Project Layout

| Project | Purpose |
|---------|---------|
| `src/Wolfgang.Hawsey.Engine` | Game-logic library (cards, bidding, trick play, scoring, rules). The NuGet package. |
| `src/Wolfgang.Hawsey.UI.Blazor` | Blazor WebAssembly app, played at [chris-wolfgang.github.io/Hawsey/play](https://chris-wolfgang.github.io/Hawsey/play/). Its components are in `src/Wolfgang.Hawsey.UI.Shared`; the game logic is the engine's `GameSession` ([ADR 0007](docs/adr/0007-game-session-in-the-engine-and-testable-uis.md)). Not published as a package. |
| `src/Wolfgang.Hawsey.UI.Maui` | .NET MAUI app with AI opponents, built on the engine's `GameSession`. Also targets plain `net10.0` for its unit tests ([ADR 0007](docs/adr/0007-game-session-in-the-engine-and-testable-uis.md)). Not published as a package. |
| `src/Wolfgang.Hawsey.UI.Shared` | The Blazor app's Razor components and styles. |
| `tests/Wolfgang.Hawsey.Engine.Tests.Unit` | Engine unit tests, run on every target framework from `net462` to `net10.0`. |
| `tests/Wolfgang.Hawsey.UI.Maui.Tests.Unit` | Unit tests for the MAUI app: view models, converters and the app host (XAML and DI). |
| `tests/Wolfgang.Hawsey.UI.Maui.Tests.Concurrency` | Coyote concurrency tests for the engine's `GameSession`. |
| `benchmarks/Wolfgang.Hawsey.Engine.Benchmarks` | BenchmarkDotNet benchmarks for the engine. |
| `examples/Wolfgang.Hawsey.Engine.AotSmoke` | Native AOT smoke test that plays complete games through the engine. |

The engine is UI-agnostic. Each `GameEngine` method takes the current `GameState` and returns a new one, which makes the engine easy to unit test and replay (see [ADR 0002](docs/adr/0002-ui-agnostic-engine-with-immutable-state.md)).

---

## 🎯 Supported Frameworks

`Wolfgang.Hawsey.Engine` targets:

- `netstandard2.0`
- `net10.0`

The MAUI app targets `net10.0-android` on every OS, plus `net10.0-windows10.0.19041.0` when built on Windows and `net10.0-ios` / `net10.0-maccatalyst` when built on macOS.

Once the package is published, see the [NuGet package page](https://www.nuget.org/packages/Wolfgang.Hawsey.Engine/) for the authoritative per-TFM compatibility matrix.

---

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers**, ReSharper InspectCode on every PR, and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This repository uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- ❌ `Task.Wait()`, `Task.Result` - Use `await` instead
- ❌ `Thread.Sleep()` - Use `await Task.Delay()` instead
- ❌ Synchronous file I/O (`File.ReadAllText`) - Use async versions
- ❌ Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- ❌ `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- ❌ Obsolete APIs (`WebClient`, `BinaryFormatter`)

---

## 🛠️ Building from Source

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) 10.0
- The .NET MAUI workloads, to build the MAUI app (`dotnet workload restore` installs the ones this OS needs)
- [PowerShell 7](https://github.com/PowerShell/PowerShell) (`pwsh`) for the scripts under `scripts/`

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/Hawsey.git
cd Hawsey

# Install the workloads the MAUI app needs
dotnet workload restore

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting
pwsh ./scripts/format.ps1

# Run the PR workflow's Windows stage locally (build, tests on every TFM, coverage gates, DevSkim, gitleaks)
pwsh ./scripts/build-pr.ps1
```

To build only the engine, without any workloads:

```bash
dotnet build src/Wolfgang.Hawsey.Engine --configuration Release
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting without changing files
dotnet format --verify-no-changes
```

See [docs/README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting guidelines.

### Documentation

API documentation is generated with [DocFX](https://dotnet.github.io/docfx/) from `docfx_project/`. The release workflow builds it and deploys it to GitHub Pages: each release lands under `versions/<tag>/` (plus `versions/latest/`), with a version picker on every page. See [docs/DOCFX-VERSION-PICKER.md](docs/DOCFX-VERSION-PICKER.md).

---

## 🔐 Verify a Release

Every package published from this repository carries a [SLSA build-provenance attestation](https://docs.github.com/en/actions/security-for-github-actions/using-artifact-attestations/using-artifact-attestations-to-establish-provenance-for-builds): a signed statement, recorded on GitHub, that the exact `.nupkg` bytes were produced by this repository's release workflow at a given commit. Verify a downloaded package with the GitHub CLI:

```bash
gh attestation verify Wolfgang.Hawsey.Engine.X.Y.Z.nupkg \
  --repo Chris-Wolfgang/Hawsey \
  --signer-workflow Chris-Wolfgang/Hawsey/.github/workflows/release.yaml
```

`--repo` restricts the lookup to this repository's attestations and `--signer-workflow` requires that the signing workflow was this repository's `release.yaml`; with both, the command fails if the package was built anywhere else or was modified after the build. A CycloneDX SBOM (`*.bom.json`) listing the package's full dependency closure is attached to each GitHub Release alongside the package.

Each release also attaches `Wolfgang.Hawsey.Engine.<version>.reproducible-build-manifest.json`, which lists the expected SHA-256 of every assembly in the package. You can rebuild from the tagged commit and confirm the published DLLs are byte-identical. The procedure, and how to report a match or mismatch as an independent verifier, is in [docs/REPRODUCIBLE-BUILD.md](docs/REPRODUCIBLE-BUILD.md).

---

## 🔄 Keeping Up With the Template

This repository was generated from [Chris-Wolfgang/repo-template](https://github.com/Chris-Wolfgang/repo-template); `.template-version` records the template commit it was set up from. To pull later template fixes (workflows, analyzer rules, tool pins):

```powershell
pwsh ./scripts/upgrade.ps1          # dry run: safe / review / in-sync / removed per file
pwsh ./scripts/upgrade.ps1 -Apply   # take the safe files, sidecar the rest, re-stamp
```

Files never touched here come across as-is; files customised here get a `<file>.template` sidecar to merge by hand. Workflow and `Directory.Build.props` changes must travel in a configuration-only PR (the Protected Files guard passes it on review); mixed with code they fail the guard.

---

## 🤝 Contributing

Contributions are welcome, especially:

- **House-rule variants.** New variants belong in `HouseRules` (see [ADR 0004](docs/adr/0004-house-rules-as-an-options-object.md)).
- **AI strategies.** Implement `IPlayerStrategy`.
- **UI work** on the Blazor app or the MAUI app.

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details
