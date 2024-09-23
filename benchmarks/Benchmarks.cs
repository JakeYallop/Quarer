using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>(args: args);

[IterationTime(1000)]
[MemoryDiagnoser]
[ShortRunJob]
public partial class Benchmarks
{
}
