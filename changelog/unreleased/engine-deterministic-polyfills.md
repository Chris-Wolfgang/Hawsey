type: internal

The netstandard2.0 build of the engine is now reproducible: PolySharp generates only the IsExternalInit polyfill the engine uses, instead of every polyfill in a run-dependent order.
