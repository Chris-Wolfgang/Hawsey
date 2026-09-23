using System.Globalization;
using Wolfgang.Hawsey.Engine.Bidding;
using Wolfgang.Hawsey.Engine.Cards;
using Wolfgang.Hawsey.Engine.Players;
using Wolfgang.Hawsey.Engine.Rules;
using Wolfgang.Hawsey.Engine.Strategy;
using Wolfgang.Hawsey.Engine.Tests.Unit.Helpers;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Globalization;

/// <summary>
/// Runs engine behaviour under cultures that break naive formatting and
/// comparison (dotted-I, decimal comma, CJK collation, RTL with native digits,
/// full-width digits) and asserts every result matches the invariant culture.
/// </summary>
/// <remarks>
/// Culture-sensitivity allowlist: none. No public engine member is
/// intentionally culture-sensitive. Game logic compares enums and integers
/// only, and every string the engine produces (<see cref="Card.ToString"/>,
/// exception messages) is built from enum names and integers, which format the
/// same in every culture. Any new culture-dependent member must be added here
/// with a reason.
/// </remarks>
public class CultureInvarianceTests
{
    public static TheoryData<string> Cultures => new()
    {
        "en-US",
        "tr-TR",
        "de-DE",
        "zh-CN",
        "ar-SA",
        "ja-JP"
    };



    /// <summary>
    /// Runs <paramref name="action"/> with both <see cref="CultureInfo.CurrentCulture"/>
    /// and <see cref="CultureInfo.CurrentUICulture"/> set to <paramref name="cultureName"/>,
    /// restoring the originals afterwards even if it throws.
    /// </summary>
    private static T InCulture<T>(string cultureName, Func<T> action)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var culture = new CultureInfo(cultureName);

        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }



    private static string[] FullDeckText() =>
        Deck.CreatePinochleDeck().Select(c => c.ToString()).ToArray();



    private static (int NorthSouth, int EastWest, int Tricks) PlayGame()
    {
        var state = new GameRunner().RunGame
        (
            new TestPlayerStrategy(),
            new HouseRules { MustBeat = true },
            PlayerPosition.South,
            new Random(1234)
        );

        return (state.NorthSouthScore, state.EastWestScore, state.CompletedTricks.Count);
    }



    private static string BelowMinimumMessage() =>
        Assert.Throws<InvalidOperationException>
        (
            () => new BiddingPhase(PlayerPosition.North, 6).PlaceBid(PlayerPosition.East, new BidAction.NumberBid(5))
        ).Message;



    [Theory]
    [MemberData(nameof(Cultures))]
    public void Card_ToString_in_any_culture_matches_the_invariant_culture(string culture)
    {
        var expected = InCulture(CultureInfo.InvariantCulture.Name, FullDeckText);

        var actual = InCulture(culture, FullDeckText);

        Assert.Equal(expected, actual);
    }



    [Theory]
    [MemberData(nameof(Cultures))]
    public void RunGame_in_any_culture_plays_the_same_game_as_the_invariant_culture(string culture)
    {
        var expected = InCulture(CultureInfo.InvariantCulture.Name, PlayGame);

        var actual = InCulture(culture, PlayGame);

        Assert.Equal(expected, actual);
    }



    [Theory]
    [MemberData(nameof(Cultures))]
    public void PlaceBid_error_message_in_any_culture_matches_the_invariant_culture(string culture)
    {
        var expected = InCulture(CultureInfo.InvariantCulture.Name, BelowMinimumMessage);

        var actual = InCulture(culture, BelowMinimumMessage);

        Assert.Equal(expected, actual);
    }



    [Theory]
    [MemberData(nameof(Cultures))]
    public void InCulture_applies_both_cultures_inside_the_action(string culture)
    {
        var applied = InCulture(culture, () => (CultureInfo.CurrentCulture.Name, CultureInfo.CurrentUICulture.Name));

        Assert.Equal((culture, culture), applied);
    }



    [Theory]
    [MemberData(nameof(Cultures))]
    public void InCulture_when_the_action_throws_restores_the_original_cultures(string culture)
    {
        var before = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);

        Assert.Throws<InvalidOperationException>
        (
            () => InCulture<int>(culture, () => throw new InvalidOperationException("boom"))
        );

        Assert.Equal(before, (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture));
    }
}
