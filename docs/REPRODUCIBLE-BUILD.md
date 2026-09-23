# Verify the build

Anyone can rebuild `Wolfgang.Hawsey.Engine` from source and confirm that the
assemblies inside the published NuGet package are **byte-for-byte** what the
tagged source compiles to. This guide covers:

1. [Checking the provenance attestation](#1-check-the-provenance-attestation): proves who built the package, from which commit.
2. [Rebuilding and comparing hashes](#2-rebuild-and-compare-hashes): proves the published code matches the source.
3. [Reporting the result](#3-report-the-result): records a third-party verification, or a discrepancy.

## What is reproducible, and what isn't

- **The assemblies are reproducible:** `lib/netstandard2.0/Wolfgang.Hawsey.Engine.dll`
  and `lib/net10.0/Wolfgang.Hawsey.Engine.dll`. CI builds are deterministic
  (`Deterministic` plus `ContinuousIntegrationBuild`, which also normalises
  source paths to `/_/`). Two independent clones of the same commit at different
  paths produce identical DLLs. CI checks this on every change (#82).
- **The `.nupkg` file is not.** NuGet writes zip metadata that varies per pack,
  so two packs of identical DLLs hash differently. Compare the DLLs *inside* the
  package, never the package's own hash.

## Tooling

The build depends on the compiler, so use the **exact SDK** the release used.
Each GitHub release attaches `reproducible-build-manifest.json`:

```json
{
  "package": "Wolfgang.Hawsey.Engine",
  "version": "0.2.0",
  "commit": "<40-char commit SHA the tag points at>",
  "sdk": "10.0.400",
  "copyright": "Copyright (c) 2026 Chris Wolfgang",
  "assemblies": {
    "lib/netstandard2.0/Wolfgang.Hawsey.Engine.dll": "<sha256>",
    "lib/net10.0/Wolfgang.Hawsey.Engine.dll": "<sha256>"
  }
}
```

`copyright` is recorded because `Directory.Build.props` stamps the current year
into the assembly. A rebuild in a later year must pass the recorded value back in
(step 2). The expected hashes for a release are the `assemblies` values.

## 1. Check the provenance attestation

The release workflow signs a SLSA build-provenance attestation for every
`.nupkg` before publishing, using GitHub's keyless (OIDC) signing:

```bash
nuget install Wolfgang.Hawsey.Engine -Version 0.2.0 -OutputDirectory pkg   # or download from nuget.org
gh attestation verify pkg/Wolfgang.Hawsey.Engine.0.2.0/Wolfgang.Hawsey.Engine.0.2.0.nupkg \
  --repo Chris-Wolfgang/Hawsey \
  --signer-workflow Chris-Wolfgang/Hawsey/.github/workflows/release.yaml
```

A pass proves the package bytes came from `release.yaml` in this repository at
the commit named in the attestation. That commit must match the manifest's
`commit`.

## 2. Rebuild and compare hashes

```bash
git clone https://github.com/Chris-Wolfgang/Hawsey.git && cd Hawsey
git checkout <commit from the manifest>

# Install exactly the manifest's SDK (https://dot.net/v1/dotnet-install.sh or .ps1)
./dotnet-install.sh --version <sdk from the manifest>
dotnet --version        # must print the manifest's sdk

# CI=true turns on ContinuousIntegrationBuild (deterministic paths).
CI=true dotnet build src/Wolfgang.Hawsey.Engine -c Release \
  -p:Copyright="<copyright from the manifest>"

sha256sum src/Wolfgang.Hawsey.Engine/bin/Release/netstandard2.0/Wolfgang.Hawsey.Engine.dll \
          src/Wolfgang.Hawsey.Engine/bin/Release/net10.0/Wolfgang.Hawsey.Engine.dll
```

Then hash the DLLs inside the published package (a `.nupkg` is a zip file):

```bash
unzip -o -q Wolfgang.Hawsey.Engine.0.2.0.nupkg 'lib/*' -d published
sha256sum published/lib/*/Wolfgang.Hawsey.Engine.dll
```

All three sets must match: your rebuild, the published package and the manifest.

## 3. Report the result

Use the **Reproducible build report** issue form in this repository for either
outcome. Include the version, your OS, `dotnet --version`, and the three sets of
hashes.

- **Match (third-party verification):** a report from someone other than the
  maintainer is the independent attestation. To make it tamper-evident, also
  sign the manifest with your own Sigstore identity and attach the bundle:

  ```bash
  cosign sign-blob --yes --bundle manifest.sigstore.json reproducible-build-manifest.json
  ```

  Anyone can then check it with `cosign verify-blob --bundle manifest.sigstore.json
  --certificate-identity <you> --certificate-oidc-issuer <issuer> reproducible-build-manifest.json`.
  This follows the [Reproducible Builds](https://reproducible-builds.org/) convention
  of independent builders publishing signed statements over the same artifact
  hashes.
- **Mismatch:** file the form with the differing hashes. If you can, also attach
  both DLLs, or the output of a decompiler diff such as `ilspycmd`. A mismatch on
  the maintainer's own tooling is treated as a release-blocking bug.
