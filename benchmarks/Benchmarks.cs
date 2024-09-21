using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>(args: args);

[MemoryDiagnoser]
[MemoryRandomization]
[MarkdownExporterAttribute.GitHub]
public partial class Benchmarks
{
}
