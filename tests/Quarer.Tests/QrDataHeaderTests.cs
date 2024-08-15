using System.Globalization;

namespace Quarer.Tests;
public sealed class QrDataHeaderTests
{
    [Fact]
    public void WriteHeader_ThrowsExceptionForUnsupportedMode()
    {
        foreach (var mode in Enum.GetValues<ModeIndicator>())
        {
            if (mode is not (ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend or ModeIndicator.Terminator))
            {
                QrHeaderBlock.Create(QrVersion.GetVersion(1, ErrorCorrectionLevel.Q), mode, 0);
            }
            else
            {
                Assert.Throws<NotSupportedException>(() => QrHeaderBlock.Create(QrVersion.GetVersion(1, ErrorCorrectionLevel.Q), mode, 0));
            }
        }
    }

    public static TheoryData<QrVersion, ModeIndicator, int, string> Data()
    {
        var theoryData = new TheoryData<QrVersion, ModeIndicator, int, string>();
        foreach (var mode in (ReadOnlySpan<ModeIndicator>)[ModeIndicator.Numeric, ModeIndicator.Alphanumeric, ModeIndicator.Byte, ModeIndicator.Byte])
        {
            foreach (var version in (ReadOnlySpan<QrVersion>)[
                QrVersion.GetVersion(1, ErrorCorrectionLevel.M),
                QrVersion.GetVersion(10, ErrorCorrectionLevel.Q),
                QrVersion.GetVersion(29, ErrorCorrectionLevel.H)])
            {
                var inputDataLength = 0b100100;
                var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
                var format = $"b{characterCount}";
                var finalBits = string.Create(CultureInfo.InvariantCulture, $"{(int)mode:b4}{inputDataLength.ToString(format, CultureInfo.InvariantCulture)}");
                theoryData.Add(version, mode, inputDataLength, finalBits);
            }
        }
        return theoryData;
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteHeader_WritesCorrectData(QrVersion version, ModeIndicator mode, int inputDataCount, string expectedBitString)
    {
        var buffer = new BitBuffer();
        var header = QrHeaderBlock.Create(version, mode, inputDataCount);
        var bitsForCharacterCount = CharacterCount.GetCharacterCountBitCount(version, mode);

        header.WriteHeader(buffer);

        Assert.Equal(4 + bitsForCharacterCount, buffer.Count);
        AssertExtensions.BitsEqual(expectedBitString, buffer.AsBitEnumerable());
    }
}
