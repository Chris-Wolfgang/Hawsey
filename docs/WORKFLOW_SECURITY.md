# Workflow Security

## Overview

This document describes the security measures in the GitHub Actions workflows for this repository, mainly the PR validation workflow (`.github/workflows/pr.yaml`) and the protected-files guard (`.github/workflows/protected-files.yaml`).

## Security Architecture

### 1. PR validation runs on `pull_request`

`pr.yaml` runs on `pull_request` (and on push to `main`):

```yaml
on:
  pull_request:
    branches:
      - main
  push:
    branches:
      - main
```

A PR is built and tested with its own code and its own copies of the workflow and every analyzer and configuration file. GitHub gives a `pull_request` run a read-only `GITHUB_TOKEN` and no secrets for forks, and nothing in `pr.yaml` uses a secret. Untrusted PR code therefore has nothing to reach.

Because a PR is validated with its own configuration, a PR could weaken a check and pass it in the same change. The protected-files guard (next section) closes that gap.

### 2. Protected configuration files

**Mechanism**: `protected-files.yaml`, the one workflow that runs on `pull_request_target`.

It runs from `main`, so a PR cannot edit it. It never checks out or executes PR content. It lists the PR's changed files through the API and classifies the PR:

| PR changes | Result |
|---|---|
| No protected file | Pass. `pr.yaml` validated everything. |
| Only protected files | Pass with a notice. A maintainer reviews the diff. |
| Protected files and anything else | **Fail.** Split the PR. |

**Protected files**:
- `.editorconfig` (and any `*.editorconfig`)
- `Directory.Build.props`, `Directory.Build.targets`, at the root or in any folder
- `BannedSymbols.txt`
- `*.globalconfig`, `*.ruleset`, `*.DotSettings`
- `coverlet.runsettings`
- `.config/dotnet-tools.json`
- `.gitleaks.toml`
- `.github/workflows/*.yml` and `.github/workflows/*.yaml`
- `.github/license-audit/*.json`
- `.github/requirements/*`
- The CI scripts: `scripts/build-pr.ps1`, `scripts/changelog.ps1`, `scripts/tfm-parity.ps1`, `scripts/third-party-notices.ps1`

Names match case-insensitively, because Windows and macOS resolve `directory.build.props` to the same file.

Dependabot PRs are exempt; their bumps to files such as `Directory.Build.props` are legitimate.

### 3. Credential Protection

**Mechanism**: `persist-credentials: false`

Every checkout step in `pr.yaml` sets `persist-credentials: false`, so the checkout token is not written to git config:

```yaml
- name: Checkout code
  uses: actions/checkout@<sha>  # vX
  with:
    persist-credentials: false
```

**Note**: This keeps the token out of git config. It does NOT stop a step from using `GITHUB_TOKEN` if the workflow passes it in explicitly.

### 4. Minimal Permissions

`pr.yaml` runs with read-only permissions:

```yaml
permissions:
  contents: read
```

The only write scope in `pr.yaml` is `security-events: write` on the `inspectcode-upload` job, which never checks out PR code. `protected-files.yaml` has `contents: read` and `pull-requests: read`.

## Attack Scenarios

### Scenario 1: Malicious workflow modification
**Attack**: A PR edits `.github/workflows/pr.yaml` to disable checks, together with code that would fail them.
**Prevention**: The workflow file is protected, so the guard fails the mixed PR. A workflow-only PR merges only after maintainer review.

### Scenario 2: Configuration file tampering
**Attack**: A PR edits `.editorconfig` or `BannedSymbols.txt` to disable analyzers or allow banned APIs, together with code that needs the change.
**Prevention**: Same as scenario 1.

### Scenario 3: Credential theft
**Attack**: PR code tries to read GitHub credentials.
**Prevention**: No secrets in `pr.yaml`, a read-only token, and `persist-credentials: false`.

## Maintenance

### Making changes to protected configuration files

1. **Open a configuration-only PR.** It contains the protected files and nothing else. The guard passes it with a notice, and `pr.yaml` runs with the changed files.
2. **Review the diff and merge.** This change decides what later PRs are checked against.
3. **Rebase dependent code changes onto `main`** and open them as a separate PR, validated under the reviewed configuration.

If you need to relax a rule and add code that breaks the old rule, that is two PRs: the configuration change first, then the code.

### Adding a new protected file

1. Add the path to the `case` pattern (or the `grep` pattern) in `.github/workflows/protected-files.yaml`.
2. Update the list in this document.

## References

- [GitHub Actions Security Hardening](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [Keeping your GitHub Actions secure](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions#using-third-party-actions)
- [Understanding pull_request_target](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/)
