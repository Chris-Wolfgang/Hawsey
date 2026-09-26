# Introduction

`Wolfgang.Hawsey.Engine` is the game engine for **Hawsey**, a four-player team trick-taking card game played with a 48-card pinochle deck.

## Overview

The engine holds the rules and nothing else. It does no I/O and has no UI. A caller starts a game with `GameEngine.StartGame`, then moves it forward with `PlaceBid`, `SelectTrump`, `ExchangeHawseyCards` and `PlayCard`. Each call takes the current `GameState` and returns the next one. `GameRunner` plays a complete game with an `IPlayerStrategy` making every decision.

## Key Features

- Pinochle deck: `Card`, `Rank`, `Suit`, `Deck`, with bowers handled by `CardRanking`
- Bidding: `BidAction` (`PassBid`, `NumberBid`, `HawseyBid`), `BiddingPhase`, `BiddingResult`
- Trick play: `Trick`, `PlayedCard`, `FollowSuitValidator`, `TrickResult`
- House rules: `HouseRules` (`MustBeat`, `MustTrump`, `MinimumBid`, `PointsToWin`) and `TrumpMode` (`Suited` or `AceHigh`)
- Scoring: `ScoreKeeper`, `RoundScore`, `GameResult`
- Targets `netstandard2.0` and `net10.0`; trim- and Native AOT-compatible on `net10.0`

## Getting Help

- Check the [Getting Started](getting-started.md) guide
- Review the [API Reference](../api/index.md)
- Visit the [GitHub repository](https://github.com/Chris-Wolfgang/Hawsey)
- Open an issue on [GitHub Issues](https://github.com/Chris-Wolfgang/Hawsey/issues)
