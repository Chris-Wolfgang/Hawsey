window.BENCHMARK_DATA = {
  "lastUpdate": 1793887200307,
  "repoUrl": "https://github.com/Chris-Wolfgang/Hawsey",
  "entries": {
    "BenchmarkDotNet": [
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
        "date": 1790728268916,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 544.4296566645304,
            "unit": "ns",
            "range": "± 0.6489253194969732"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 90.2740714152654,
            "unit": "ns",
            "range": "± 0.25836529829937066"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 285.7086335817973,
            "unit": "ns",
            "range": "± 0.7279251624769907"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 384578.60563151044,
            "unit": "ns",
            "range": "± 22032.205696922458"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "a77e774fa9492cbd1a8088ef9d3552f204b71faa",
          "message": "chore: park the MAUI app on feature/maui until it's ready (#931)\n\n* chore: park the MAUI app on feature/maui until it's ready\n\nThe MAUI app isn't done, so the first release ships the engine (NuGet) and the\nBlazor web app only. main drops src/Wolfgang.Hawsey.UI.Maui and its unit tests\n(and their Hawsey.slnx entries), plus the ten maui-*.md changelog fragments, so\nthe release notes don't describe an app that isn't shipping; three engine/Blazor\nfragments no longer mention it either. The README lists the Blazor projects and\npoints at feature/maui.\n\nKept: tests/Wolfgang.Hawsey.UI.Maui.Tests.Concurrency (Coyote tests of the\nengine's GameSession - it references only the engine), coyote.yaml, and the\nengine's InternalsVisibleTo for the MAUI tests.\n\nfeature/maui reverts this commit to bring the app back.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n\n* chore: changelog fragment for parking the MAUI app\n\nThe Changelog Fragment Check requires an added fragment when src/ changes; this\nPR removes src/Wolfgang.Hawsey.UI.Maui.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-09-30T10:38:23-04:00",
          "tree_id": "c0973caf7a53a2a2f547d207e35ccc9e24c029f6",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/a77e774fa9492cbd1a8088ef9d3552f204b71faa"
        },
        "date": 1790779182354,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 558.7125221888224,
            "unit": "ns",
            "range": "± 4.944845836141184"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 95.65185966094334,
            "unit": "ns",
            "range": "± 1.9556645199213616"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 288.50426657994586,
            "unit": "ns",
            "range": "± 1.8018507231478469"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 385235.53125,
            "unit": "ns",
            "range": "± 3847.9186272541947"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "1047ebee7a9595e8644563207fbd1614fe47357c",
          "message": "docs(engine): give the NuGet package its own readme (#936)\n\nv0.1.0 went to nuget.org with no readme (the push warned \"Readme\nmissing\"), so its page is blank. The package gets src/.../README.md:\nwhat the engine is, GameSession / GameEngine / GameRunner samples, a\nshort rules summary and links. It is not the repository README, which\ncovers the apps, CI and the template and uses repo-relative links that\nbreak on nuget.org. The csproj also gains PackageProjectUrl and\nPackageTags. The repository README stops saying the package is\nunpublished.\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-09-30T13:14:50-04:00",
          "tree_id": "9bb99ed5bc6ff19bb563e8939db81ffeb8a88304",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/1047ebee7a9595e8644563207fbd1614fe47357c"
        },
        "date": 1790788558045,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 472.1004063288371,
            "unit": "ns",
            "range": "± 2.9656715920665997"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 59.50312799215317,
            "unit": "ns",
            "range": "± 0.19839104821953954"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 200.13665334383646,
            "unit": "ns",
            "range": "± 0.3457963312479402"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 241068.83650716147,
            "unit": "ns",
            "range": "± 2704.348361714831"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "6d67920fc8a0e702f67badc6447fda3726d8839b",
          "message": "Bump the dotnet-dependencies group with 8 updates (#950)\n\nBumps coverlet.collector from 10.0.1 to 10.1.0\nBumps jetbrains.resharper.globaltools from 2026.2.2 to 2026.2.3.1\nBumps Meziantou.Analyzer from 3.0.290 to 3.0.294\nBumps Microsoft.AspNetCore.Components.Web from 10.0.0 to 10.0.12\nBumps Microsoft.AspNetCore.Components.WebAssembly from 10.0.0 to 10.0.12\nBumps Microsoft.AspNetCore.Components.WebAssembly.DevServer from 10.0.0 to 10.0.12\nBumps microsoft.cst.devskim.cli from 1.0.90 to 1.0.100\nBumps SonarAnalyzer.CSharp from 10.34.0.3385 to 10.35.0.4138\n\n---\nupdated-dependencies:\n- dependency-name: coverlet.collector\n  dependency-version: 10.1.0\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: coverlet.collector\n  dependency-version: 10.1.0\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: jetbrains.resharper.globaltools\n  dependency-version: 2026.2.3.1\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.294\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.Web\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.WebAssembly\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.WebAssembly.DevServer\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: microsoft.cst.devskim.cli\n  dependency-version: 1.0.100\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: SonarAnalyzer.CSharp\n  dependency-version: 10.35.0.4138\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Chris Wolfgang <210299580+Chris-Wolfgang@users.noreply.github.com>",
          "timestamp": "2026-10-04T19:30:31-04:00",
          "tree_id": "e260f5f982dad7177ea9a78cab01a70de751a147",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/6d67920fc8a0e702f67badc6447fda3726d8839b"
        },
        "date": 1791156698217,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 566.2644271850586,
            "unit": "ns",
            "range": "± 0.922463329546992"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 74.88915232817332,
            "unit": "ns",
            "range": "± 0.2468984095134897"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 249.02357784907022,
            "unit": "ns",
            "range": "± 1.0823288924679006"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 442573.40380859375,
            "unit": "ns",
            "range": "± 457.72083172273005"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "47bebf17056611e2e2c9be2e3fe06b75f75cab80",
          "message": "Bump the dotnet-dependencies group with 8 updates (#996)\n\nBumps cyclonedx from 6.2.0 to 6.3.1\nBumps FsCheck.Xunit from 3.4.0 to 3.5.0\nBumps Meziantou.Analyzer from 3.0.297 to 3.0.303\nBumps Microsoft.AspNetCore.Components.Web from 10.0.12 to 10.0.13\nBumps Microsoft.AspNetCore.Components.WebAssembly from 10.0.12 to 10.0.13\nBumps Microsoft.AspNetCore.Components.WebAssembly.DevServer from 10.0.12 to 10.0.13\nBumps Microsoft.SourceLink.GitHub from 10.0.401 to 10.0.402\nBumps Microsoft.VisualStudio.Threading.Analyzers from 18.7.23 to 18.12.1\n\n---\nupdated-dependencies:\n- dependency-name: cyclonedx\n  dependency-version: 6.3.1\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: FsCheck.Xunit\n  dependency-version: 3.5.0\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.303\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.Web\n  dependency-version: 10.0.13\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.WebAssembly\n  dependency-version: 10.0.13\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.AspNetCore.Components.WebAssembly.DevServer\n  dependency-version: 10.0.13\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.SourceLink.GitHub\n  dependency-version: 10.0.402\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.VisualStudio.Threading.Analyzers\n  dependency-version: 18.12.1\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-10-18T20:14:10-04:00",
          "tree_id": "c2157208e25fce9a0caafc0f7e3f5050f2529af5",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/47bebf17056611e2e2c9be2e3fe06b75f75cab80"
        },
        "date": 1792368927752,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 554.4286193847656,
            "unit": "ns",
            "range": "± 3.4385079218888204"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 91.18012611071269,
            "unit": "ns",
            "range": "± 0.08456875909705427"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 300.0522295633952,
            "unit": "ns",
            "range": "± 3.394366122407741"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 375716.2360026042,
            "unit": "ns",
            "range": "± 3803.182138553726"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "b3bf1415bb6c3e0ee837bea6ceb2267f7101089a",
          "message": "build(deps): bump ASP.NET Core to 10.0.14 and FsCheck.Xunit to 3.6.0 (non-protected half of #1028) (#1034)\n\n- Microsoft.AspNetCore.Components.WebAssembly / .DevServer 10.0.13 -> 10.0.14 (UI.Blazor)\n- Microsoft.AspNetCore.Components.Web 10.0.13 -> 10.0.14 (UI.Shared)\n- FsCheck.Xunit 3.5.0 -> 3.6.0 (Engine.Tests.Unit)\n- changelog/unreleased fragment (type: internal)\n\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-11-02T13:38:23-05:00",
          "tree_id": "71765e5dade9e13965f0dec370a450c22cd4d29d",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/b3bf1415bb6c3e0ee837bea6ceb2267f7101089a"
        },
        "date": 1793645623198,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 369.0159918467204,
            "unit": "ns",
            "range": "± 5.685552358944154"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 36.2522241572539,
            "unit": "ns",
            "range": "± 0.18415315657457973"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 137.3709487915039,
            "unit": "ns",
            "range": "± 1.8708449525732236"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 162950.89778645834,
            "unit": "ns",
            "range": "± 1037.7718261728162"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "988361d88090be05e6416761a0c99c2a208f9873",
          "message": "chore: drop issue-number references from code comments and project files (#1007) (#1010)\n\nA comment states the rule; the incident that led to it lives in the commit\nmessage and PR body. Each parenthesised issue number is removed, and where the\nsentence relied on it the reason is kept in words. No code changes.\n\nHawsey.slnx.DotSettings is left alone: it is template-synced, so the change\nbelongs in repo-template first.\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-04T16:20:06-05:00",
          "tree_id": "90c79bb57d81a85daf216deca142c2aa512aca7a",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/988361d88090be05e6416761a0c99c2a208f9873"
        },
        "date": 1793827286702,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 548.654569307963,
            "unit": "ns",
            "range": "± 3.1794588557565584"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 92.9927312930425,
            "unit": "ns",
            "range": "± 1.7357768097020958"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 285.4967101414998,
            "unit": "ns",
            "range": "± 1.2594831794680903"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 380261.52913411456,
            "unit": "ns",
            "range": "± 1432.2590744472463"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "b24016862328e8b818d2b429c84150d6fd05070c",
          "message": "chore: project-file hygiene (#1006, 1/2) (#1017)\n\n* chore: drop issue-number references from code comments and project files (#1007)\n\nA comment states the rule; the incident that led to it lives in the commit\nmessage and PR body. Each parenthesised issue number is removed, and where the\nsentence relied on it the reason is kept in words. No code changes.\n\nHawsey.slnx.DotSettings is left alone: it is template-synced, so the change\nbelongs in repo-template first.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* chore: project-file hygiene (#1006)\n\n- Engine tests: xunit.runner.visualstudio 3.0.1 on net8.0+, and 2.8.2 capped\n  with allowedVersions=\"(,3.0.0)\" on net462-net481, net6.0 and net7.0, so a\n  Dependabot bump cannot move those slots to a runner that drops them\n  (netcoreapp3.1/net5.0 keep 2.4.5, capped below 2.5.0).\n- Engine csproj: drop <Authors>, which Directory.Build.props already sets; add\n  <RepositoryUrl>, which Directory.Build.props leaves to each src project.\n- UI.Blazor / UI.Shared csproj: drop <Authors> and the year-less <Copyright>\n  that overrode the year-stamped value from Directory.Build.props.\n- Remove src/, tests/, benchmarks/ and examples/.gitkeep: each directory has\n  held real projects since 0.1.0.\n- CHANGELOG.md: add the Keep a Changelog link references for [Unreleased]\n  and [0.1.0].\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-04T21:38:00-05:00",
          "tree_id": "956a31deb01cd53d666a6bffa07fa97242e2acb7",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/b24016862328e8b818d2b429c84150d6fd05070c"
        },
        "date": 1793846375778,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 474.31538009643555,
            "unit": "ns",
            "range": "± 1.5617500315157482"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 59.93132162094116,
            "unit": "ns",
            "range": "± 0.19521999246107172"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 218.41302820046744,
            "unit": "ns",
            "range": "± 4.044812566540248"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 253252.71940104166,
            "unit": "ns",
            "range": "± 3555.334536816175"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "9e727755424343efdb1397b50cc836cf87743599",
          "message": "test: Method_when_condition names and formatting from CLAUDE.md (#1008) (#1020)\n\n* test: formatting drift from CLAUDE.md (#1008)\n\n- No leading blank line in eleven test files.\n- TestPlayerStrategy: multi-line parameter lists with the parentheses on their\n  own lines.\n- GameEngineTests: `new()` for the engine field; the nested loops that looked\n  for a card outside the bidder's hand become\n  Deck.CreatePinochleDeck().First(c => !bidderHand.Contains(c)), as the same\n  file already does elsewhere.\n- TrickTests: the two out-of-range constructor checks become a [Theory] with\n  InlineData(1) and InlineData(5).\n- SimpleAiStrategyBiddingAndLeadTests: the \"beats a lower high bid and passes\n  otherwise\" [Fact] is split into one test per case.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* test: name every test Method_when_condition_expected_result (#1008)\n\n146 test methods used sentence names or omitted the condition. Each is\nrenamed to the CLAUDE.md pattern, naming the member under test first; no\ntest body changes. The Coyote csproj comment that names one of them follows.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-04T22:03:28-05:00",
          "tree_id": "9e7929f631423916df5c92300ea0e9f6c4a934c6",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/9e727755424343efdb1397b50cc836cf87743599"
        },
        "date": 1793847889738,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 556.9026981989542,
            "unit": "ns",
            "range": "± 1.9493762062022504"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 95.94861276944478,
            "unit": "ns",
            "range": "± 0.6009977447780506"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 291.50137519836426,
            "unit": "ns",
            "range": "± 0.9264265660578347"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 395752.3030598958,
            "unit": "ns",
            "range": "± 201.79068162437298"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "1a0994574a220078ebcd3f5191f6181df86107e6",
          "message": "fix(engine): PerformHawseyExchange refuses invalid cards; name the Hawsey score (#1004) (#1011)\n\nGameSession.PerformHawseyExchange checked only the phase and the bidder, so a\nwrong count or a card not held reached GameEngine.ExchangeHawseyCards and threw\nInvalidOperationException. Every other human move refuses with false. It now\nreturns false, and changes nothing, unless both arrays are two cards the right\nplayer holds (the same Holds check PerformHumanHawseyExchange uses); a null\narray still throws ArgumentNullException, as for PerformHumanHawseyExchange.\n\nGameState.TricksPerRound (12) and GameState.HawseyBidAmount (24) replace the\nliterals in GameEngine and RoundScore, so a UI can read them too.\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-05T08:22:42-05:00",
          "tree_id": "5e4927fd577c1bc5a8e66d6d8b81c1051ea903af",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/1a0994574a220078ebcd3f5191f6181df86107e6"
        },
        "date": 1793885037601,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 550.3775625228882,
            "unit": "ns",
            "range": "± 1.5728595598461441"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 86.00299276908238,
            "unit": "ns",
            "range": "± 1.1332355762431563"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 278.89576721191406,
            "unit": "ns",
            "range": "± 0.28523592942472265"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 460002.0006510417,
            "unit": "ns",
            "range": "± 6049.479665283507"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "8e33d89dbb417e9da981f42b7521098c9303c4ba",
          "message": "fix(ui): read the bid range and Hawsey score from the engine (#1004, 2/2) (#1012)\n\n* fix(engine): PerformHawseyExchange refuses invalid cards; name the Hawsey score (#1004)\n\nGameSession.PerformHawseyExchange checked only the phase and the bidder, so a\nwrong count or a card not held reached GameEngine.ExchangeHawseyCards and threw\nInvalidOperationException. Every other human move refuses with false. It now\nreturns false, and changes nothing, unless both arrays are two cards the right\nplayer holds (the same Holds check PerformHumanHawseyExchange uses); a null\narray still throws ArgumentNullException, as for PerformHumanHawseyExchange.\n\nGameState.TricksPerRound (12) and GameState.HawseyBidAmount (24) replace the\nliterals in GameEngine and RoundScore, so a UI can read them too.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* fix(ui): read the bid range and Hawsey score from the engine (#1004)\n\n- The bid picker stopped at 11 while the engine accepts a number bid up to\n  GameState.MaximumBid (12); it now runs to GameState.MaximumBid.\n- The no-state fallback minimum is HouseRules.Default.MinimumBid, not 6.\n- \"Hawsey (24)\" is built once from GameState.HawseyBidAmount.\n- OnCardClickAsync no longer scans the legal plays before PlayHumanCard,\n  which already checks the turn and the card once and returns false.\n- The dead `|| hand is null` after TryGetValue on a non-nullable List is gone.\n- index.html drops user-scalable=no, which blocked pinch-zoom (WCAG 1.4.4).\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-11-05T08:55:22-05:00",
          "tree_id": "10d245df7d0eabfe51e9c13d7708114a7286b4c6",
          "url": "https://github.com/Chris-Wolfgang/Hawsey/commit/8e33d89dbb417e9da981f42b7521098c9303c4ba"
        },
        "date": 1793887197363,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.Shuffle",
            "value": 480.185866355896,
            "unit": "ns",
            "range": "± 3.6520236445631697"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.GetLegalPlays",
            "value": 62.02043652534485,
            "unit": "ns",
            "range": "± 0.39932449689509936"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.SortHandByStrength",
            "value": 206.71052730083466,
            "unit": "ns",
            "range": "± 0.4342447823192027"
          },
          {
            "name": "Wolfgang.Hawsey.Engine.Benchmarks.EngineBenchmarks.PlayFullGame",
            "value": 263873.685546875,
            "unit": "ns",
            "range": "± 1097.6543347565039"
          }
        ]
      }
    ]
  }
}