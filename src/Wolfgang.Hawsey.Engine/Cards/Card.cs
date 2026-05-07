using System.Runtime.InteropServices;

namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Represents a single playing card in a pinochle deck.
/// </summary>
/// <param name="Rank">The rank of the card.</param>
/// <param name="Suit">The suit of the card.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Card(Rank Rank, Suit Suit)
{
    /// <inheritdoc />
    public override string ToString() => $"{Rank} of {Suit}";
}
