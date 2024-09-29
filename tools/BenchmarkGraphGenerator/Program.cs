// See https://aka.ms/new-console-template for more information
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using ScottPlot;

if (!File.Exists("../../../../../benchmarks/bin/Release/net9.0/BenchmarkDotNet.Artifacts/results/Benchmarks-report.csv"))
{
    Console.BackgroundColor = ConsoleColor.Black;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Benchmark results file not found. Please run the benchmarks project first (remmeber to run it in release configuration).");
    return;
}

using var streamReader = new StreamReader("../../../../../benchmarks/bin/Release/net9.0/BenchmarkDotNet.Artifacts/results/Benchmarks-report.csv");
using var csvParser = new CsvReader(streamReader, CultureInfo.InvariantCulture);

var results = new List<Result>(160);
var uniqueMethods = new HashSet<string>();

foreach (var record in csvParser.EnumerateRecords(new BenchmarkRecord()))
{
    var versionSpaceIndex = record.Numeric.IndexOf(' ');
    var version = int.Parse(record.Numeric[(versionSpaceIndex + 1)..]);

    var meanSpan = record.Mean.AsSpan();
    var meanSpaceIndex = meanSpan.IndexOf(' ');
    var mean = double.Parse(meanSpan[..meanSpaceIndex]);
    var meanUnit = meanSpan[(meanSpaceIndex + 1)..];
    var nanosecondMean = GetNanosecondsValue(meanUnit, mean);

    var allocatedSpan = record.Allocated.AsSpan();
    var allocatedIndexOfSpace = allocatedSpan.IndexOf(' ');
    var allocated = double.Parse(allocatedSpan[..allocatedIndexOfSpace]);
    var allocatedUnit = allocatedSpan[(allocatedIndexOfSpace + 1)..];
    var bytesAllocated = GetBytesValue(allocatedUnit, allocated);

    results.Add(new(record.Method, version, nanosecondMean, bytesAllocated));

    uniqueMethods.Add(record.Method);
}

var sortedByVersion = results.OrderBy(x => x.Method != "Quarer").ThenBy(x => x.Method).ThenBy(x => x.Version).ToArray();

var timingPlot = new Plot();
var memoryPlot = new Plot();

foreach (var chunkedResults in sortedByVersion.Chunk(40))
{
    var color = timingPlot.Add.GetNextColor();

    var xs = chunkedResults.Select(x => x.Version).ToArray();
    var timingYs = chunkedResults.Select(x => x.MeanNanonseconds).ToArray();
    var memoryYs = chunkedResults.Select(x => x.AllocatedBytes).ToArray();
    timingPlot.Add.ScatterLine(xs, timingYs, color);
    memoryPlot.Add.ScatterLine(xs, memoryYs, color);
};

timingPlot.Title("Encoding time (ns), numeric data, error correction level M");
memoryPlot.Title("Memory allocated (B), numeric data, error correction level M");
timingPlot.XLabel("QR Code version");
memoryPlot.XLabel("QR Code version");

timingPlot.Axes.SetLimitsX(1, 40);
timingPlot.Axes.SetLimitsY(0, sortedByVersion.Max(x => x.MeanNanonseconds * 1.05));
memoryPlot.Axes.SetLimitsX(1, 40);
memoryPlot.Axes.SetLimitsY(0, sortedByVersion.Max(x => x.AllocatedBytes * 1.05));
timingPlot.ShowLegend();
memoryPlot.ShowLegend();
timingPlot.SavePng("timing.png", 1200, 700);
memoryPlot.SavePng("memory.png", 1200, 700);

static double GetNanosecondsValue(ReadOnlySpan<char> unit, double value)
{
    return unit switch
    {
        "ns" => value,
        "μs" => value * 1_000,
        "ms" => value * 1_000_000,
        "s" => value * 1_000_000_000,
        _ => throw new NotSupportedException($"Unit {unit} not recognised.")
    };
}

static double GetBytesValue(ReadOnlySpan<char> unit, double value)
{
    return unit switch
    {
        "B" => value,
        "KB" => value * 1_024,
        "MB" => value * 1_048_576,
        "GB" => value * 1_073_741_824,
        _ => throw new NotSupportedException($"Unit {unit} not recognised.")
    };
}

public sealed class BenchmarkRecord
{
    public string Method { get; init; } = null!;
    public string Job { get; init; } = null!;
    public string AnalyzeLaunchVariance { get; init; } = null!;
    public string EvaluateOverhead { get; init; } = null!;
    public string MaxAbsoluteError { get; init; } = null!;
    public string MaxRelativeError { get; init; } = null!;
    public string MinInvokeCount { get; init; } = null!;
    public string MinIterationTime { get; init; } = null!;
    public string OutlierMode { get; init; } = null!;
    public string Affinity { get; init; } = null!;
    public string EnvironmentVariables { get; init; } = null!;
    public string Jit { get; init; } = null!;
    public string LargeAddressAware { get; init; } = null!;
    public string Platform { get; init; } = null!;
    public string PowerPlanMode { get; init; } = null!;
    public string Runtime { get; init; } = null!;
    public string AllowVeryLargeObjects { get; init; } = null!;
    public string Concurrent { get; init; } = null!;
    public string CpuGroups { get; init; } = null!;
    public string Force { get; init; } = null!;
    public string HeapAffinitizeMask { get; init; } = null!;
    public string HeapCount { get; init; } = null!;
    public string NoAffinitize { get; init; } = null!;
    public string RetainVm { get; init; } = null!;
    public string Server { get; init; } = null!;
    public string Arguments { get; init; } = null!;
    public string BuildConfiguration { get; init; } = null!;
    public string Clock { get; init; } = null!;
    public string EngineFactory { get; init; } = null!;
    public string NuGetReferences { get; init; } = null!;
    public string Toolchain { get; init; } = null!;
    public string IsMutator { get; init; } = null!;
    public string InvocationCount { get; init; } = null!;
    public string IterationCount { get; init; } = null!;
    public string IterationTime { get; init; } = null!;
    public string LaunchCount { get; init; } = null!;
    public string MaxIterationCount { get; init; } = null!;
    public string MaxWarmupIterationCount { get; init; } = null!;
    public string MemoryRandomization { get; init; } = null!;
    public string MinIterationCount { get; init; } = null!;
    public string MinWarmupIterationCount { get; init; } = null!;
    public string RunStrategy { get; init; } = null!;
    public string UnrollFactor { get; init; } = null!;
    public string WarmupCount { get; init; } = null!;
    public string Numeric { get; init; } = null!;
    public string Mean { get; init; } = null!;
    public string Error { get; init; } = null!;
    public string StdDev { get; init; } = null!;
    public string Ratio { get; init; } = null!;
    public string RatioSD { get; init; } = null!;
    public string Gen0 { get; init; } = null!;
    public string Gen1 { get; init; } = null!;
    public string Gen2 { get; init; } = null!;
    public string Allocated { get; init; } = null!;
    [Name("Alloc Ratio")]
    public string AllocRatio { get; init; } = null!;
}

public sealed record Result(string Method, int Version, double MeanNanonseconds, double AllocatedBytes);

public sealed class SpecificStringFirstComparer(string specialString) : Comparer<string>
{
    public override int Compare(string? x, string? y) => x is null ? (y is null ? 0 : -1) : (x == specialString ? 1 : x.CompareTo(y));
}
