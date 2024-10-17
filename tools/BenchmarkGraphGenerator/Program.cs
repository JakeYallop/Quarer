using System.Collections.Frozen;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using ScottPlot;
using ScottPlot.Plottables;

const string BenchmarkResultsPath = "../../../../../benchmarks/BenchmarkDotNet.Artifacts/results/Benchmarks-report.csv";
if (!File.Exists(BenchmarkResultsPath))
{
    var oldBackgroundColor = Console.BackgroundColor;
    var oldForegroundColor = Console.ForegroundColor;
    Console.BackgroundColor = ConsoleColor.Black;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Benchmark results file not found. Please run the benchmarks project first (remmeber to run it in release configuration).");
    Console.BackgroundColor = oldBackgroundColor;
    Console.ForegroundColor = oldForegroundColor;
    return;
}

using var streamReader = new StreamReader(BenchmarkResultsPath);
using var csvParser = new CsvReader(streamReader, CultureInfo.InvariantCulture);

var results = new List<Result>(160);
var uniqueMethods = new HashSet<string>();

// Exclude the following libraries from the graph as they are signficantly slower than the others and throw off the scale
var excluded = new string[] { "Apose.BarCode", "SkiaSharp.QrCode" }.ToFrozenSet();

foreach (var record in csvParser.EnumerateRecords(new BenchmarkRecord()))
{
    if (excluded.Contains(record.Method))
    {
        continue;
    }

    var versionSpaceIndex = record.Version.IndexOf(' ');
    var version = int.Parse(record.Version[(versionSpaceIndex + 1)..]);

    var meanSpan = record.Mean.AsSpan();
    var meanSpaceIndex = meanSpan.IndexOf(' ');
    var mean = double.Parse(meanSpan[..meanSpaceIndex]);
    var meanUnits = meanSpan[(meanSpaceIndex + 1)..];

    var allocatedSpan = record.Allocated.AsSpan();
    var allocatedIndexOfSpace = allocatedSpan.IndexOf(' ');
    var allocated = double.Parse(allocatedSpan[..allocatedIndexOfSpace]);
    var allocatedUnits = allocatedSpan[(allocatedIndexOfSpace + 1)..];

    results.Add(new(record.Method, version, mean, meanUnits.ToString(), allocated, allocatedUnits.ToString()));

    uniqueMethods.Add(record.Method);
}

var sortedByVersion = results.OrderBy(x => x.Method != "Quarer").ThenBy(x => x.Method).ThenBy(x => x.Version).ToArray();

var timingPlot = new Plot();
var memoryPlot = new Plot();

var legend = new List<LegendItem>();

foreach (var chunkedResults in sortedByVersion.Chunk(40))
{
    var color = timingPlot.Add.GetNextColor();

    var xs = chunkedResults.Select(x => x.Version).ToArray();
    var timingYs = chunkedResults.Select(x => x.Mean).ToArray();
    var memoryYs = chunkedResults.Select(x => x.Allocated).ToArray();
    timingPlot.Add.ScatterLine(xs, timingYs, color);
    memoryPlot.Add.ScatterLine(xs, memoryYs, color);
    legend.Add(new()
    {
        LineColor = color,
        LineWidth = 3,
        LabelText = chunkedResults[0].Method,
    });
};

var timingUnits = results[0].MeanUnits;
var memoryUnits = results[0].AllocatedUnits;
SavePlot(timingPlot, "timing", "Time", $"Encoding time ({timingUnits}), numeric data, error correction level M", legend, sortedByVersion.Max(x => x.Mean * 1.05));
SavePlot(memoryPlot, "memory", "Allocated", $"Memory allocated ({memoryUnits}), numeric data, error correction level M", legend, sortedByVersion.Max(x => x.Allocated * 1.05));

static void SavePlot(Plot plot, string plotName, string yLabel, string title, List<LegendItem> legend, double yLimit)
{
    plot.Title(title);
    plot.XLabel("Version");
    plot.YLabel(yLabel);
    plot.Axes.SetLimitsX(1, 40);
    plot.Axes.SetLimitsY(0, yLimit);

    plot.ShowLegend(legend);
    plot.Legend.Alignment = Alignment.UpperLeft;
    plot.SavePng($"{plotName}.png", 1200, 700);
}

#pragma warning disable CA1050 // Declare types in namespaces
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
    public string Version { get; init; } = null!;
    public string Mean { get; init; } = null!;
    public string Error { get; init; } = null!;
    public string StdDev { get; init; } = null!;
    public string Ratio { get; init; } = null!;
    public string RatioSD { get; init; } = null!;
    public string Allocated { get; init; } = null!;
    [Name("Alloc Ratio")]
    public string AllocRatio { get; init; } = null!;
}

public sealed record Result(string Method, int Version, double Mean, string MeanUnits, double Allocated, string AllocatedUnits);

public sealed class SpecificStringFirstComparer(string specialString) : Comparer<string>
{
    public override int Compare(string? x, string? y) => x is null ? (y is null ? 0 : -1) : (x == specialString ? 1 : x.CompareTo(y));
}
