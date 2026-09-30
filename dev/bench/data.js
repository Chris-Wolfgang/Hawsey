window.BENCHMARK_DATA = {
  "lastUpdate": 1790788560729,
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
      }
    ]
  }
}