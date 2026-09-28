type: breaking

Removed public types nothing used: `ScoreKeeper` and `GameResult` (a second scoring path that disagreed with the engine on who wins when both teams reach the target; the engine scores with `RoundScore` and names the winner in `GameState.Winner`), `DealerRotation`, `PlayerHand`, and `GamePhase.Dealing` (never set). The package has not been released, so no published version had them.
