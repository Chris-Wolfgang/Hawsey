type: breaking

The engine rejects values that could never work where they come in: a number bid above 12 (`BiddingPhase.MaximumBid`, which replaces `GameSession.MaximumBid`), a `HouseRules.MinimumBid` outside 1-12 or a `BiddingPhase` minimum outside 1-12, a `HouseRules.PointsToWin` below 1, and an undefined `Suit` passed to `GameEngine.SelectTrump`.
