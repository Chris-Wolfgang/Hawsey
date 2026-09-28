type: feature

The engine tells a UI what to show at the end of a trick and a round: `GameState.TableCards` and `GameState.TableWinningCard` (the trick on the table, including the one just won until the next lead) and `GameState.RoundScore` (the round just scored), with `RoundScore.DeltaFor(team)` and `RoundScore.TricksFor(team)`. The MAUI and Blazor UIs use them instead of their own copies of the rules.
