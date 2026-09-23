type: fix

The MAUI app no longer corrupts or crashes a game when a card is double-tapped, a move is tapped during an AI turn, or New Game is pressed while the AI is thinking: every game-state change is serialized and stale moves are ignored.
