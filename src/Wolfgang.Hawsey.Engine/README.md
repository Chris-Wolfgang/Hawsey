# Wolfgang.Hawsey.Engine

The game engine for **Hawsey**, a 4-player partnership trick-taking card game played with a pinochle deck. It holds the rules, bidding, trick play, scoring and an AI player, and it does no I/O. A UI renders the `GameState` the engine hands back and sends it the player's moves.

Play it in the browser: [chris-wolfgang.github.io/Hawsey/play](https://chris-wolfgang.github.io/Hawsey/play/) (a Blazor WebAssembly app built on this package).

```bash
dotnet add package Wolfgang.Hawsey.Engine
```

Targets `netstandard2.0` and `net10.0`. On `net10.0` it is trimmable and Native AOT-compatible.

## Play one human against three AI seats: `GameSession`

`GameSession` is a live game with the human in the South seat and the engine's AI in the other three. `AdvanceAsync()` plays the AI moves and returns what the game needs from the human next.

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

The session pauses between AI moves so a person can follow them. It raises `StateChanged`, `TrickCompleted`, `RoundCompleted` and `GameOver` for a UI to redraw. A move made out of turn, such as a double-tap, is ignored and returns `false`.

## Drive the rules directly: `GameEngine`

`GameEngine` is stateless. Each method takes the current `GameState` and returns the next one, and never changes the state passed in, so any earlier state can be replayed.

```csharp
using Wolfgang.Hawsey.Engine.Game;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;

var engine = new GameEngine();
var state = engine.StartGame(HouseRules.Default, PlayerPosition.South, new Random());

// state.Phase == GamePhase.Bidding; state.Hands holds 12 cards for each seat.
// Then PlaceBid, SelectTrump, ExchangeHawseyCards and PlayCard move the game on.
```

## Play whole games: `IPlayerStrategy` and `GameRunner`

Implement `IPlayerStrategy` (bid, name trump, make the Hawsey exchange, play a card) and `GameRunner.RunGame` plays a complete game with it, for AI-against-AI simulations or tests. `SimpleAiStrategy` is the engine's own AI.

```csharp
using Wolfgang.Hawsey.Engine.Strategy;

var final = new GameRunner().RunGame(new SimpleAiStrategy(), HouseRules.Default, PlayerPosition.South, new Random());

// final.Phase == GamePhase.GameOver
```

## The game

- 48-card pinochle deck (two each of 9, 10, J, Q, K, A in four suits), 12 cards each, North/South against East/West.
- **Bidding:** pass, or bid the number of tricks your team will take (minimum 6 by default).
- **Trump:** the high bidder names a suit, or plays ace high. With a suit, the jack of trump (right bower) is highest and the other jack of the same colour (left bower) is second and counts as trump.
- **Scoring:** one point per trick. A bidding team that falls short loses its bid. First to 62 wins.
- **Hawsey:** the bidder plays alone for all 12 tricks, swapping two cards with their partner, for 24 points (or −24).
- `HouseRules` sets the variants: `MustBeat`, `MustTrump`, `MinimumBid` and `PointsToWin`.

## Links

- Source, issues and full rules: [github.com/Chris-Wolfgang/Hawsey](https://github.com/Chris-Wolfgang/Hawsey)
- API documentation: [chris-wolfgang.github.io/Hawsey](https://chris-wolfgang.github.io/Hawsey/)
- Changelog: [CHANGELOG.md](https://github.com/Chris-Wolfgang/Hawsey/blob/main/CHANGELOG.md)
- License: MIT
