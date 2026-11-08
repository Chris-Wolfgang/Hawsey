window.BENCHMARK_DATA = {
  "lastUpdate": 1794141887142,
  "repoUrl": "https://github.com/Chris-Wolfgang/Hawsey",
  "entries": {
    "Mutation score": [
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "7ccb32521814c97612d8ba59529b83b4555cea8c",
          "message": "ci(pr): Stages 2 and 3 fail when tests ran without recording coverage (#915) (#926)\n\nStage 1 got these rules from repo-template#654 (#917); the Windows and macOS\nstages still skipped their coverage gate silently when no coverage file\nappeared, and couldn't tell an assembly that ran without coverage from one\nthat wasn't there. Both stages now record the test assemblies that collected\ncoverage and require a report row for each; a missing coverage file fails when\na coverage-collecting TFM ran. Stage 2 records only projects that ran a net5+\nTFM (net4x / netcoreapp3.1 run without the collector by design). Stage 3 keeps\nbash 3.2 compatibility (while-read, no mapfile). Ported from\nstock-portfolio#308, which has no macOS stage.\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-09-29T22:30:11Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/7ccb32521814c97612d8ba59529b83b4555cea8c"
        },
        "date": 1790729275246,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.6,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "41a0e9a8ea5d84dbfc77823e880c4ba13efe9498",
          "message": "ci(coyote): file a bug only when the tests finished and failed (#945)\n\n* ci(coyote): file a bug only when the tests finished and failed\n\n#943 was opened as \"a concurrency bug was found\" for a run whose runner\nwas shut down (exit 143) after 2 h 22 m: the tests never finished, the\nresults upload never ran, and there was no bug report. The report job ran\non any failure of the coyote job and always said \"bug\".\n\nThe test step now records finished=true once dotnet test exits for any\nreason other than the 300-minute `timeout` (124). The report job files\n\"Coyote: a concurrency bug was found in GameSession\" (label bug) only when\nthe tests finished; otherwise it files \"Coyote: the scheduled run did not\nfinish\" (label maintenance), which says it is not evidence of a bug. Each\ntitle keeps its own open issue and gets a comment on a repeat. GameService\nis GameSession now.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n\n* ci(coyote): a bug is a failed test in the TRX, not any non-timeout exit\n\nReview on #945: \"finished\" was set for every exit but the timeout's, so a\nrestore or build error inside dotnet test, a test-host crash or a tee\nfailure would still be filed as a concurrency bug. And the other branch,\nwhich also covers checkout, setup and tool-restore failures, blamed a\ntimeout or a lost runner.\n\nThe tests are now built in their own step and run with --no-build. The run\nstep sets test-failed=true only when coyote.trx has a UnitTestResult with\noutcome=\"Failed\" (checked against a passing TRX, a failing one and a\nmissing file). Every other failure gets \"Coyote: the scheduled run failed\nwithout a test failure\", which points at the log instead of guessing a\ncause.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-02T00:27:17Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/41a0e9a8ea5d84dbfc77823e880c4ba13efe9498"
        },
        "date": 1791101560781,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.76,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "dependabot[bot]",
            "username": "dependabot[bot]",
            "email": "49699333+dependabot[bot]@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "4735400b44fc9304d46f6dd44c7f25292e508af4",
          "message": "Bump the dotnet-dependencies group with 2 updates (#989)\n\nBumps Meziantou.Analyzer from 3.0.294 to 3.0.297\nBumps Roslynator.Analyzers from 5.0.0 to 5.0.1\n\n---\nupdated-dependencies:\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.297\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Roslynator.Analyzers\n  dependency-version: 5.0.1\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Chris Wolfgang <210299580+Chris-Wolfgang@users.noreply.github.com>",
          "timestamp": "2026-10-10T23:03:02Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/4735400b44fc9304d46f6dd44c7f25292e508af4"
        },
        "date": 1791699832330,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.76,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "ecf84fa383ef96f0f79c50a537b253fef73751b2",
          "message": "fix(scripts): security-alerts updates the open summary issue instead of filing a new one (#993)\n\nGet-TrackedIssues keyed each marker to the LAST issue in the listing. The REST\nissues listing (state=all) is newest first, so with a closed old summary and an\nopen newer one the map kept the CLOSED issue, and every weekly run filed a\nduplicate summary issue. Prefer the OPEN issue for a marker, and between two of\nthe same state the higher number. Port of Chris-Wolfgang/repo-template#719.\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T20:05:05Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/ecf84fa383ef96f0f79c50a537b253fef73751b2"
        },
        "date": 1792304524012,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.76,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "7a35852501cb377d8b73f6da27f2972077018025",
          "message": "docs: describe main as it is: Blazor app, dev/maui branch, published package (#1002) (#1021)\n\n* chore: drop issue-number references from code comments and project files (#1007)\n\nA comment states the rule; the incident that led to it lives in the commit\nmessage and PR body. Each parenthesised issue number is removed, and where the\nsentence relied on it the reason is kept in words. No code changes.\n\nHawsey.slnx.DotSettings is left alone: it is template-synced, so the change\nbelongs in repo-template first.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* chore: project-file hygiene (#1006)\n\n- Engine tests: xunit.runner.visualstudio 3.0.1 on net8.0+, and 2.8.2 capped\n  with allowedVersions=\"(,3.0.0)\" on net462-net481, net6.0 and net7.0, so a\n  Dependabot bump cannot move those slots to a runner that drops them\n  (netcoreapp3.1/net5.0 keep 2.4.5, capped below 2.5.0).\n- Engine csproj: drop <Authors>, which Directory.Build.props already sets; add\n  <RepositoryUrl>, which Directory.Build.props leaves to each src project.\n- UI.Blazor / UI.Shared csproj: drop <Authors> and the year-less <Copyright>\n  that overrode the year-stamped value from Directory.Build.props.\n- Remove src/, tests/, benchmarks/ and examples/.gitkeep: each directory has\n  held real projects since 0.1.0.\n- CHANGELOG.md: add the Keep a Changelog link references for [Unreleased]\n  and [0.1.0].\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* chore: rename the Coyote test project to Wolfgang.Hawsey.Engine.Tests.Concurrency (#1006)\n\ntests/Wolfgang.Hawsey.UI.Maui.Tests.Concurrency tests only the engine's\nGameSession, and the MAUI app is not on main. The project, its folder and\nnamespace become Wolfgang.Hawsey.Engine.Tests.Concurrency; Hawsey.slnx,\nREADME.md, .github/copilot-instructions.md and two comments follow.\n\ncoyote.yaml builds and tests the project by path; it is a protected file and\nchanges in its own PR, to merge together with this one.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* docs: describe main as it is: Blazor app, dev/maui branch, published package (#1002)\n\n- README links the MAUI branch as dev/maui (the\n  feature/maui link 404s), and no longer say the package is unpublished.\n- copilot-instructions: the project table lists the Blazor projects instead\n  of the MAUI ones; no workload prerequisite; the workflow list names\n  release-apps.yaml instead of a MAUI build.\n- getting-started.md / docfx index.md: drop the \"not published yet\" text;\n  0.1.0 is on nuget.org.\n- SECURITY.md: consumers and package coordinates reflect the published package\n  and the Blazor app.\n- CONTRIBUTING.md: no MAUI workload prerequisite.\n- TEMPLATE-DRIFT.md: the timeouts are history from the MAUI app being on main;\n  docfx row reason; Hawsey-only workflows list release-apps.yaml, not\n  maui-build.yaml.\n- THREAT-MODEL.md: GameService -> GameSession. adr/index.md: next is 0008.\n- RELEASE-WORKFLOW-SETUP.md / STACKED-PRS.md: say Hawsey has no\n  Setup-BranchRuleset.ps1 (Fix-BranchRuleset.ps1 calls it).\n- Blazor Program.cs, Index.razor, both UI csproj comments: no \"like the MAUI\n  app\" for an app not on this branch. AssemblyInfo keeps the MAUI tests'\n  InternalsVisibleTo (the project lives on dev/maui) and says why.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-05T18:11:39Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/7a35852501cb377d8b73f6da27f2972077018025"
        },
        "date": 1793904412658,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.78,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "dependabot[bot]",
            "username": "dependabot[bot]",
            "email": "49699333+dependabot[bot]@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "efa657ec58d453feb23aed22e55999f1ff0ab384",
          "message": "Bump the dotnet-dependencies group with 3 updates (#1076)\n\nBumps bunit from 2.13.3 to 2.14.0\nBumps jetbrains.resharper.globaltools from 2026.2.4 to 2026.2.4.1\nBumps Meziantou.Analyzer from 3.0.320 to 3.0.321\n\n---\nupdated-dependencies:\n- dependency-name: bunit\n  dependency-version: 2.14.0\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: jetbrains.resharper.globaltools\n  dependency-version: 2026.2.4.1\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.321\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Chris Wolfgang <210299580+Chris-Wolfgang@users.noreply.github.com>",
          "timestamp": "2026-11-08T02:28:07Z",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/efa657ec58d453feb23aed22e55999f1ff0ab384"
        },
        "date": 1794141882384,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score (tests/Wolfgang.Hawsey.Engine.Tests.Unit)",
            "value": 91.94,
            "unit": "%"
          }
        ]
      }
    ]
  }
}