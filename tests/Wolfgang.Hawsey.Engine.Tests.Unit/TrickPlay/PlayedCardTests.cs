using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.TrickPlay;

public class PlayedCardTests
{
    [Fact]
    public void Constructor_when_called_assigns_card()
    {
        var card = new Card(Rank.Ace, Suit.Spades);

        var played = new PlayedCard(card, PlayerPosition.North, playOrder: 0);

        Assert.Equal(card, played.Card);
    }



    [Fact]
    public void Constructor_when_called_assigns_player()
    {
        var played = new PlayedCard(new Card(Rank.King, Suit.Hearts), PlayerPosition.East, playOrder: 1);

        Assert.Equal(PlayerPosition.East, played.Player);
    }



    [Fact]
    public void Constructor_when_called_assigns_play_order()
    {
        var played = new PlayedCard(new Card(Rank.Queen, Suit.Diamonds), PlayerPosition.South, playOrder: 2);

        Assert.Equal(2, played.PlayOrder);
    }



    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void Constructor_when_player_is_any_position_round_trips(PlayerPosition player)
    {
        var played = new PlayedCard(new Card(Rank.Ten, Suit.Clubs), player, playOrder: 3);

        Assert.Equal(player, played.Player);
    }



    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Constructor_when_play_order_is_zero_through_three_round_trips(int order)
    {
        var played = new PlayedCard(new Card(Rank.Jack, Suit.Hearts), PlayerPosition.North, order);

        Assert.Equal(order, played.PlayOrder);
    }
}
