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

    public static TheoryData<QrVersion, BitWriter, byte[]> EncodeAndInterleaveErrorCorrectionBlocksData()
    {
        return new()
        {
            {
                QrVersion.GetVersion(1, ErrorCorrectionLevel.H),
                Writer([32, 65, 205, 69, 41, 220, 46, 128, 236]),
                [
                    //data codewords
                    32, 65, 205, 69, 41, 220, 46, 128, 236,
                    //error codewords
                    42, 159, 74, 221, 244, 169, 239, 150, 138, 70, 237, 85, 224, 96, 74, 219, 61
                ]
            },
            {
                QrVersion.GetVersion(5, ErrorCorrectionLevel.H),
                Writer(
                [
                    //each line is one data block
                    67, 70, 22, 38, 54, 70, 86, 102, 118, 108, 1,
                    134, 150, 166, 182, 198, 214, 230, 247, 7, 90, 254,
                    23, 39, 55, 71, 87, 103, 119, 135, 151, 81, 56, 123,
                    166, 22, 38, 54, 70, 86, 102, 118, 134, 87, 42, 55
                ]),
                [
                    //data codewords
                    67, 134, 23, 166, 70, 150, 39, 22, 22, 166, 55, 38, 38, 182, 71, 54, 54, 198,
                    87, 70, 70, 214, 103, 86, 86, 230, 119, 102, 102, 247, 135, 118, 118, 7, 151,
                    134, 108, 90, 81, 87, 1, 254, 56, 42, 123, 55,

                    //error codewords
                    197, 19, 2, 205, 3, 78, 176, 65, 115, 34, 136, 166, 194, 113, 179, 129, 46, 17,
                    128, 188, 161, 242, 19, 64, 167, 46, 24, 3, 202, 208, 17, 88, 40, 105, 109,
                    177, 133, 54, 128, 227, 195, 122, 179, 202, 90, 240, 214, 205, 69, 51, 5, 201,
                    45, 95, 135, 193, 28, 44, 78, 239, 26, 239, 62, 119, 13, 186, 68, 132, 126,
                    195, 171, 59, 224, 17, 192, 128, 103, 244, 104, 245, 61, 121, 49, 46, 104, 64,
                    117, 83,
                ]
            },
        };

        static BitWriter Writer(ReadOnlySpan<byte> bytes)
        {
            var bitWriter = new BitWriter(bytes.Length * 8);
            foreach (var b in bytes)
            {
                bitWriter.WriteBits(b, 8);
            }
            return bitWriter;
        }
    }

    [Theory]
    [MemberData(nameof(EncodeAndInterleaveErrorCorrectionBlocksData))]
    public void EncodeAndInterleaveErrorCorrectionBlocks_ReturnsExpectedResult1(QrVersion version, BitWriter dataCodewordsBitBuffer, byte[] expectedBytes)
    {
        var resultBitWriter = QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(version, dataCodewordsBitBuffer);

        Assert.Equal(expectedBytes, resultBitWriter.GetByteStream().ToArray());
    }

    [Fact]
    public void EncodeAndInterleaveErrorCorrectionBlocks_BitWriterInputSizeDoesNotMatchExpectedSizeFromVersion_ThrowsArgumentException()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.H);
        var dataCodewordsBitBuffer = new BitWriter(1);
        Assert.Throws<ArgumentException>(() => QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(version, dataCodewordsBitBuffer));
    }
}
