type: docs

The engine's XML docs now list every exception its public methods throw (a null `state` on each `GameEngine` transition, the wrong phase for `SelectTrump`, an out-of-range `Trick` size, an undefined `Suit` in `GameSession.SelectTrump`), describe each `GameState` constructor parameter, and say what each `GameSession` advance method returns.
