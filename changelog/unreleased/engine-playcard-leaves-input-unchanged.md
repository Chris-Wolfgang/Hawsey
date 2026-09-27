type: fix

`GameEngine.PlayCard` no longer adds the card to the trick held by the state passed in. The input state is left unchanged, as `GameState` documents, so replaying from an earlier state works.
