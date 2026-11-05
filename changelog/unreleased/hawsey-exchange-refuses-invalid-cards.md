type: fix

`GameSession.PerformHawseyExchange` now returns `false`, and changes nothing, when either array is not two cards the right player holds, instead of throwing `InvalidOperationException`; the Hawsey score and the tricks in a round are public as `GameState.HawseyBidAmount` and `GameState.TricksPerRound`.
