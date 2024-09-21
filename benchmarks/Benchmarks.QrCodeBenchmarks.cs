using BenchmarkDotNet.Attributes;
using Quarer;

#pragma warning disable CA1050
public partial class Benchmarks
{
    [Params("1234567", "A short alphanumeric string with some extra bits on the end ::::::*******")]
    public string Data { get; set; } = null!;

    [Benchmark(Description = "Quarer")]
    public QrCode Quarer() => QrCode.Create(Data);
}
