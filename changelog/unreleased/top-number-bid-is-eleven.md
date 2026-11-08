type: breaking

`GameState.MaximumBid` is 11, not 12, and `GameEngine.PlaceBid` refuses a number bid of 12: bidding all twelve tricks is the Hawsey bid. Code compiled against 0.1.0 keeps the old value of the constant until it is rebuilt.
