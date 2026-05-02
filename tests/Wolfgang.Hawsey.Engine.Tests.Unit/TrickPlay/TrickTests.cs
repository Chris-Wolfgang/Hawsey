using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.TrickPlay;

public class TrickTests
{
    [Fact]
    public void Play_when_first_card_sets_led_suit()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));

        Assert.Equal(Suit.Clubs, trick.LedSuit);
    }



    [Fact]
    public void Play_when_left_bower_led_sets_led_suit_to_trump()
    {
        // Clubs trump, Jack of Spades is left bower (effective suit = Clubs)
        var trick = new Trick(Suit.Clubs);

        trick.Play(PlayerPosition.North, new Card(Rank.Jack, Suit.Spades));

        Assert.Equal(Suit.Clubs, trick.LedSuit);
    }



    [Fact]
    public void IsComplete_when_4_cards_played_returns_true()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Queen, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Ten, Suit.Clubs));

        Assert.True(trick.IsComplete);
    }



    [Fact]
    public void IsComplete_when_3_of_4_cards_played_returns_false()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Queen, Suit.Clubs));

        Assert.False(trick.IsComplete);
    }



    [Fact]
    public void IsComplete_when_hawsey_3_player_trick()
    {
        var trick = new Trick(Suit.Hearts, expectedPlays: 3);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Queen, Suit.Clubs));

        Assert.True(trick.IsComplete);
    }



    [Fact]
    public void GetResult_when_highest_card_wins()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ten, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Queen, Suit.Clubs));

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.East, result.Winner);
    }



    [Fact]
    public void GetResult_when_trump_beats_non_trump()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Nine, Suit.Hearts)); // trump
        trick.Play(PlayerPosition.South, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Queen, Suit.Clubs));

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.East, result.Winner);
    }



    [Fact]
    public void GetResult_when_right_bower_beats_left_bower()
    {
        var trick = new Trick(Suit.Clubs);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Jack, Suit.Spades));  // left bower
        trick.Play(PlayerPosition.South, new Card(Rank.Jack, Suit.Clubs)); // right bower
        trick.Play(PlayerPosition.West, new Card(Rank.King, Suit.Clubs));

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.South, result.Winner);
    }



    [Fact]
    public void GetResult_when_duplicate_cards_first_played_wins()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Ace, Suit.Clubs)); // duplicate
        trick.Play(PlayerPosition.West, new Card(Rank.Queen, Suit.Clubs));

        var result = trick.GetResult();

        // First Ace wins
        Assert.Equal(PlayerPosition.North, result.Winner);
    }



    [Fact]
    public void GetResult_when_off_suit_loses_to_led_suit()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Nine, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Ace, Suit.Diamonds)); // off-suit
        trick.Play(PlayerPosition.South, new Card(Rank.Ten, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Ace, Suit.Spades)); // off-suit

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.South, result.Winner);
    }



    [Fact]
    public void GetResult_when_ace_high_no_trump()
    {
        var trick = new Trick(trumpSuit: null); // Ace high

        trick.Play(PlayerPosition.North, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Queen, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Jack, Suit.Clubs));

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.East, result.Winner);
    }



    [Fact]
    public void GetResult_when_ace_high_off_suit_cannot_win()
    {
        var trick = new Trick(trumpSuit: null); // Ace high

        trick.Play(PlayerPosition.North, new Card(Rank.Nine, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Ace, Suit.Hearts)); // off-suit
        trick.Play(PlayerPosition.South, new Card(Rank.Ten, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Ace, Suit.Spades)); // off-suit

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.South, result.Winner);
    }



    [Fact]
    public void GetResult_returns_all_played_cards()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Queen, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Ten, Suit.Clubs));

        var result = trick.GetResult();

        Assert.Equal(4, result.Cards.Count);
    }



    [Fact]
    public void Play_when_trick_complete_throws()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.King, Suit.Clubs));
        trick.Play(PlayerPosition.South, new Card(Rank.Queen, Suit.Clubs));
        trick.Play(PlayerPosition.West, new Card(Rank.Ten, Suit.Clubs));

        Assert.Throws<InvalidOperationException>
        (
            () => trick.Play(PlayerPosition.North, new Card(Rank.Nine, Suit.Clubs))
        );
    }



    [Fact]
    public void GetResult_when_not_complete_throws()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Clubs));

        Assert.Throws<InvalidOperationException>(() => trick.GetResult());
    }



    [Fact]
    public void Constructor_when_expected_plays_out_of_range_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Trick(Suit.Hearts, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Trick(Suit.Hearts, 5));
    }



    [Fact]
    public void GetCurrentWinner_when_no_plays_returns_null()
    {
        var trick = new Trick(Suit.Hearts);

        Assert.Null(trick.GetCurrentWinner());
    }



    [Fact]
    public void GetCurrentWinner_returns_winning_card_mid_trick()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ten, Suit.Clubs));
        trick.Play(PlayerPosition.East, new Card(Rank.Ace, Suit.Clubs));

        Assert.Equal(new Card(Rank.Ace, Suit.Clubs), trick.GetCurrentWinner());
    }



    [Fact]
    public void LedSuit_when_no_plays_returns_null()
    {
        var trick = new Trick(Suit.Hearts);

        Assert.Null(trick.LedSuit);
    }



    [Fact]
    public void GetResult_when_left_bower_beats_ace_of_trump()
    {
        var trick = new Trick(Suit.Hearts);

        trick.Play(PlayerPosition.North, new Card(Rank.Ace, Suit.Hearts));
        trick.Play(PlayerPosition.East, new Card(Rank.Jack, Suit.Diamonds)); // left bower
        trick.Play(PlayerPosition.South, new Card(Rank.King, Suit.Hearts));
        trick.Play(PlayerPosition.West, new Card(Rank.Queen, Suit.Hearts));

        var result = trick.GetResult();

        Assert.Equal(PlayerPosition.East, result.Winner);
    }
}
