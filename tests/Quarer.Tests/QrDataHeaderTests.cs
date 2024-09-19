using System.Globalization;

namespace Quarer.Tests;
public sealed class QrDataHeaderTests
{
    [Fact]
    public void WriteHeader_ThrowsExceptionForUnsupportedMode()
    {
        var bitWriter = new BitWriter();

        foreach (var mode in Enum.GetValues<ModeIndicator>())
        {
            if (mode is not (ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend or ModeIndicator.Terminator))
            {
                QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1, ErrorCorrectionLevel.Q), mode, 0);
            }
            else
            {
                _ = Assert.Throws<NotSupportedException>(() => QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1, ErrorCorrectionLevel.Q), mode, 0));
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
#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    [MemberData(nameof(Data))]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    public void WriteHeader_WritesCorrectData(QrVersion version, ModeIndicator mode, int inputDataCount, string expectedBitString)
    {
        var writer = new BitWriter();
        var bitsForCharacterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        QrHeaderBlock.WriteHeader(writer, version, mode, inputDataCount);

        Assert.Equal(4 + bitsForCharacterCount, writer.BitsWritten);
        AssertExtensions.BitsEqual(expectedBitString, writer.Buffer.AsBitEnumerable());
    }
}
