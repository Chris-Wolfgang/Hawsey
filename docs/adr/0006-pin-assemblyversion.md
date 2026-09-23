# ADR 0006 — Pin the engine's `<AssemblyVersion>` at `1.0.0.0`

- **Status:** Accepted
- **Date:** 2026-09-22 (records the setting in the engine's `.csproj`)
- **Deciders:** Chris Wolfgang

## Context

The engine's `.csproj` fixes `<AssemblyVersion>` at `1.0.0.0`, and its comment
gives binding stability as the reason. Exact version binding and binding
redirects apply only to **strong-named** assemblies on .NET Framework. The
engine is **not** strong-named (no `SignAssembly` or key), so today the pin has
**no binding effect**. .NET Framework loads a weak-named assembly regardless of
its `AssemblyVersion`.

## Decision

Keep `<AssemblyVersion>` at `1.0.0.0` for now, because it's harmless and ready
if the engine is ever strong-named, but don't rely on it for anything.
`<FileVersion>` (derived from `<Version>`) and `InformationalVersion` carry the
real release number. If the engine is strong-named, this pin becomes
load-bearing, and the signing change must revisit it in a new ADR.

## Alternatives considered

- **AssemblyVersion = package version (the SDK default)** — equally valid while
  the engine is weak-named. Switching now is churn with no consumer benefit.
- **`0.{minor}` (the fleet convention for new libraries)** — the engine already
  shipped `1.0.0.0`. Changing it has no effect today and would matter only after
  strong-naming.

## Consequences

- The `.csproj` comment's binding-stability reason is aspirational until the
  engine is strong-named. Read it with this ADR.
- Tools that read `AssemblyVersion` see `1.0.0.0` whatever the release. Use
  `FileVersion` or `InformationalVersion` to identify a build.
