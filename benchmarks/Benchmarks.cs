using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>(args: args);

[HideColumns("RatioSD", "StdDev")]
[IterationTime(1000)]
[MemoryDiagnoser]
[ShortRunJob]
#pragma warning disable CA1050 // Declare types in namespaces
public partial class Benchmarks
{
}
