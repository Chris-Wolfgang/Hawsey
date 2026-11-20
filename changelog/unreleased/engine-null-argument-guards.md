type: fix

The `GameState` and `TrickResult` constructors and `GameSession.PlaceHumanBid` now throw `ArgumentNullException` for a `null` argument instead of failing later or only on the human's turn.
