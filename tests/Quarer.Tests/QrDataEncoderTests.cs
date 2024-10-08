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
        var mode = QrDataEncoder.DeriveMode(Encoding.UTF8.GetBytes(data));
        Assert.Equal(expectedMode, mode);
    }

    [Fact]
    public void AnalyzeSimple_ValidNumericData_ReturnsCorrectEncoding()
    {
        var data = "1234567890"u8;
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
        var data = "HELLO123"u8;
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
        var data = "HELLO WORLD 123456790"u8;
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
        var data = Enumerable.Repeat((byte)'A', 5000).ToArray(); // Exceed typical QR code capacity
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
        var data = Encoding.UTF8.GetBytes($"0{tripletOne}{tripletTwo}{doubleDigit}").AsSpan();
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var version = QrVersion.GetVersion(1);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, ModeIndicator.Numeric, NumericEncoder.GetBitStreamLength(data), new(0, data.Length), EciCode.Empty)]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        // 16 data codeword capacity for 1M QR Code
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
    public void EncodeDataBits_ValidAlphanumericData_ReturnsExpectedBitStream()
    {
        var data = "ABBC/*FG$.JK: "u8;
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var mode = ModeIndicator.Alphanumeric;
        var version = QrVersion.GetVersion(1);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, mode, AlphanumericEncoder.GetBitStreamLength(data), new(0, data.Length), EciCode.Empty)]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        var sb = new StringBuilder();

        // 28 data codeword capacity for 2M QR Code
        Assert.Equal(16, bitBuffer.ByteCount);
        AssertExtensions.BitsEqual($"""
            {(int)mode:B4}
            {data.Length.ToString($"B{characterBitCount}")}
            {Bits(data, sb)}
            {"0000"}
            {"00"}
            {QrDataEncoder.PadPattern32Bits:B32}
            """, bitBuffer.AsBitEnumerable(), divideIntoBytes: true);

        static string Bits(ReadOnlySpan<byte> bytes, StringBuilder sb)
        {
            var writer = new BitWriter();
            AlphanumericEncoder.Encode(writer, bytes);
            sb.Clear();
            foreach (var b in writer.Buffer.AsBitEnumerable())
            {
                sb.Append(b ? '1' : '0');
            }
            return sb.ToString();
        }
    }

    [Fact]
    public void EncodeDataBits_ValidLatin1Data_ReturnsExpectedBitStream()
    {
        var data = Encoding.Latin1.GetBytes("Hello World! û þ ç Ã");
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var mode = ModeIndicator.Byte;
        var version = QrVersion.GetVersion(2);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, mode, data.Length, new(0, data.Length), EciCode.Empty)]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);

        // 28 data codeword capacity for 2M QR Code
        Assert.Equal(28, bitBuffer.ByteCount);
        // no eci code here, as the default byte encoding for a QR code is Latin1
        // 20 bytes + mode + character count = 21 + 4 bits
        // 21 + terminator (4 bits) + final byte padding = 22 bytes
        // 6 bytes of padding to reach 28 bytes
        AssertExtensions.BitsEqual($"""
            {(int)mode:B4}
            {data.Length.ToString($"B{characterBitCount}")}
            {ByteBits(data)}
            {"0000"}
            {QrDataEncoder.PadPattern32Bits:B32}
            {QrDataEncoder.PadPattern8_1:B8}
            {QrDataEncoder.PadPattern8_2:B8}
            """, bitBuffer.AsBitEnumerable(), divideIntoBytes: true);
    }

    [Fact]
    public void EncodeDataBits_Utf8Data_ReturnsExpectedBitStreamWithEciIndicator()
    {
        // \u1F604 - 😄, encoded as 0xF0 0x9F 0x98 0x84 in UTF-8
        var data = Encoding.UTF8.GetBytes("Hello World! 😄");
        var eciCode = new EciCode(26); // eci code for UTF-8 encoding
        var errorCorrectionLevel = ErrorCorrectionLevel.M;
        var mode = ModeIndicator.Byte;
        var version = QrVersion.GetVersion(2);
        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var encodingInfo = new QrEncodingInfo(version, errorCorrectionLevel, [DataSegment.Create(characterBitCount, mode, data.Length, new(0, data.Length), eciCode)]);

        var bitBuffer = QrDataEncoder.EncodeDataBits(encodingInfo, data);

        // 28 data codeword capacity for 2M QR Code
        Assert.Equal(28, bitBuffer.ByteCount);
        // 17 bytes of data
        // eci mode indicator (4 bits) + eci code (8 bits) + mode indicator (4 bits) + character count (8 bits) = 3 bytes
        // data + header data + terminator (4 bits) = 20 bytes and 4 bits
        // (19 and 4 bits) + final byte padding = 21 bytes (4 bits of padding)
        // 7 bytes of padding
        AssertExtensions.BitsEqual($"""
            {(int)ModeIndicator.Eci:B4}
            {eciCode.Value:B8}
            {(int)mode:B4}
            {data.Length.ToString($"B{characterBitCount}")}
            {ByteBits(data)}
            {"0000"}
            {"0000"}
            {QrDataEncoder.PadPattern32Bits:B32}
            {QrDataEncoder.PadPattern8_1:B8}
            {QrDataEncoder.PadPattern8_2:B8}
            {QrDataEncoder.PadPattern8_1:B8}
            """, bitBuffer.AsBitEnumerable(), divideIntoBytes: true);
    }

    private static string ByteBits(ReadOnlySpan<byte> bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append($"{b:B8}");
        }
        return sb.ToString();
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
                    197, 214, 240, 207, 3, 77, 203, 191, 115, 81, 2, 142, 194, 179, 5, 17, 46, 63, 241, 141, 161, 83,
                    194, 54, 167, 137, 53, 195, 202, 26, 236, 188, 40, 65, 88, 218, 133, 179, 236, 34, 195, 185, 138, 180,
                    90, 170, 152, 30, 69, 118, 124, 247, 45, 114, 136, 140, 28, 48, 119, 86, 26, 245, 254, 232, 13, 183,
                    79, 243, 126, 189, 191, 196, 224, 241, 54, 44, 103, 147, 160, 204, 61, 68, 66, 245, 104, 40, 44, 242,
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
