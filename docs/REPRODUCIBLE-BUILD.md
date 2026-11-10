# Verify the build

Anyone can rebuild `Wolfgang.Hawsey.Engine` from source and confirm that the
assemblies inside the published NuGet package are **byte-for-byte** what the
tagged source compiles to. This guide covers:

1. [Checking the provenance attestation](#1-check-the-provenance-attestation): proves who built the package, from which commit.
2. [Rebuilding and comparing hashes](#2-rebuild-and-compare-hashes): proves the published code matches the source.
3. [Reporting the result](#3-report-the-result): records a third-party verification, or a discrepancy.

## What is reproducible, and what isn't

- **The assemblies and symbols are reproducible:**
  `lib/netstandard2.0/Wolfgang.Hawsey.Engine.dll` and
  `lib/net10.0/Wolfgang.Hawsey.Engine.dll`, and their `.pdb` files. CI builds are
  deterministic (`Deterministic` plus `ContinuousIntegrationBuild`, which also
  normalises source paths to `/_/`). Two independent clones of the same commit at
  different paths produce identical DLLs and PDBs. CI checks this on every change
  by building twice on Windows and twice on Linux, then comparing within each OS
  and between Windows and Linux (#82, #997).
- **Any OS works, from the first release built with #997.** MSBuild writes the
  sources it generates (`AssemblyInfo.cs`, `GlobalUsings.g.cs`, the
  target-framework attributes) with the OS line ending, CRLF on Windows and LF on
  Linux and macOS. Their checksums go into the PDB, and the DLL records the PDB's
  ID. `Directory.Build.targets` now rewrites those files to LF before compiling,
  so a Linux and a Windows rebuild of the same commit give the same bytes.
  Releases built before that change only reproduce on the release OS: Windows
  (the manifest's `os` field).
- **Use the manifest's exact SDK, installed privately.** The PDB also records the
  version of the .NET runtime the compiler ran on. A machine-wide install can roll
  the compiler forward to a newer runtime and change the PDB, and with it the DLL.
  The private install in step 2 avoids this.
- **The `.nupkg` file is not reproducible.** NuGet writes zip metadata that varies
  per pack, so two packs of identical DLLs hash differently. Compare the DLLs
  *inside* the package, never the package's own hash.

## Tooling

The build depends on the compiler, so use the **exact SDK** the release used.
Each GitHub release attaches one manifest per package, named
`<PackageId>.<version>.reproducible-build-manifest.json` (for example
`Wolfgang.Hawsey.Engine.0.2.0.reproducible-build-manifest.json`). Below it's
called `manifest.json`:

```json
{
  "package": "Wolfgang.Hawsey.Engine",
  "version": "0.2.0",
  "commit": "<40-char commit SHA the tag points at>",
  "sdk": "10.0.400",
  "os": "windows",
  "copyright": "Copyright (c) 2026 Chris Wolfgang",
  "assemblies": {
    "lib/netstandard2.0/Wolfgang.Hawsey.Engine.dll": "<sha256>",
    "lib/net10.0/Wolfgang.Hawsey.Engine.dll": "<sha256>"
  }
}
```

`copyright` is recorded because releases built before #1107 stamped the build
year into the assembly, so a rebuild in a later year must pass the recorded value
back in (step 2). From #1107 on, `Directory.Build.props` holds a literal copyright
and passing it back in changes nothing. The expected hashes for a release are the
`assemblies` values.

From the first release built with #1113, the release also attaches
`SHA256SUMS.txt`, in the format `sha256sum` reads. It has one line for each
`.nupkg` and `.snupkg`, and one line for each assembly and PDB inside them, named
`<package-file>/<entry>`:

```text
<sha256>  Wolfgang.Hawsey.Engine.0.2.0.nupkg
<sha256>  Wolfgang.Hawsey.Engine.0.2.0.nupkg/lib/net10.0/Wolfgang.Hawsey.Engine.dll
<sha256>  Wolfgang.Hawsey.Engine.0.2.0.snupkg
<sha256>  Wolfgang.Hawsey.Engine.0.2.0.snupkg/lib/net10.0/Wolfgang.Hawsey.Engine.pdb
```

Its DLL lines carry the same hashes as the manifest's `assemblies`; its PDB lines
add the symbols, so a rebuild can be checked for both.

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

From the first release built with #1113, the same attestation also covers
`SHA256SUMS.txt` and the manifest. Check them with the same command before
trusting the hashes they list:

```bash
gh attestation verify SHA256SUMS.txt \
  --repo Chris-Wolfgang/Hawsey \
  --signer-workflow Chris-Wolfgang/Hawsey/.github/workflows/release.yaml
```

## 2. Rebuild and compare hashes

```bash
git clone https://github.com/Chris-Wolfgang/Hawsey.git && cd Hawsey
git checkout <commit from the manifest>

# Install exactly the manifest's SDK into a private directory and put it first
# on PATH, so no machine-wide SDK can be picked up instead.
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --version <sdk from the manifest> --install-dir "$HOME/.dotnet-repro"
export DOTNET_ROOT="$HOME/.dotnet-repro" PATH="$HOME/.dotnet-repro:$PATH"
dotnet --version        # must print the manifest's sdk

# CI=true turns on ContinuousIntegrationBuild (deterministic paths).
CI=true dotnet build src/Wolfgang.Hawsey.Engine -c Release \
  -p:Copyright="<copyright from the manifest>"

sha256sum src/Wolfgang.Hawsey.Engine/bin/Release/netstandard2.0/Wolfgang.Hawsey.Engine.dll \
          src/Wolfgang.Hawsey.Engine/bin/Release/net10.0/Wolfgang.Hawsey.Engine.dll
```

On Windows (PowerShell) the install step is:

```powershell
Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
./dotnet-install.ps1 -Version <sdk from the manifest> -InstallDir "$env:USERPROFILE\.dotnet-repro"
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet-repro"; $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$env:CI = 'true'; dotnet build src/Wolfgang.Hawsey.Engine -c Release -p:Copyright="<copyright from the manifest>"
Get-FileHash src/Wolfgang.Hawsey.Engine/bin/Release/*/Wolfgang.Hawsey.Engine.dll -Algorithm SHA256
```

Then hash the DLLs inside the published package (a `.nupkg` is a zip file):

```bash
unzip -o -q Wolfgang.Hawsey.Engine.0.2.0.nupkg -d published
sha256sum published/lib/*/Wolfgang.Hawsey.Engine.dll
```

All three sets must match: your rebuild, the published package and the manifest.

With a `SHA256SUMS.txt` (releases built with #1113 and later), check the
downloaded packages directly, then compare each rebuilt DLL and PDB against the
line for its entry:

```bash
grep -E 'pkg$' SHA256SUMS.txt | sha256sum -c      # the .nupkg and .snupkg you downloaded
for tfm in netstandard2.0 net10.0; do
  for ext in dll pdb; do
    pkg=$([ "$ext" = dll ] && echo nupkg || echo snupkg)
    want=$(grep " Wolfgang.Hawsey.Engine.0.2.0.$pkg/lib/$tfm/Wolfgang.Hawsey.Engine.$ext\$" SHA256SUMS.txt | cut -d' ' -f1)
    got=$(sha256sum "src/Wolfgang.Hawsey.Engine/bin/Release/$tfm/Wolfgang.Hawsey.Engine.$ext" | cut -d' ' -f1)
    [ "$want" = "$got" ] && echo "OK   $tfm $ext" || echo "DIFF $tfm $ext"
  done
done
```

## 3. Report the result

Use the **Reproducible build report** issue form in this repository for either
outcome. Include the version, your OS, `dotnet --version`, and the three sets of
hashes.

- **Match (third-party verification):** a report from someone other than the
  maintainer is the independent attestation. To make it tamper-evident, also
  sign the manifest with your own Sigstore identity and attach the bundle:

  ```bash
  cosign sign-blob --yes --bundle manifest.sigstore.json manifest.json
  ```

  Anyone can then check it with `cosign verify-blob --bundle manifest.sigstore.json
  --certificate-identity <you> --certificate-oidc-issuer <issuer> manifest.json`.
  This follows the [Reproducible Builds](https://reproducible-builds.org/) convention
  of independent builders publishing signed statements over the same artifact
  hashes.
- **Mismatch:** file the form with the differing hashes. If you can, also attach
  both DLLs, or the output of a decompiler diff such as `ilspycmd`. A mismatch on
  the maintainer's own tooling is treated as a release-blocking bug.
