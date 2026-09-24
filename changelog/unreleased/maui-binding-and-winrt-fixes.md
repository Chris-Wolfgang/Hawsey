type: fix

The MAUI app's card tap binding now resolves `PlayCardCommand` on the game view model at compile time instead of against the card, and the Windows build generates AOT-safe WinRT vtables for its commands.
