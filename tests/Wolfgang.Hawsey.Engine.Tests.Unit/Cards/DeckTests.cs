using Wolfgang.Hawsey.Engine.Cards;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Cards;

public class DeckTests
{
    [Fact]
    public void CreatePinochleDeck_when_called_returns_48_cards()
    {
        var deck = Deck.CreatePinochleDeck();

        Assert.Equal(Deck.CardCount, deck.Count);
    }



    [Fact]
    public void CreatePinochleDeck_when_called_contains_two_of_each_card()
    {
        var deck = Deck.CreatePinochleDeck();

        var grouped = deck.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

        // 6 ranks * 4 suits = 24 unique cards, each appearing twice
        Assert.Equal(24, grouped.Count);
        Assert.All(grouped.Values, count => Assert.Equal(2, count));
    }



    [Fact]
    public void CreatePinochleDeck_when_called_contains_only_pinochle_ranks()
    {
        var deck = Deck.CreatePinochleDeck();
        var validRanks = new[] { Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };

        Assert.All(deck, card => Assert.Contains(card.Rank, validRanks));
    }



    [Fact]
    public void CreatePinochleDeck_when_called_contains_all_four_suits()
    {
        var deck = Deck.CreatePinochleDeck();
        var suits = deck.Select(c => c.Suit).Distinct().OrderBy(s => s).ToArray();

        Assert.Equal
        (
            new[] { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades },
            suits
        );
    }



    [Fact]
    public void Shuffle_when_called_returns_the_same_count()
    {
        var deck = Deck.CreatePinochleDeck();
        var shuffled = Deck.Shuffle(deck, new Random(42));

        Assert.Equal(deck.Count, shuffled.Count);
    }



    [Fact]
    public void Shuffle_when_called_returns_the_same_cards()
    {
        var deck = Deck.CreatePinochleDeck();
        var shuffled = Deck.Shuffle(deck, new Random(42));

        Assert.Equal
        (
            deck.OrderBy(c => c.Suit).ThenBy(c => c.Rank).ToList(),
            shuffled.OrderBy(c => c.Suit).ThenBy(c => c.Rank).ToList()
        );
    }



    [Fact]
    public void Shuffle_when_seeds_match_produces_the_same_order()
    {
        var deck = Deck.CreatePinochleDeck();
        var shuffled1 = Deck.Shuffle(deck, new Random(42));
        var shuffled2 = Deck.Shuffle(deck, new Random(42));

        Assert.Equal(shuffled1, shuffled2);
    }



    [Fact]
    public void Shuffle_when_seeds_differ_produces_different_orders()
    {
        var deck = Deck.CreatePinochleDeck();
        var shuffled1 = Deck.Shuffle(deck, new Random(42));
        var shuffled2 = Deck.Shuffle(deck, new Random(99));

        Assert.NotEqual(shuffled1, shuffled2);
    }



    [Fact]
    public void Shuffle_when_cards_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => Deck.Shuffle(null!, new Random(42)));
    }



    [Fact]
    public void Shuffle_when_random_null_throws()
    {
        var deck = Deck.CreatePinochleDeck();

        Assert.Throws<ArgumentNullException>(() => Deck.Shuffle(deck, null!));
    }



    [Fact]
    public void Shuffle_when_given_two_cards_produces_both_orders_across_seeds()
    {
        var first = new Card(Rank.Nine, Suit.Hearts);
        var second = new Card(Rank.Ace, Suit.Spades);

        var orders = Enumerable
            .Range(0, 64)
            .Select(seed => Deck.Shuffle([first, second], new Random(seed))[0])
            .Distinct()
            .Count();

        Assert.Equal(2, orders);
    }
}
