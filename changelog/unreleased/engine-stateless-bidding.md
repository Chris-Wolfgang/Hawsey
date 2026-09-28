type: breaking

`GameEngine.PlaceBid(state, player, action)` reads the bidding so far from the `GameState` (`HighBid`, `HighBidder`, `NextToAct`) instead of advancing a separate, caller-owned `BiddingPhase`, which is removed. `BiddingPhase.MaximumBid` becomes `GameState.MaximumBid`.
