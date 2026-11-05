type: feature

`Trick.CurrentWinningPlay` returns the play winning the trick so far (player and card), so a strategy or UI no longer re-derives it; `Trick.GetResult` now gives the `TrickResult` its own copy of the plays.
