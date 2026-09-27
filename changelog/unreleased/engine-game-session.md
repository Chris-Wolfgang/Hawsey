type: feature

The engine now includes `GameSession`, a live game between one human (South) and three AI seats, with paced AI moves, protection against stale or out-of-turn moves, and events for state changes, tricks, rounds and game over; and `SimpleAiStrategy`, the AI player it uses. Every Hawsey UI uses these instead of its own copy.
