# Hawsey

A C# implementation of **Hawsey**, a four-player team trick-taking card game played with a pinochle deck. Hawsey is popular in Pennsylvania Dutch communities and shares ancestry with pinochle, schafkopf, and similar bidding/trick games.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/Hawsey)

---

## 📖 About the Game

Hawsey is a **4-player partnership trick-taking game** dealt from a 48-card pinochle deck (two each of 9, 10, J, Q, K, A in all four suits). Two teams of two players sit across from each other. Each hand has three phases:

1. **Bidding** — Players bid for the right to name trump, with the highest bidder committing to take a minimum number of points.
2. **Trump declaration** — The winning bidder names the trump suit (and, in some house rules, the trump-card priority mode).
3. **Trick play** — Players follow suit when possible; the trick is won by the highest trump or, if no trump was played, the highest card of the lead suit.

Scoring rewards both card-point capture (taking aces, tens, kings) and the *last trick*. The bidding team must meet their bid or be **bucked** (lose the bid amount).

This implementation supports configurable **house rules** (trump-card priority modes, bidding minimums, partner-call variants) so you can play the variant your grandparents taught you.

---

## 🧩 Project Layout

| Project | Purpose |
|---------|---------|
| `Wolfgang.Hawsey.Engine` | Pure game-logic library (cards, bidding, trick play, scoring, rules). Multi-targets `netstandard2.0` and `net10.0`. |
| `Wolfgang.Hawsey.UI.Shared` | Shared ViewModels and services used by every UI front-end. |
| `Wolfgang.Hawsey.UI.Blazor` | Blazor Server / Blazor WebAssembly front-end. |
| `Wolfgang.Hawsey.UI.Maui` | .NET MAUI front-end (Windows, macOS, iOS, Android). |
| `Wolfgang.Hawsey.UI.MauiHybrid` | MAUI Blazor Hybrid front-end (native shell hosting Blazor views). |
| `Wolfgang.Hawsey.UI.WinForms` | Windows Forms front-end (legacy desktop). |

The engine is pure, deterministic, and UI-agnostic. State transitions are immutable: every method takes a `GameState` and returns a new one, which makes the engine straightforward to unit test, replay, and serialize.

---

## ✨ Engine Features

- **Pinochle deck modeling** — `Card`, `Rank`, `Suit`, `Deck` with shuffle support
- **Bidding phase** — `BidAction`, `BiddingPhase`, `BiddingResult` with validation
- **Trick play** — `Trick`, `PlayedCard`, `FollowSuitValidator`, `TrickResult`
- **Configurable house rules** — `HouseRules`, `TrumpMode` (Ace-high, Jack-high variants)
- **Player & team modeling** — `PlayerHand`, `PlayerPosition`, `Team`, `DealerRotation`
- **Scoring** — `ScoreKeeper`, `RoundScore`, `GameResult`
- **Strategy hooks** — `IPlayerStrategy`, `GameRunner` for plugging in human or AI players

---

## 🚀 Quick Start

> **Status:** v0.1.0 — engine is feature-complete; UI front-ends are in active development. The engine is not yet published to NuGet.

### Build from source

```bash
git clone https://github.com/Chris-Wolfgang/Hawsey.git
cd Hawsey
dotnet restore
dotnet build --configuration Release
dotnet test
```

### Use the engine

```csharp
using Wolfgang.Hawsey.Engine;

var engine = new GameEngine();
var rules = HouseRules.Default;
var random = new Random();

// Start a game with the dealer in the South seat
var state = engine.StartGame(rules, PlayerPosition.South, random);

// state.Phase == GamePhase.Bidding
// state.Hands contains 12 cards per player
// Each player position rotates around the table
```

The `GameRunner` plus an `IPlayerStrategy` per seat is the easiest way to drive a full game programmatically (useful for AI-vs-AI evaluation or replay).

---

## 🎯 Target Frameworks

| Project | Frameworks |
|---------|-----------|
| `Wolfgang.Hawsey.Engine` | `netstandard2.0`, `net10.0` |
| UI projects | `net10.0` (with MAUI / Blazor / WinForms platform targets where applicable) |

`netstandard2.0` on the engine keeps the door open for hosting it from older runtimes (e.g., classic .NET Framework game shells), without giving up modern target features.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/Hawsey](https://github.com/Chris-Wolfgang/Hawsey)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)
- **Code of Conduct:** [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
- **Security Policy:** [SECURITY.md](SECURITY.md)

---

## 🤝 Contributing

Contributions are welcome — especially:

- **House-rule variants** — the codebase is set up to be data-driven; new variants should land in `HouseRules` and `TrumpMode`.
- **AI strategies** — implement `IPlayerStrategy` and contribute it as a sample or test fixture.
- **UI work** — the front-ends are at varying levels of completeness; pick the one you prefer.

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for code style and PR guidelines.

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.
