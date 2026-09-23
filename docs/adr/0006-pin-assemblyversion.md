# ADR 0006 — Pin the engine's `<AssemblyVersion>` at `1.0.0.0`

- **Status:** Accepted
- **Date:** 2026-09-22 (records the setting in the engine's `.csproj`)
- **Deciders:** Chris Wolfgang

## Context

.NET Framework (`net462` to `net481`) consumers bind strong assembly versions
exactly. If `AssemblyVersion` changes on every release, a consumer that picks up
a newer engine through another dependency needs a recompile or a binding
redirect. The engine multi-targets `netstandard2.0`, so Framework consumers are
in scope.

## Decision

`<AssemblyVersion>` is fixed at `1.0.0.0` and changes only on a deliberate
binary-breaking change. `<FileVersion>` is derived from `<Version>`, and
`InformationalVersion` (auto-derived from `<Version>`) carries the real release
number.

## Alternatives considered

- **AssemblyVersion = package version** — the SDK default, but it causes binding
  churn on every release for Framework consumers.
- **Pin to `0.{minor}`** — the fleet convention for *new* libraries. The engine
  already shipped `1.0.0.0` in its first build, and moving to `0.x` would itself
  break binding.

## Consequences

- A binary-breaking release must decide deliberately whether to bump `AssemblyVersion`.
- Tools that read `AssemblyVersion` see `1.0.0.0` whatever the package version.
  Use `FileVersion` or `InformationalVersion` to identify a build.
