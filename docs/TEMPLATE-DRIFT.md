# Template drift

Hawsey is generated from [repo-template](https://github.com/Chris-Wolfgang/repo-template),
and `.template-version` records the template commit it last synced to. This page lists
where Hawsey **deliberately** differs from the template, so a drift scan can tell those
files apart from files that have fallen behind. Any difference not listed here is
unexpected: take the template's version, or add the difference here with the reason.

Last full sync: template `e5e665a` (2026-09-26, Hawsey #24).

## Files that differ on purpose

| File | Difference | Why |
|---|---|---|
| `.github/workflows/pr.yaml` | InspectCode `timeout-minutes: 35` (template 20) | The MAUI app's `dotnet workload restore` alone takes minutes (#844). |
| `.github/workflows/codeql.yaml` | `timeout-minutes: 45` (template 30) | Same MAUI workload restore (#844). |
| `.github/workflows/release.yaml` | Builds and attaches a reproducible-build manifest | See [REPRODUCIBLE-BUILD.md](REPRODUCIBLE-BUILD.md) (#91). |
| `.github/workflows/benchmarks.yaml`, `BannedSymbols.txt` | Template placeholders filled in | Project path and namespace are Hawsey's. |
| `.config/dotnet-tools.json` | Adds `microsoft.coyote.cli` | Concurrency tests for the engine's `GameSession` (#843). |
| `.editorconfig` | Trimming, AOT and single-file analyzers at `warning` | The engine is AOT- and trim-compatible (#844). A candidate to feed back to the template. |
| `docfx_project/docfx.json` | API metadata from the engine only, at `TargetFramework` net10.0; placeholders filled in | The MAUI project can't be built without workloads, and the engine doesn't target the template's old net8.0. |
| `docs/WORKFLOW_SECURITY.md`, `docs/RELEASE-WORKFLOW-SETUP.md`, `.github/copilot-instructions.md`, `README.md`, `CONTRIBUTING.md` | Hawsey's own text | These describe this repository, not the template (#37, #40, #41). |
| `scripts/Setup-GitHubPages.ps1` | Kept, although the template marks it template-only | Added deliberately in #810 for this repository's Pages setup. |
| `.gitignore`, and trailing whitespace in several workflows | Order and whitespace only | Cosmetic (#814, #822). No behavioural difference. |

## Workflows only Hawsey has

`aot-smoke.yaml`, `coyote.yaml`, `cross-platform-differential.yaml`, `fuzz.yaml`,
`maui-build.yaml`, `pr-benchmarks.yaml` and `reproducible-build.yaml`. These cover engine- and
MAUI-specific checks that the template has no equivalent for.

## Template files Hawsey doesn't have

These are template-only by design: `scripts/setup.ps1`, `scripts/Setup-BranchRuleset.ps1`,
`README-TEMPLATE.md`, `REPO-INSTRUCTIONS.md`, `TEMPLATE-PLACEHOLDERS.md`, `LICENSE-*.txt`,
`docs/repository-baseline.md`, `.github/workflows/template-deps.yaml`, and the template's own
`changelog/unreleased/*.md` fragments.

`.github/ISSUE_TEMPLATE/upstream_gap.yaml` is also skipped. Hawsey is an application with
no upstream library to report gaps in.
