---
_layout: landing
---

# Hawsey Documentation

Documentation for `Wolfgang.Hawsey.Engine`, the game engine for Hawsey: a four-player team trick-taking card game played with a pinochle deck.

## Quick Links

- [Getting Started](docs/getting-started.md) - Start a game with the engine
- [API Reference](api/index.md) - API documentation generated from the XML comments
- [GitHub Repository](https://github.com/Chris-Wolfgang/Hawsey) - Source code

## About Hawsey

The engine is a UI-agnostic .NET library. It models the pinochle deck, bidding, trump selection, the Hawsey exchange, trick play and scoring. Each `GameEngine` method takes the current `GameState` and returns the next one. It targets `netstandard2.0` and `net10.0`.

## Installation

```bash
dotnet add package Wolfgang.Hawsey.Engine
```

The package has not been published to NuGet yet. Until the first release, build it from source.

## Documentation Sections

### 📖 [Documentation](docs/getting-started.md)
Guides for using the engine.

### 📚 [API Reference](api/index.md)
API documentation generated from the source code's XML comments.

## Additional Resources

- [Contributing Guidelines](https://github.com/Chris-Wolfgang/Hawsey/blob/main/CONTRIBUTING.md)
- [Code of Conduct](https://github.com/Chris-Wolfgang/Hawsey/blob/main/CODE_OF_CONDUCT.md)
- [License](https://github.com/Chris-Wolfgang/Hawsey/blob/main/LICENSE)

---

*Documentation built with [DocFX](https://dotnet.github.io/docfx/)*
