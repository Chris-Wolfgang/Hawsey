# Getting Started

This guide shows how to start a game with `Wolfgang.Hawsey.Engine`.

## Prerequisites

- A project targeting a framework that supports `netstandard2.0` (for example .NET Framework 4.6.2 or later, or .NET 8 or later)

## Installation

The package has not been published to NuGet yet. Once it is:

```bash
dotnet add package Wolfgang.Hawsey.Engine
```

Until then, clone the [repository](https://github.com/Chris-Wolfgang/Hawsey) and reference `src/Wolfgang.Hawsey.Engine/Wolfgang.Hawsey.Engine.csproj` from your project.

## Quick Start

```csharp
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;

var engine = new GameEngine();
var state = engine.StartGame(HouseRules.Default, PlayerPosition.South, new Random());

// state.Phase == GamePhase.Bidding
// state.NextToAct is the first player to bid
```

Move the game forward with `PlaceBid`, `SelectTrump`, `ExchangeHawseyCards` (Hawsey rounds only) and `PlayCard`. Each takes the current `GameState` and returns the next one. `state.GetLegalPlays()` returns the cards the player to act may play.

To play a complete game, implement `IPlayerStrategy` and pass it to `GameRunner.RunGame`:

```csharp
using Wolfgang.Hawsey.Engine.Strategy;

var final = new GameRunner().RunGame(myStrategy, HouseRules.Default, PlayerPosition.South, new Random());

// final.Phase == GamePhase.GameOver
```

## Next Steps

- Explore the [API Reference](../api/index.md) for detailed documentation
- Read the [Introduction](introduction.md) for an overview of the engine
- See `examples/Wolfgang.Hawsey.Engine.AotSmoke` in the [GitHub repository](https://github.com/Chris-Wolfgang/Hawsey) for a program that plays complete games

## Additional Resources

- [GitHub Repository](https://github.com/Chris-Wolfgang/Hawsey)
- [Contributing Guidelines](https://github.com/Chris-Wolfgang/Hawsey/blob/main/CONTRIBUTING.md)
- [Report an Issue](https://github.com/Chris-Wolfgang/Hawsey/issues)
