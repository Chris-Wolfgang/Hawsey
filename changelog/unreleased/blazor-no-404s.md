type: fix

The Blazor app no longer requests two files that don't exist: it has a favicon, and it no longer links a scoped-CSS bundle the app never generates.
