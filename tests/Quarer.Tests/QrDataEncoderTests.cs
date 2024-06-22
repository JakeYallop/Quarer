namespace Quarer.Tests;

public sealed class QrDataEncoderTests
{
    [Theory]
    [InlineData("1234567890", ModeIndicator.Numeric)]
    [InlineData("HELLO123", ModeIndicator.Alphanumeric)]
    [InlineData("HELLO WORLD", ModeIndicator.Alphanumeric)]
    [InlineData("\u935F\uE4AA", ModeIndicator.Kanji)]
    [InlineData("Hello, \u4E16\u754C!", ModeIndicator.Byte)]
    public void DeriveMode_ValidData_ReturnsCorrectMode(string data, ModeIndicator expectedMode)
    {
        var mode = QrDataEncoder.DeriveMode(data);
        Assert.Equal(expectedMode, mode);
    }

    [Theory]
    [InlineData("1234567890", ModeIndicator.Numeric, 34)]
    [InlineData("HELLO123", ModeIndicator.Alphanumeric, 44)]
    [InlineData("HELLO WORLD", ModeIndicator.Alphanumeric, 61)]
    [InlineData("こんにちは", ModeIndicator.Kanji, 65)]
    [InlineData("Hello, \u4E16\u754C!", ModeIndicator.Byte, 160)]
    public void GetBitStreamLength_ValidData_ReturnsCorrectLength(string data, ModeIndicator mode, int expectedLength)
    {
        var length = mode.GetBitStreamLength(data);
        Assert.Equal(expectedLength, length);
    }

    [Fact]
    public void AnalyzeSimple_ValidNumericData_ReturnsCorrectEncoding()
    {
        var data = "1234567890";
        var errorCorrectionLevel = ErrorCorrectionLevel.M;

        var encoding = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.NotNull(encoding);
        Assert.Single(encoding.DataSegments);
        Assert.Equal(ModeIndicator.Numeric, encoding.DataSegments[0].Mode);
        Assert.Equal(QrAnalysisResult.Success, encoding.Result);
    }

    [Fact]
    public void AnalyzeSimple_ValidAlphanumericData_ReturnsCorrectEncoding()
    {
        var data = "HELLO123";
        var errorCorrectionLevel = ErrorCorrectionLevel.L;

        var encoding = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.NotNull(encoding);
        Assert.Single(encoding.DataSegments);
        Assert.Equal(ModeIndicator.Alphanumeric, encoding.DataSegments[0].Mode);
        Assert.Equal(QrAnalysisResult.Success, encoding.Result);
    }

    [Fact]
    public void AnalyzeSimple_DataTooLarge_ReturnsEncodingWithDefaultVersion_AndDataTooLargeResult()
    {
        var data = new string('A', 5000); // Exceed typical QR code capacity
        var errorCorrectionLevel = ErrorCorrectionLevel.Q;

        var encoding = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.NotNull(encoding);

        Assert.Equal(default, encoding.Version);
        Assert.Equal(default, encoding.DataSegments);
        Assert.Equal(QrAnalysisResult.DataTooLarge, encoding.Result);
    }
}
