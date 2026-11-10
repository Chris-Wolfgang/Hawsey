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
| `.github/workflows/pr.yaml` | InspectCode `timeout-minutes: 35` (template 20) | Set while the MAUI app (and its `dotnet workload restore`) was on `main`. The app now lives on `dev/maui`, so this can return to the template value. |
| `.github/workflows/pr.yaml` | InspectCode retries when it can't connect to MSBuild | Seen twice on 2026-11-06. It isn't in the template yet. |
| `.github/workflows/pr.yaml` | Adds `Stage 4: Linux ARM64 Tests` and `Stage 5: Windows ARM64 Tests` as one matrix job, `test-arm64`, built from Stage 3's steps | The fleet test-matrix standard requires them (#1078); the template has no canonical stages yet (repo-template#811). Take the template's form when it lands. |
| `.github/dependabot.yml` | pip ignores `semgrep`, `mcp` and `opentelemetry-*` | `semgrep.txt` uses overrides, which Dependabot can't apply. Update them with the uv command in that file's header. |
| `.github/workflows/codeql.yaml` | `timeout-minutes: 45` (template 30) | Same history as the InspectCode timeout; can return to the template value. |
| `.github/workflows/release.yaml` | Builds and attaches a reproducible-build manifest and `SHA256SUMS.txt`, and attests both with the packages | See [REPRODUCIBLE-BUILD.md](REPRODUCIBLE-BUILD.md) (#91, #1077). The template has no canonical form yet (repo-template#808); take it when it lands. |
| `.github/workflows/benchmarks.yaml`, `BannedSymbols.txt` | Template placeholders filled in | Project path and namespace are Hawsey's. |
| `coverlet.runsettings` | The comment allows `[ExcludeFromCodeCoverage]` on the Blazor host's `Program.Main` too | `Main` only creates and runs the WebAssembly host, which throws outside a browser; its configuration is in the tested `HawseyHost.Configure` (#1122). Comment only: no setting differs. |
| `.config/dotnet-tools.json` | Adds `microsoft.coyote.cli` | Concurrency tests for the engine's `GameSession` (#843). |
| `.editorconfig` | Trimming, AOT and single-file analyzers at `warning` | The engine is AOT- and trim-compatible (#844). A candidate to feed back to the template. |
| `docfx_project/docfx.json` | API metadata from the engine only, at `TargetFramework` net10.0; placeholders filled in | The engine is the only documented API (the apps are not packages), and it doesn't target the template's old net8.0. |
| `docs/WORKFLOW_SECURITY.md`, `docs/RELEASE-WORKFLOW-SETUP.md`, `.github/copilot-instructions.md`, `README.md`, `CONTRIBUTING.md` | Hawsey's own text | These describe this repository, not the template (#37, #40, #41). |
| `scripts/Setup-GitHubPages.ps1` | Kept, although the template marks it template-only | Added deliberately in #810 for this repository's Pages setup. |
| `.gitignore`, and trailing whitespace in several workflows | Order and whitespace only | Cosmetic (#814, #822). No behavioural difference. |

## Workflows only Hawsey has

`aot-smoke.yaml`, `coyote.yaml`, `cross-platform-differential.yaml`, `fuzz.yaml`,
`pr-benchmarks.yaml`, `release-apps.yaml` and `reproducible-build.yaml`. These cover engine- and
app-specific checks and the Blazor app's deployment, which the template has no equivalent for.

`aot-smoke.yaml` runs on linux-x64 and linux-arm64 (#1078). When the template ships its
canonical `aot-smoke.yaml` (repo-template#811), compare the two and take the template's form.

## Template files Hawsey doesn't have

These are template-only by design: `scripts/setup.ps1`, `scripts/Setup-BranchRuleset.ps1`,
`README-TEMPLATE.md`, `REPO-INSTRUCTIONS.md`, `TEMPLATE-PLACEHOLDERS.md`, `LICENSE-*.txt`,
`docs/repository-baseline.md`, `.github/workflows/template-deps.yaml`, and the template's own
`changelog/unreleased/*.md` fragments.

`.github/ISSUE_TEMPLATE/upstream_gap.yaml` is also skipped. Hawsey is an application with
no upstream library to report gaps in.
