type: fix

The engine rejects values that could never work where they come in: a number bid above `GameState.MaximumBid` (12), a `HouseRules.MinimumBid` outside 1-12, a `HouseRules.PointsToWin` below 1, and an undefined `Suit` passed to `GameEngine.SelectTrump`.
