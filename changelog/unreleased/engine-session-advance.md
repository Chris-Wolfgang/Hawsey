type: feature

`GameSession.AdvanceAsync()` plays the AI seats until the game waits for the human or for the next round, and returns what it is waiting for (`WaitingFor`). The Blazor UI drives the game with it instead of its own copy of the loop, and the human's forced last card of a round is played automatically.
