namespace Wolfgang.Hawsey.Engine.Tests.Unit.PropertyBased;

/// <summary>
/// Serializes the tests that change <see cref="FuzzPropertyAttribute.MaxTestVariable"/>.
/// </summary>
[CollectionDefinition(nameof(FuzzEnvironmentCollection), DisableParallelization = true)]
public sealed class FuzzEnvironmentCollection;
