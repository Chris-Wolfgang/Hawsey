type: breaking

`GameSession` and `SimpleAiStrategy` are now sealed, so code that derives from either must wrap it instead; to customise the AI, implement `IPlayerStrategy`.
