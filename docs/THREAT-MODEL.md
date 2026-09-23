# Threat model — Wolfgang.Hawsey.Engine

- **Last reviewed:** 2026-09-23
- **Next review due:** 2027-09-23, or at the next major release, whichever comes first
- **Scope:** the `Wolfgang.Hawsey.Engine` NuGet package (public API and runtime
  behaviour) and the pipeline that builds and publishes it. The UI projects are
  local apps and are covered only where they feed the engine.

## System summary

The engine is a pure, in-process .NET library. It does **no** I/O, networking,
persistence, dynamic code generation or deserialization, and it is
declared trim- and AOT-compatible (see ADR 0002 and #79). Callers drive it by
passing a `GameState` and a move. Randomness comes only from a
`System.Random` the caller supplies. Decisions come from the caller's own code,
either directly or through `IPlayerStrategy` (`GameRunner`).

**Trust boundary:** everything inside the host process is trusted. The engine
defends against *incorrect* moves (wrong turn, illegal card, bid below the
minimum). It does not defend against a malicious host, which already has full
control of the process.

## Assets

| Asset | Why it matters |
|---|---|
| Correct game rules | Every UI and AI relies on the engine to decide legality and scoring |
| Integrity of the published package | Consumers run the engine's code in their process |
| Release credentials | The NuGet publish identity (OIDC trusted publishing, #114) and repository write access |

## STRIDE analysis

| # | Category | Threat | Mitigation | Status |
|---|---|---|---|---|
| S1 | Spoofing | A caller acts for a player out of turn | `GameEngine` and `BiddingPhase` check `NextToAct` / bidding order and throw `InvalidOperationException` | Mitigated |
| S2 | Spoofing | A look-alike or typosquatted package | Releases carry a signed SLSA build-provenance attestation (`release.yaml`); consumers can verify it (#91) | Mitigated; author signing blocked on a certificate (#120) |
| T1 | Tampering | A caller edits `GameState.Hands` / `CompletedTricks` (exposed as mutable collections) to cheat | Documented as read-only on `GameState`; see ADR 0002 | **Accepted:** the host is trusted and controls the process anyway |
| T2 | Tampering | A compromised dependency or CI action injects code into the build | Actions pinned by commit SHA; Dependabot; license audit; Semgrep and CodeQL; gitleaks; protected-files guard on CI configuration | Mitigated |
| T3 | Tampering | A rebuilt package differs from the reviewed source | Deterministic CI builds (`ContinuousIntegrationBuild`) plus a reproducibility check (#82); provenance attestation | Mitigated once #82 lands |
| R1 | Repudiation | A player disputes a move | Out of scope for a local library. A networked host must keep its own signed move log | **Accepted** |
| I1 | Information disclosure | Any strategy or UI can read **all four hands** from `GameState` (an AI could peek) | None in the engine, by design. The engine is a referee, not a hidden-information server | **Accepted** for local play; a networked host must project per-seat views before sending state |
| D1 | Denial of service | A strategy that keeps returning illegal moves stalls a game | Illegal moves throw immediately; `GameRunner` has no retry loop | Mitigated |
| D2 | Denial of service | A huge or hostile input | Inputs are enums, small integers and 48-card collections; no parsing or unbounded input | Not applicable |
| E1 | Elevation of privilege | Code execution through the engine | No deserialization, `Process`, file access or dynamic code (the trim/AOT analyzers enforce this); the only reflection is `GetType().Name` in a `BiddingPhase` error message | Not applicable |
| E2 | Elevation / fairness | Shuffles are predictable if the `Random` seed is known | Seeding is the caller's choice: `System.Random` is fine for casual play (S2245 note in `GameService`) | **Accepted:** anything with stakes must pass a `Random` backed by a cryptographic RNG |

## Accepted risks

T1, R1, I1 and E2 are accepted because the engine's contract is a *trusted,
in-process rules referee*. Each one becomes a live risk only if Hawsey is hosted
for untrusted remote players. That change must revisit this model first. Any
threat found in a review that is neither mitigated nor accepted here is filed as
an issue with the `kind:threat-mitigation` label and stays open until it is.

## Review

Review this model yearly (the **Next review due** date above) and before every
major release. Update the dates, re-check each row against the code, and record
new threats as rows or as `kind:threat-mitigation` issues.
