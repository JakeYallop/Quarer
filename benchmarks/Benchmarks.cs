using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>(args: args);

[IterationTime(1000)]
[MemoryDiagnoser]
[ShortRunJob]
#pragma warning disable CA1050 // Declare types in namespaces
public partial class Benchmarks
{
}
