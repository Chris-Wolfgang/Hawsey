type: fix

The `netstandard2.0` package no longer declares dependencies on `System.Memory` and `Microsoft.Bcl.HashCode`, which the engine never used.
