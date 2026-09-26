# Release Workflow Setup Guide

This guide explains how this repository's `release.yaml` workflow is configured and what it does.

## Overview

The release workflow triggers when you **publish a GitHub Release**. It:
- ✅ Checks that the release tag matches a `<Version>` in a `src/` csproj
- ✅ Tests all target frameworks per test project on Windows
- ✅ Enforces line coverage: 90% for `src/` assemblies, 100% for test assemblies
- ✅ Packs the NuGet packages and smoke-tests installing them
- ✅ Verifies the documentation builds
- ✅ Attests build provenance and publishes to NuGet.org through Trusted Publishing
- ✅ Deploys the documentation and attaches the packages, SBOMs, reproducible-build manifests and coverage report to the GitHub Release

## Required Configuration

### NuGet Trusted Publishing

The workflow does not use a stored NuGet API key. The `publish-nuget` job runs `NuGet/login` with the job's OIDC identity and receives a short-lived API key for that run.

For this to work, nuget.org needs a **Trusted Publishing policy** for this repository:

1. Sign in to nuget.org as the package owner (`Chris-Wolfgang`, the `user` passed to `NuGet/login`).
2. Open **Trusted Publishing** from the account menu and add a policy.
3. Set the repository owner to `Chris-Wolfgang`, the repository to `Hawsey`, and the workflow file to `release.yaml`.

### Branch Protection

**Location:** Settings → Rules → Rulesets

This repository has no setup script for branch protection; the `main` ruleset is configured by hand. It has these rules:

- ✅ **Require a pull request before merging**
- ✅ **Require status checks to pass before merging**:
  - "Secrets Scan (gitleaks)"
  - "Detect .NET Projects"
  - "Stage 1: Linux Tests (.NET 5.0-10.0) + Coverage Gate"
  - "Stage 2: Windows Tests (.NET 5.0-10.0, Framework 4.6.2-4.8.1)"
  - "Stage 3: macOS Tests (.NET 6.0-10.0)"
  - "Security Scan (DevSkim)"
  - "Security Scan (CodeQL) (csharp)"
  - "Protected Files Guard"
- ✅ **Require code scanning results**
- ✅ **Restrict deletions** and **block force pushes**
- ✅ **Require linear history**

## Workflow Jobs

```
Trigger: published GitHub Release
  │
  ├─ validate-release (Windows)
  │    • Check the tag matches a src/ csproj <Version>
  │    • Build, test every framework, collect coverage
  │    • Enforce the coverage thresholds; upload the coverage report
  │
  ├─ pack-and-validate (Windows)          needs: validate-release
  │    • Pack the NuGet packages
  │    • Write the reproducible-build manifests
  │    • Smoke-test installing each package
  │
  ├─ verify-docs-build (Windows)          needs: validate-release, pack-and-validate
  │
  ├─ publish-nuget (Windows)              needs: pack-and-validate, verify-docs-build
  │    • Attest build provenance for each .nupkg
  │    • NuGet/login (OIDC), then push to NuGet.org
  │
  ├─ trigger-docs                         needs: validate-release
  │    • Calls docfx.yaml to build and deploy the docs to GitHub Pages
  │
  └─ update-release-artifacts             needs: validate-release, pack-and-validate, publish-nuget
       • Attach packages, SBOMs, manifests and the coverage report to the release
```

## Before a Release

- [ ] All PRs for the release are merged to `main`, and `pr.yaml` is green on `main`
- [ ] The `<Version>` in `src/Wolfgang.Hawsey.Engine/Wolfgang.Hawsey.Engine.csproj` is the version you will tag
- [ ] `CHANGELOG.md` has the new version section, assembled from the fragments in `changelog/unreleased/` (`pwsh ./scripts/changelog.ps1 assemble`)
- [ ] Local build succeeds: `dotnet build --configuration Release`
- [ ] Local tests pass: `dotnet test --configuration Release`

**Create the release:**
1. Go to the repository's **Releases** page
2. Click **"Draft a new release"**
3. Create the version tag (e.g., `v0.1.0`) targeting `main`
4. Add a title and release notes
5. Click **"Publish release"**

**After the workflow completes:**
- [ ] The package appears on NuGet.org
- [ ] The docs are live under `versions/<tag>/` and `versions/latest/`
- [ ] The release has the packages, `*.bom.json`, `*.reproducible-build-manifest.json` and `release-coverage.zip` attached

## Troubleshooting

### "Release tag ... does not match any src csproj version"

Bump the csproj `<Version>` or correct the release tag, then re-run the workflow.

### NuGet login or push fails

Check that the Trusted Publishing policy on nuget.org names this repository and `release.yaml`, and that the package owner matches the `user` in the `NuGet/login` step. Re-run the failed jobs from the Actions tab; do not re-publish the release.

### Tests fail on a specific framework

1. Check the test logs for framework-specific issues
2. Reproduce locally: `dotnet test --framework net462`
3. Merge the fix, then re-run the workflow

### Coverage below the threshold

1. Download the `release-coverage` artifact and read `Summary.txt`
2. Add tests for the uncovered code
3. Merge the fix, then re-run the workflow

### Smoke test fails to install the package

1. Check the package dependencies in the `.csproj`
2. Test locally: `dotnet pack`, then install the package into a test project
3. Merge the fix, then re-run the workflow

## Support

If you encounter issues not covered in this guide:

1. Check the Actions tab of this repository on GitHub for detailed logs
2. Review artifacts uploaded by failed jobs
3. Consult the [GitHub Actions documentation](https://docs.github.com/en/actions)
4. Open an issue in this repository with:
   - Workflow run URL
   - Error message and logs
   - Steps to reproduce
