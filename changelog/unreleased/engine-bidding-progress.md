type: feature

`GameState.HighBid` and `GameState.HighBidder` report the bidding so far (the bid to beat, and who holds it), and `BiddingPhase` exposes `HighestBid` and `HighestBidder`. A UI can show "current high bid: 8 by West", and a strategy can make a legal bid.
