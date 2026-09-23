using BenchmarkDotNet.Running;

namespace Wolfgang.Hawsey.Engine.Benchmarks;

internal static class Program
{
    private static void Main(string[] args) =>
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
}
