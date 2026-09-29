# Copilot Coding Agent Instructions

## Repository Summary

Hawsey is a C# implementation of a four-player team trick-taking card game played with a pinochle deck. The repository contains a game-engine library, a .NET MAUI app, their tests, benchmarks, and a Native AOT smoke example. It was generated from `Chris-Wolfgang/repo-template` (`.template-version` records the template commit).

**Primary Language**: C#
**Solution**: `Hawsey.slnx`

| Project | Target frameworks |
|---|---|
| `src/Wolfgang.Hawsey.Engine` | `netstandard2.0;net10.0` |
| `src/Wolfgang.Hawsey.UI.Maui` | `net10.0` (unit-test target, no platform head) and `net10.0-android`, plus `net10.0-windows10.0.19041.0` on Windows and `net10.0-ios;net10.0-maccatalyst` on macOS |
| `tests/Wolfgang.Hawsey.Engine.Tests.Unit` | `net462` through `net10.0` (13 TFMs) |
| `tests/Wolfgang.Hawsey.UI.Maui.Tests.Unit` | `net10.0` |
| `tests/Wolfgang.Hawsey.UI.Maui.Tests.Concurrency` | `net10.0` (Coyote) |
| `benchmarks/Wolfgang.Hawsey.Engine.Benchmarks` | `net10.0` |
| `examples/Wolfgang.Hawsey.Engine.AotSmoke` | `net10.0` |

## Build and Validation Instructions

### Prerequisites
- .NET 10 SDK
- The MAUI workloads for the MAUI app: `dotnet workload restore`
- Local tools pinned in `.config/dotnet-tools.json`: `dotnet tool restore`

### Build and test

```powershell
dotnet workload restore
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

To build only the engine without workloads: `dotnet build src/Wolfgang.Hawsey.Engine --configuration Release`.

To run the Windows stage of the PR workflow locally (build, tests on every TFM, coverage gates, DevSkim, gitleaks):

```powershell
pwsh ./scripts/build-pr.ps1
```

### Critical Build Requirements
- **Warnings as errors**: `TreatWarningsAsErrors` is on for Release builds (`Directory.Build.props`).
- **Code coverage**: 95% line coverage for `src/` assemblies and 100% for test assemblies (`CODECOV_MINIMUM` / `CODECOV_TEST_MINIMUM` in `pr.yaml`).
- **Banned APIs**: `BannedSymbols.txt` bans blocking and synchronous APIs (`Task.Wait`, `Task.Result`, `Thread.Sleep`, synchronous file and stream I/O, `DateTime.Now`, and others).
- **Changelog**: a PR that changes `src/` adds a fragment under `changelog/unreleased/` (see `changelog/unreleased/README.md`) or carries the `no-changelog` label.
- **Protected files**: workflows, `.editorconfig`, `Directory.Build.props`, `BannedSymbols.txt` and the other files listed in `.github/workflows/protected-files.yaml` go in their own PR. The Protected Files Guard fails a PR that mixes them with other changes.

## Project Layout and Architecture

- The engine (`src/Wolfgang.Hawsey.Engine`) is UI-agnostic and does no I/O. Each `GameEngine` method takes the current `GameState` and returns the next one. Namespaces follow folders: `Bidding`, `Cards`, `Game`, `Players`, `Rules`, `Scoring`, `Strategy`, `TrickPlay` under `Wolfgang.Hawsey.Engine`.
- Card comparisons go through `CardRanking` (bowers change a card's effective suit and rank). See `docs/adr/` for the design decisions.
- The MAUI app (`src/Wolfgang.Hawsey.UI.Maui`) uses the engine by project reference. Game logic that needs no UI framework (the AI, the human-vs-AI `GameSession`) belongs in the engine, not in a UI (ADR 0007).

### Key Configuration Files
- `.editorconfig`: code style and analyzer severities
- `Directory.Build.props`: shared build settings and analyzer package references
- `BannedSymbols.txt`: banned APIs
- `coverlet.runsettings`: coverage settings
- `.gitleaks.toml`: secrets-scan configuration

### GitHub Integration
- `.github/workflows/pr.yaml`: PR checks (gitleaks, project detection, changelog fragment check, InspectCode, Linux/Windows/macOS test stages with coverage gates, DevSkim)
- `.github/workflows/protected-files.yaml`: protected-files guard
- `.github/workflows/release.yaml`: release on a published GitHub Release
- `.github/workflows/docfx.yaml`: documentation build and deploy
- Other workflows cover CodeQL, Semgrep, Scorecard, Stryker mutation testing, Coyote, fuzzing, benchmarks, the MAUI build, the AOT smoke test and reproducible builds.
- `ISSUE_TEMPLATE/`, `pull_request_template.md`, `CODEOWNERS`, `dependabot.yml`

## Agent Guidelines

1. Follow `.editorconfig` (file-scoped namespaces, Allman braces, 4-space indentation).
2. Add or update tests for every behaviour change. Engine tests run on every TFM, including .NET Framework.
3. Keep public API members documented with XML doc comments.
4. Run the build and tests in Release configuration before submitting.
5. Ensure all GitHub Actions checks pass.
