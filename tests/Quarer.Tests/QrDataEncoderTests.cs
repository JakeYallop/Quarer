using System.Text;

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
        Assert.Single(result.Value.DataSegments);
        Assert.Equal(ModeIndicator.Numeric, result.Value.DataSegments[0].Mode);
        Assert.Equal(AnalysisResult.Success, result.Reason);
        Assert.Equal(1, result.Value.Version.Version);
    }

    [Fact]
    public void AnalyzeSimple_ValidAlphanumericData_ReturnsCorrectEncoding()
    {
        var data = "HELLO123";
        var errorCorrectionLevel = ErrorCorrectionLevel.L;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.True(result.Success);
        Assert.NotNull(result);
        Assert.Single(result.Value.DataSegments);
        Assert.Equal(ModeIndicator.Alphanumeric, result.Value.DataSegments[0].Mode);
        Assert.Equal(AnalysisResult.Success, result.Reason);
        Assert.Equal(1, result.Value.Version.Version);
    }

    [Fact]
    public void AnalyzeSimple_ValidAlphanumericData_ReturnsCorrectEncoding2()
    {
        var data = "HELLO WORLD 123456790";
        var errorCorrectionLevel = ErrorCorrectionLevel.H;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.True(result.Success);
        Assert.NotNull(result);
        Assert.Single(result.Value.DataSegments);
        Assert.Equal(ModeIndicator.Alphanumeric, result.Value.DataSegments[0].Mode);
        Assert.Equal(AnalysisResult.Success, result.Reason);
        Assert.Equal(3, result.Value.Version.Version);
    }

    [Fact]
    public void AnalyzeSimple_DataTooLarge_ReturnsEncodingWithDefaultVersion_AndDataTooLargeResult()
    {
        var data = new string('A', 5000); // Exceed typical QR code capacity
        var errorCorrectionLevel = ErrorCorrectionLevel.Q;

        var result = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.Equal(AnalysisResult.DataTooLarge, result.Reason);
    }

    [Fact]
    public void EncodeDataBits_ValidNumericData_ReturnsExpectedBitStream()
    {
        var tripletOne = 012;
        var tripletTwo = 345;
        var doubleDigit = 67;
        var data = $"0{tripletOne}{tripletTwo}{doubleDigit}";
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var version = QrVersion.GetVersion(1);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, ModeIndicator.Numeric, NumericEncoder.GetBitStreamLength(data), new(0, data.Length))]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        // 16 data codeword capacity
        Assert.Equal(16, bitBuffer.ByteCount);
        AssertExtensions.BitsEqual($"""
            {(int)ModeIndicator.Numeric:B4}
            {data.Length.ToString($"B{characterBitCount}")}
            {tripletOne:B10}{tripletTwo:B10}{doubleDigit:B7}
            {"0000"}
            {"000"}
            {QrDataEncoder.PadPattern32Bits:B32}
            {QrDataEncoder.PadPattern32Bits:B32}
            {QrDataEncoder.PadPattern8_1:B8}
            {QrDataEncoder.PadPattern8_2:B8}
            """, bitBuffer.AsBitEnumerable(), divideIntoBytes: true);
    }

    [Fact]
    public void EncodeDataBits_ValidAlphanumericData_ReturnsExpectedBitStream2()
    {
        var pair1 = "AB";
        var pair2 = "BC";
        var pair3 = "/*";
        var pair4 = "FG";
        var pair5 = "$.";
        var pair6 = "JK";
        var pair7 = ": ";
        var data = $"{pair1}{pair2}{pair3}{pair4}{pair5}{pair6}{pair7}";
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var version = QrVersion.GetVersion(1);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Alphanumeric);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, ModeIndicator.Alphanumeric, AlphanumericEncoder.GetBitStreamLength(data), new(0, data.Length))]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        var sb = new StringBuilder();

        // 16 data codeword capacity
        Assert.Equal(16, bitBuffer.ByteCount);
        AssertExtensions.BitsEqual($"""
            {(int)ModeIndicator.Alphanumeric:B4}
            {data.Length.ToString($"B{characterBitCount}")}
            {Bits(pair1)}{Bits(pair2)}{Bits(pair3)}{Bits(pair4)}{Bits(pair5)}{Bits(pair6)}{Bits(pair7)}
            {"0000"}
            {"00"}
            {QrDataEncoder.PadPattern32Bits:B32}
            """, bitBuffer.AsBitEnumerable(), divideIntoBytes: true);

        string Bits(ReadOnlySpan<char> pair)
        {
            var writer = new BitWriter();
            AlphanumericEncoder.Encode(writer, pair);
            sb.Clear();
            foreach (var b in writer.Buffer.AsBitEnumerable())
            {
                sb.Append(b ? '1' : '0');
            }
            return sb.ToString();
        }
    }

    public static TheoryData<QrVersion, ErrorCorrectionLevel, BitBuffer, byte[]> EncodeAndInterleaveErrorCorrectionBlocksData()
    {
        return new()
        {
            {
                QrVersion.GetVersion(1), ErrorCorrectionLevel.H,
                Buffer([32, 65, 205, 69, 41, 220, 46, 128, 236]),
                [
                    //data codewords
                    32, 65, 205, 69, 41, 220, 46, 128, 236,
                    //error codewords
                    42, 159, 74, 221, 244, 169, 239, 150, 138, 70, 237, 85, 224, 96, 74, 219, 61
                ]
            },
            {
                QrVersion.GetVersion(5), ErrorCorrectionLevel.H,
                Buffer(
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

        static BitBuffer Buffer(ReadOnlySpan<byte> bytes)
        {
            var bitBuffer = new BitBuffer(bytes.Length * 8);
            var writer = new BitWriter(bitBuffer);
            foreach (var b in bytes)
            {
                writer.WriteBitsBigEndian(b, 8);
            }
            return bitBuffer;
        }
    }

    [Theory]
    [MemberData(nameof(EncodeAndInterleaveErrorCorrectionBlocksData))]
    public void EncodeAndInterleaveErrorCorrectionBlocks_ReturnsExpectedResult1(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, BitBuffer dataCodewordsBitBuffer, byte[] expectedBytes)
    {
        var resultBitBuffer = QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(dataCodewordsBitBuffer, version, errorCorrectionLevel);

        Assert.Equal(expectedBytes, resultBitBuffer.AsByteEnumerable().ToArray());
    }

    [Fact]
    public void EncodeAndInterleaveErrorCorrectionBlocks_BitWriterInputSizeDoesNotMatchExpectedSizeFromVersion_ThrowsArgumentException()
    {
        var version = QrVersion.GetVersion(1);
        var dataCodewordsBitBuffer = new BitBuffer(1);
        Assert.Throws<ArgumentException>(() => QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(dataCodewordsBitBuffer, version, ErrorCorrectionLevel.H));
    }
}
