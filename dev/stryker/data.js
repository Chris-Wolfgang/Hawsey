window.BENCHMARK_DATA = {
  "lastUpdate": 1791699839130,
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
      }
    ]
  }
}