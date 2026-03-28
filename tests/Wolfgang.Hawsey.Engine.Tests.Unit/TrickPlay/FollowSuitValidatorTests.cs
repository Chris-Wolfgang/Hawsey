using Wolfgang.Hawsey.Engine;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.TrickPlay;

public class FollowSuitValidatorTests
{
    private static readonly HouseRules DefaultRules = HouseRules.Default;



    [Fact]
    public void GetLegalPlays_when_leading_returns_all_cards()
    {
        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.King, Suit.Hearts),
            new Card(Rank.Nine, Suit.Clubs)
        };

        var result = FollowSuitValidator.GetLegalPlays(hand, ledSuit: null, Suit.Hearts, DefaultRules, currentWinningCard: null);

        Assert.Equal(3, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_can_follow_suit_must_follow()
    {
        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.King, Suit.Spades),
            new Card(Rank.Nine, Suit.Clubs)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, DefaultRules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(Suit.Spades, c.Suit));
    }



    [Fact]
    public void GetLegalPlays_when_void_in_led_suit_can_play_any()
    {
        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Hearts),
            new Card(Rank.King, Suit.Clubs),
            new Card(Rank.Nine, Suit.Diamonds)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, DefaultRules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Equal(3, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_left_bower_counts_as_trump_for_follow_suit()
    {
        // Clubs is trump, Jack of Spades is left bower (effective suit = Clubs)
        var hand = new[]
        {
            new Card(Rank.Jack, Suit.Spades),  // left bower — effectively Clubs
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.King, Suit.Hearts)
        };

        // Led suit is Clubs (trump)
        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Clubs, Suit.Clubs, DefaultRules, new Card(Rank.Ten, Suit.Clubs)
        );

        // Only the left bower can follow Clubs
        Assert.Single(result);
        Assert.Equal(new Card(Rank.Jack, Suit.Spades), result[0]);
    }



    [Fact]
    public void GetLegalPlays_when_left_bower_not_forced_to_follow_printed_suit()
    {
        // Clubs is trump, Jack of Spades is left bower (effective suit = Clubs, NOT Spades)
        var hand = new[]
        {
            new Card(Rank.Jack, Suit.Spades),  // left bower — effectively Clubs
            new Card(Rank.King, Suit.Hearts)
        };

        // Led suit is Spades — left bower does NOT count as Spades
        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Clubs, DefaultRules, new Card(Rank.Ten, Suit.Spades)
        );

        // Void in Spades (left bower is Clubs), can play anything
        Assert.Equal(2, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_must_trump_and_void_must_play_trump()
    {
        var rules = new HouseRules { MustTrump = true };

        var hand = new[]
        {
            new Card(Rank.Nine, Suit.Hearts),   // trump
            new Card(Rank.Ace, Suit.Diamonds),
            new Card(Rank.King, Suit.Clubs)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, rules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Single(result);
        Assert.Equal(new Card(Rank.Nine, Suit.Hearts), result[0]);
    }



    [Fact]
    public void GetLegalPlays_when_must_trump_but_no_trump_can_play_any()
    {
        var rules = new HouseRules { MustTrump = true };

        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Diamonds),
            new Card(Rank.King, Suit.Clubs)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, rules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Equal(2, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_must_beat_and_can_beat_must_play_higher()
    {
        var rules = new HouseRules { MustBeat = true };

        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Spades),    // beats Ten
            new Card(Rank.Nine, Suit.Spades),   // does not beat Ten
            new Card(Rank.King, Suit.Hearts)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, rules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Single(result);
        Assert.Equal(new Card(Rank.Ace, Suit.Spades), result[0]);
    }



    [Fact]
    public void GetLegalPlays_when_must_beat_but_cannot_beat_returns_all_follow_suit()
    {
        var rules = new HouseRules { MustBeat = true };

        var hand = new[]
        {
            new Card(Rank.Nine, Suit.Spades),
            new Card(Rank.Ten, Suit.Spades),
            new Card(Rank.King, Suit.Hearts)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, rules, new Card(Rank.Ace, Suit.Spades)
        );

        // Can't beat Ace, so all Spades are legal
        Assert.Equal(2, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_must_trump_and_must_beat_combined()
    {
        var rules = new HouseRules { MustTrump = true, MustBeat = true };

        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Hearts),    // high trump — beats current winner
            new Card(Rank.Nine, Suit.Hearts),   // low trump — doesn't beat current winner
            new Card(Rank.King, Suit.Diamonds)
        };

        // Led Spades, current winner is a trump (King of Hearts)
        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, Suit.Hearts, rules, new Card(Rank.King, Suit.Hearts)
        );

        // Must trump (void in Spades) AND must beat — only Ace of Hearts qualifies
        Assert.Single(result);
        Assert.Equal(new Card(Rank.Ace, Suit.Hearts), result[0]);
    }



    [Fact]
    public void GetLegalPlays_when_ace_high_no_trump_behavior()
    {
        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Hearts),
            new Card(Rank.King, Suit.Clubs)
        };

        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, trumpSuit: null, DefaultRules, new Card(Rank.Ten, Suit.Spades)
        );

        // Void in Spades, no trump (ace high), can play anything
        Assert.Equal(2, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_must_trump_in_ace_high_mode_plays_any()
    {
        var rules = new HouseRules { MustTrump = true };

        var hand = new[]
        {
            new Card(Rank.Ace, Suit.Hearts),
            new Card(Rank.King, Suit.Clubs)
        };

        // Ace high mode (no trump), must-trump has no effect
        var result = FollowSuitValidator.GetLegalPlays
        (
            hand, Suit.Spades, trumpSuit: null, rules, new Card(Rank.Ten, Suit.Spades)
        );

        Assert.Equal(2, result.Count);
    }



    [Fact]
    public void GetLegalPlays_when_hand_empty_returns_empty()
    {
        var result = FollowSuitValidator.GetLegalPlays
        (
            Array.Empty<Card>(), Suit.Spades, Suit.Hearts, DefaultRules, currentWinningCard: null
        );

        Assert.Empty(result);
    }



    [Fact]
    public void GetLegalPlays_when_hand_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => FollowSuitValidator.GetLegalPlays(null!, Suit.Spades, Suit.Hearts, DefaultRules, currentWinningCard: null)
        );
    }



    [Fact]
    public void GetLegalPlays_when_rules_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => FollowSuitValidator.GetLegalPlays(Array.Empty<Card>(), Suit.Spades, Suit.Hearts, null!, currentWinningCard: null)
        );
    }
}
