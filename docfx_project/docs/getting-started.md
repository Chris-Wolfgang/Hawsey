# Getting Started

This guide shows how to start a game with `Wolfgang.Hawsey.Engine`.

## Prerequisites

- A project targeting a framework that supports `netstandard2.0` (for example .NET Framework 4.6.2 or later, or .NET 8 or later)

## Installation

```bash
dotnet add package Wolfgang.Hawsey.Engine
```

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

## Play Against the AI

`GameSession` is a live game with the human in the South seat and the engine's AI, `SimpleAiStrategy`, in the other three. `AdvanceAsync()` plays the AI moves and returns what the game needs from the human next:

```csharp
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Game;

var session = new GameSession();
session.StartNewGame();

while (true)
{
    switch (await session.AdvanceAsync())
    {
        case WaitingFor.HumanBid:
            session.PlaceHumanBid(BidAction.PassBid.Instance);  // or new BidAction.NumberBid(7), BidAction.HawseyBid.Instance
            break;

        case WaitingFor.HumanTrump:
            session.SelectTrump(Suit.Hearts);                    // null plays ace high (no trump)
            break;

        case WaitingFor.HumanHawseyExchange:
            var hand = session.CurrentState!.Hands[GameSession.HumanPosition];
            session.PerformHumanHawseyExchange([hand[0], hand[1]]);
            break;

        case WaitingFor.HumanCard:
            session.PlayHumanCard(session.CurrentState!.GetLegalPlays()[0]);
            break;

        case WaitingFor.NextRound:
            session.StartNextRound();                            // CurrentState.RoundScore has the round just scored
            break;

        case WaitingFor.GameOver:
            Console.WriteLine($"{session.CurrentState!.Winner} wins");
            return;
    }
}
```

The session pauses between AI moves so a person can follow them, and raises `StateChanged` for a UI to redraw. This is how the [Blazor app](https://chris-wolfgang.github.io/Hawsey/play/) plays.

`GameSession` and `SimpleAiStrategy` are sealed. To play with your own AI, implement `IPlayerStrategy` and use `GameRunner`, as above.

## Next Steps

- Explore the [API Reference](../api/index.md) for detailed documentation
- Read the [Introduction](introduction.md) for an overview of the engine
- See `examples/Wolfgang.Hawsey.Engine.AotSmoke` in the [GitHub repository](https://github.com/Chris-Wolfgang/Hawsey) for a program that plays complete games

## Additional Resources

- [GitHub Repository](https://github.com/Chris-Wolfgang/Hawsey)
- [Contributing Guidelines](https://github.com/Chris-Wolfgang/Hawsey/blob/main/CONTRIBUTING.md)
- [Report an Issue](https://github.com/Chris-Wolfgang/Hawsey/issues)
