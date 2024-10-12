﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>(args: args);

[HideColumns("RatioSD", "StdDev")]
[MemoryDiagnoser]
[ShortRunJob]
[HtmlExporter]
[Outliers(Perfolizer.Mathematics.OutlierDetection.OutlierMode.RemoveAll)]
#pragma warning disable CA1050 // Declare types in namespaces
public partial class Benchmarks
{
}
