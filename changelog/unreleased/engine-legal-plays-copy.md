type: fix

`FollowSuitValidator.GetLegalPlays` (and so `GameState.GetLegalPlays`) always returns a new list. When every card was legal it used to return the hand itself, the game state's own list, which a caller could cast back to `List<Card>` and change.
