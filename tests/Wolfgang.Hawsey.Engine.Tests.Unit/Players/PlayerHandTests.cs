using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Players;

public class PlayerHandTests
{
    [Fact]
    public void Constructor_initializes_with_given_cards()
    {
        var cards = new[]
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.King, Suit.Hearts)
        };

        var hand = new PlayerHand(cards);

        Assert.Equal(2, hand.Cards.Count);
        Assert.Contains(new Card(Rank.Ace, Suit.Spades), hand.Cards);
        Assert.Contains(new Card(Rank.King, Suit.Hearts), hand.Cards);
    }



    [Fact]
    public void Constructor_when_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PlayerHand(null!));
    }



    [Fact]
    public void Remove_removes_one_instance_of_card()
    {
        var hand = new PlayerHand(new[]
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.King, Suit.Hearts)
        });

        hand.Remove(new Card(Rank.Ace, Suit.Spades));

        Assert.Equal(2, hand.Cards.Count);
        Assert.Contains(new Card(Rank.Ace, Suit.Spades), hand.Cards);
    }



    [Fact]
    public void Remove_when_card_not_in_hand_throws()
    {
        var hand = new PlayerHand(new[]
        {
            new Card(Rank.King, Suit.Hearts)
        });

        Assert.Throws<InvalidOperationException>
        (
            () => hand.Remove(new Card(Rank.Ace, Suit.Spades))
        );
    }



    [Fact]
    public void Add_adds_cards_to_hand()
    {
        var hand = new PlayerHand(new[]
        {
            new Card(Rank.King, Suit.Hearts)
        });

        hand.Add(new[]
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.Queen, Suit.Diamonds)
        });

        Assert.Equal(3, hand.Cards.Count);
    }



    [Fact]
    public void Add_when_null_throws()
    {
        var hand = new PlayerHand(Array.Empty<Card>());

        Assert.Throws<ArgumentNullException>(() => hand.Add(null!));
    }



    [Fact]
    public void Contains_when_card_exists_returns_true()
    {
        var hand = new PlayerHand(new[] { new Card(Rank.Ace, Suit.Spades) });

        Assert.True(hand.Contains(new Card(Rank.Ace, Suit.Spades)));
    }



    [Fact]
    public void Contains_when_card_missing_returns_false()
    {
        var hand = new PlayerHand(new[] { new Card(Rank.Ace, Suit.Spades) });

        Assert.False(hand.Contains(new Card(Rank.King, Suit.Hearts)));
    }
}
