type: internal

The engine and its unit tests no longer use implicit usings; every file names the `System.*` namespaces it needs, so all target frameworks compile the same source.
