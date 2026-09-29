type: feature

`GameSession.AdvanceAsync()` plays the AI seats until the game waits for the human or for the next round, and returns what it is waiting for (`WaitingFor`). The MAUI and Blazor UIs both drive the game with it instead of their own copies of the loop, and the human's forced last card of a round is played automatically in both.
