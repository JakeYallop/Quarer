using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Benchmarks).Assembly).Run(args: args);

[HideColumns("RatioSD", "StdDev")]
[MemoryDiagnoser(displayGenColumns: false)]
[ShortRunJob]
[Outliers(Perfolizer.Mathematics.OutlierDetection.OutlierMode.RemoveAll)]
#pragma warning disable CA1050 // Declare types in namespaces
public partial class Benchmarks
{
}
