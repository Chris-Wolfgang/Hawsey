using System.Globalization;
using FsCheck.Xunit;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// An FsCheck <see cref="PropertyAttribute"/> whose case count comes from the
/// <c>HAWSEY_FUZZ_MAX_TEST</c> environment variable. Normal test runs leave it
/// unset and check 100 cases per property. The weekly fuzz workflow sets it to
/// 100,000 and runs the same properties as a long fuzzing pass.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class FuzzPropertyAttribute : PropertyAttribute
{
    /// <summary>The environment variable that overrides the per-property case count.</summary>
    public const string MaxTestVariable = "HAWSEY_FUZZ_MAX_TEST";

    private const int DefaultMaxTest = 100;



    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzPropertyAttribute"/> class.
    /// </summary>
    public FuzzPropertyAttribute()
    {
        MaxTest = int.TryParse
        (
            Environment.GetEnvironmentVariable(MaxTestVariable),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var maxTest
        ) && maxTest > 0
            ? maxTest
            : DefaultMaxTest;
    }
}
