window.BENCHMARK_DATA = {
  "lastUpdate": 1790728269727,
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
      }
    ]
  }
}