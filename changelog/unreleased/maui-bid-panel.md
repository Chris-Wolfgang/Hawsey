type: fix

The MAUI bid panel shows who holds the current high bid ("Current high bid: 8 by East (opponents)"), offers only the bids you can legally make (the minimum legal bid up to 11), and, when you are the stuck dealer, only the minimum or Hawsey. A bid the engine would reject is ignored instead of silently throwing. The engine adds `GameState.MinimumLegalBid` and `GameState.IsNextBidderStuck`, and `GameSession.PlaceHumanBid` returns false for an illegal bid.
