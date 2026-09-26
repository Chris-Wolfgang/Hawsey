using System.Reflection;
using Wolfgang.Hawsey.Engine.Game;

namespace Wolfgang.Hawsey.Engine.Tests.Unit.Packaging;

/// <summary>
/// Pins the engine's version attributes to one source, <c>&lt;Version&gt;</c> (#27):
/// FileVersion and InformationalVersion follow it, and AssemblyVersion stays at the
/// 1.0.0.0 pin recorded in ADR 0006.
/// </summary>
public class VersionMetadataTests
{
    private static readonly Assembly Engine = typeof(GameEngine).Assembly;



    private static string PackageVersion()
    {
        var informational = Engine.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        return informational.Split('+', '-')[0];
    }



    [Fact]
    public void AssemblyVersion_is_pinned_at_1_0_0_0()
    {
        Assert.Equal(new Version(1, 0, 0, 0), Engine.GetName().Version);
    }



    [Fact]
    public void FileVersion_is_the_package_version_with_a_zero_revision()
    {
        var fileVersion = Engine.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;

        Assert.Equal(PackageVersion() + ".0", fileVersion);
    }



    [Fact]
    public void InformationalVersion_is_a_three_part_package_version()
    {
        Assert.Equal(3, Version.Parse(PackageVersion()).ToString().Split('.').Length);
    }
}
