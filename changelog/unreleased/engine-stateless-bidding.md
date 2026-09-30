type: feature

`GameEngine.PlaceBid(state, player, action)` reads the bidding so far from the `GameState` (`HighBid`, `HighBidder`, `NextToAct`), so bidding needs no separate object to track, and replaying from any earlier state works.
