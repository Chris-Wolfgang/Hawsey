namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Manages a single trick, collecting played cards and determining the winner.
/// </summary>
public sealed class Trick
{
    private readonly int _expectedPlays;
    private readonly Suit? _trumpSuit;
    private readonly List<PlayedCard> _plays;
    private Suit? _ledSuit;



    /// <summary>
    /// Initializes a new instance of the <see cref="Trick"/> class.
    /// </summary>
    /// <param name="trumpSuit">The trump suit, or <c>null</c> for Ace high mode.</param>
    /// <param name="expectedPlays">
    /// The number of cards expected in this trick (4 normally, 3 for Hawsey).
    /// </param>
    public Trick(Suit? trumpSuit, int expectedPlays = 4)
    {
        if (expectedPlays < 2 || expectedPlays > 4)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(expectedPlays),
                expectedPlays,
                "Expected plays must be between 2 and 4."
            );
        }

        _trumpSuit = trumpSuit;
        _expectedPlays = expectedPlays;
        _plays = new List<PlayedCard>(expectedPlays);
    }



    /// <summary>
    /// Gets the effective suit of the card that was led, or <c>null</c> if no cards have been played.
    /// </summary>
    public Suit? LedSuit => _ledSuit;



    /// <summary>
    /// Gets whether the trick is complete (all expected cards have been played).
    /// </summary>
    public bool IsComplete => _plays.Count >= _expectedPlays;



    /// <summary>
    /// Gets the cards that have been played so far.
    /// </summary>
    public IReadOnlyList<PlayedCard> Plays => _plays;



    /// <summary>
    /// Plays a card into this trick.
    /// </summary>
    /// <param name="player">The player playing the card.</param>
    /// <param name="card">The card being played.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trick is already complete.</exception>
    public void Play(PlayerPosition player, Card card)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("This trick is already complete.");
        }

        var playOrder = _plays.Count;
        _plays.Add(new PlayedCard(card, player, playOrder));

        if (playOrder == 0)
        {
            _ledSuit = CardRanking.GetEffectiveSuit(card, _trumpSuit);
        }
    }



    /// <summary>
    /// Determines the winner of the trick. In case of duplicate cards
    /// (identical rank and suit), the first played wins.
    /// </summary>
    /// <returns>The result of the completed trick.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the trick is not yet complete.</exception>
    public TrickResult GetResult()
    {
        if (!IsComplete)
        {
            throw new InvalidOperationException("The trick is not yet complete.");
        }

        var comparer = new CardComparer(_trumpSuit, _ledSuit!.Value);
        var winningPlay = _plays[0];

        for (var i = 1; i < _plays.Count; i++)
        {
            var comparison = comparer.Compare(_plays[i].Card, winningPlay.Card);

            // Strictly greater — ties go to the first played (current winner)
            if (comparison > 0)
            {
                winningPlay = _plays[i];
            }
        }

        return new TrickResult(winningPlay.Player, _plays);
    }



    /// <summary>
    /// Gets the card currently winning the trick, or <c>null</c> if no cards have been played.
    /// </summary>
    /// <returns>The currently winning card, or <c>null</c>.</returns>
    public Card? GetCurrentWinner()
    {
        if (_plays.Count == 0)
        {
            return null;
        }

        var comparer = new CardComparer(_trumpSuit, _ledSuit!.Value);
        var winningPlay = _plays[0];

        for (var i = 1; i < _plays.Count; i++)
        {
            if (comparer.Compare(_plays[i].Card, winningPlay.Card) > 0)
            {
                winningPlay = _plays[i];
            }
        }

        return winningPlay.Card;
    }
}
