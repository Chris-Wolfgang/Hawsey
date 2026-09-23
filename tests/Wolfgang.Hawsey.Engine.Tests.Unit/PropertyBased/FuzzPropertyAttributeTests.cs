namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// Pins the <see cref="FuzzPropertyAttribute.MaxTestVariable"/> contract the weekly
/// fuzz workflow relies on. A parsing or variable-name regression would otherwise
/// silently shrink the long run to 100 cases.
/// </summary>
/// <remarks>
/// The environment variable is process-wide, so this class runs in a collection
/// with parallelization disabled and restores the original value after each case.
/// </remarks>
[Collection(nameof(FuzzEnvironmentCollection))]
public class FuzzPropertyAttributeTests
{
    [Theory]
    [InlineData("100000", 100000)]
    [InlineData("250", 250)]
    [InlineData(null, 100)]
    [InlineData("", 100)]
    [InlineData("not-a-number", 100)]
    [InlineData("0", 100)]
    [InlineData("-5", 100)]
    public void Ctor_reads_MaxTest_from_the_environment_or_falls_back_to_100(string? value, int expected)
    {
        var original = Environment.GetEnvironmentVariable(FuzzPropertyAttribute.MaxTestVariable);

        try
        {
            Environment.SetEnvironmentVariable(FuzzPropertyAttribute.MaxTestVariable, value);

            var attribute = new FuzzPropertyAttribute();

            Assert.Equal(expected, attribute.MaxTest);
        }
        finally
        {
            Environment.SetEnvironmentVariable(FuzzPropertyAttribute.MaxTestVariable, original);
        }
    }
}
