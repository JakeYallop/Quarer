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

    [Fact]
    public void AnalyzeSimple_ValidNumericData_ReturnsCorrectEncoding()
    {
        var data = "1234567890";
        var errorCorrectionLevel = ErrorCorrectionLevel.M;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.True(result.Success);
        Assert.NotNull(result);
        Assert.Single(result.Result.DataSegments);
        Assert.Equal(ModeIndicator.Numeric, result.Result.DataSegments[0].Mode);
        Assert.Equal(AnalysisResult.Success, result.AnalysisResult);
    }

    [Fact]
    public void AnalyzeSimple_ValidAlphanumericData_ReturnsCorrectEncoding()
    {
        var data = "HELLO123";
        var errorCorrectionLevel = ErrorCorrectionLevel.L;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.True(result.Success);
        Assert.NotNull(result);
        Assert.Single(result.Result.DataSegments);
        Assert.Equal(ModeIndicator.Alphanumeric, result.Result.DataSegments[0].Mode);
        Assert.Equal(AnalysisResult.Success, result.AnalysisResult);
    }

    [Fact]
    public void AnalyzeSimple_DataTooLarge_ReturnsEncodingWithDefaultVersion_AndDataTooLargeResult()
    {
        var data = new string('A', 5000); // Exceed typical QR code capacity
        var errorCorrectionLevel = ErrorCorrectionLevel.Q;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.NotNull(result);
        Assert.Null(result.Result);
        Assert.Equal(AnalysisResult.DataTooLarge, result.AnalysisResult);
    }

    [Fact]
    public void EncodeDataBitStream_ValidNumericData_ReturnsExpectedBitStream()
    {
        var data = "1234567890";
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var version = QrVersion.GetVersion(1, errorCorrectionLevel); //34 input data character capacity
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var encodingInfo = new QrEncodingInfo(version, [DataSegment.Create(characterCount, ModeIndicator.Numeric, NumericEncoder.GetBitStreamLength(data), new(0, data.Length))]);

        var bitStream = QrDataEncoder.EncodeDataBitStream(encodingInfo, data).ToArray();
        // mode indicator, character count, data bits, terminator
        var expectedCount = 4 + characterCount + NumericEncoder.GetBitStreamLength(new char[34]) + 4;
        Assert.Equal(expectedCount / 8, bitStream.Length);
    }
}
