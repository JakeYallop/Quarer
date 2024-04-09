using System.Globalization;

namespace Quarer.Tests;
public sealed class QrHeaderTests
{
    [Fact]
    public void WriteHeader_ThrowsExceptionForUnsupportedMode()
    {
        foreach (var mode in Enum.GetValues<ModeIndicator>())
        {
            if (mode is not (ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend or ModeIndicator.Terminator))
            {
                QrDataHeader.Create((QrVersion)1, mode, 0);
            }
            else
            {
                Assert.Throws<NotSupportedException>(() => QrDataHeader.Create((QrVersion)1, mode, 0));
            }
        }
    }

    public static TheoryData<QrVersion, ModeIndicator, int, string> Data()
    {
        var theoryData = new TheoryData<QrVersion, ModeIndicator, int, string>();
        foreach (var mode in (ReadOnlySpan<ModeIndicator>)[ModeIndicator.Numeric, ModeIndicator.Alphanumeric, ModeIndicator.Byte, ModeIndicator.Byte])
        {
            foreach (var version in (ReadOnlySpan<QrVersion>)[(QrVersion)1, (QrVersion)10, (QrVersion)29])
            {
                var inputDataLength = 0b100100;
                var characterCount = CharacterCountIndicator.GetCharacterCountBitCount(in version, in mode);
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
        var writer = new BitWriter();
        var header = QrDataHeader.Create(version, mode, inputDataCount);
        var bitsForCharacterCount = CharacterCountIndicator.GetCharacterCountBitCount(version, mode);

        header.WriteHeader(writer);

        Assert.Equal(4 + bitsForCharacterCount, writer.Count);
        AssertExtensions.BitsEqual(expectedBitString, writer.GetBitStream());
    }
}
