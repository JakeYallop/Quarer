using BenchmarkDotNet.Attributes;
using Quarer;

public partial class Benchmarks
{
    [ParamsSource(nameof(Params))]
    public string Data { get; set; } = null!;

    public static IEnumerable<string> Params()
    {
        yield return "1234567";
        yield return "A short alphanumeric string with some extra bits on the end ::::::*******";
        yield return new string('A', 3000);
        yield return new string('1', 5596); //max size 40M numeric QR code
    }

    public SkiaSharp.QrCode.QRCodeGenerator SkiaGenerator = null!;

    [GlobalSetup]
    public void Setup() => SkiaGenerator = new SkiaSharp.QrCode.QRCodeGenerator();

    [Benchmark(Description = "Quarer")]
    public QrCode Quarer() => QrCode.Create(Data, ErrorCorrectionLevel.M);

    [Benchmark(Description = "Apose.BarCode")]
    public void AposeCode()
    {
        using var generator = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.QR, Data);
        generator.Parameters.Barcode.QR.QrErrorLevel = Aspose.BarCode.Generation.QRErrorLevel.LevelM;
        generator.Save(Stream.Null, Aspose.BarCode.Generation.BarCodeImageFormat.Png);
    }

    [Benchmark(Description = "QrCode.Net")]
    public Gma.QrCodeNet.Encoding.QrCode GmaQrCodeNet() => new Gma.QrCodeNet.Encoding.QrEncoder().Encode(Data);

    [Benchmark(Description = "QrCoder")]
    public QRCoder.QRCodeData QrCoder() => QRCoder.QRCodeGenerator.GenerateQrCode(Data, QRCoder.QRCodeGenerator.ECCLevel.Default);

    [Benchmark(Description = "SkiaSharp.QrCode")]
    public SkiaSharp.QrCode.QRCodeData SkiaSharpQrCode() => SkiaGenerator.CreateQrCode(Data, SkiaSharp.QrCode.ECCLevel.M);

    [Benchmark(Description = "ZXing.Net", Baseline = true)]
    public ZXing.QrCode.Internal.QRCode ZXingQrCode() => ZXing.QrCode.Internal.Encoder.encode(Data, ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
}
