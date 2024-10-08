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
                QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1), mode, inputDataLength: 0, EciCode.Empty);
            }
            else
            {
                _ = Assert.Throws<NotSupportedException>(() => QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1), mode, inputDataLength: 0, EciCode.Empty));
            }
        }
    }

    public static TheoryData<QrVersion, ModeIndicator, int, string> Data()
    {
        var theoryData = new TheoryData<QrVersion, ModeIndicator, int, string>();
        foreach (var mode in (ReadOnlySpan<ModeIndicator>)[ModeIndicator.Numeric, ModeIndicator.Alphanumeric, ModeIndicator.Byte, ModeIndicator.Byte])
        {
            foreach (var version in (ReadOnlySpan<QrVersion>)[
                QrVersion.GetVersion(1),
                QrVersion.GetVersion(10),
                QrVersion.GetVersion(29)])
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
        var writer = new BitWriter();
        var bitsForCharacterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        QrHeaderBlock.WriteHeader(writer, version, mode, inputDataCount, EciCode.Empty);

        Assert.Equal(4 + bitsForCharacterCount, writer.BitsWritten);
        AssertExtensions.BitsEqual(expectedBitString, writer.Buffer.AsBitEnumerable());
    }

    public static TheoryData<QrVersion, ModeIndicator, int, byte?, string> DataWithEciCode()
    {
        var theoryData = new TheoryData<QrVersion, ModeIndicator, int, byte?, string>();
        foreach (var version in (ReadOnlySpan<QrVersion>)[
                QrVersion.GetVersion(10),
                QrVersion.GetVersion(40)])
        {
            var mode = ModeIndicator.Byte;
            var eciCode = (byte?)Random.Shared.Next(3, 128);
            var inputDataLength = 0b100100;
            var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
            var format = $"b{characterCount}";
            var finalBits = string.Create(CultureInfo.InvariantCulture, $"{(int)ModeIndicator.Eci:B4}{eciCode.Value:B8}{(int)mode:b4}{inputDataLength.ToString(format, CultureInfo.InvariantCulture)}");
            theoryData.Add(version, mode, inputDataLength, eciCode, finalBits);
        }
        return theoryData;
    }
    [Theory]
    [MemberData(nameof(DataWithEciCode))]
    public void WriteHeader_WithEciCode_WritesCorrectData(QrVersion version, ModeIndicator mode, int inputDataCount, byte? eciCode, string expectedBitString)
    {
        var writer = new BitWriter();
        var bitsForCharacterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        QrHeaderBlock.WriteHeader(writer, version, mode, inputDataCount, new EciCode(eciCode));

        //eci mode + eci code + byte mode
        Assert.Equal(4 + 8 + 4 + bitsForCharacterCount, writer.BitsWritten);
        AssertExtensions.BitsEqual(expectedBitString, writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void EciCodeGreaterThan127_ThrowsArgumentOutOfRangeException()
    {
        var bitWriter = new BitWriter();
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1), ModeIndicator.Byte, inputDataLength: 0, new EciCode(128)));
    }

    [Fact]
    public void EciCodeNotNullAndModeNotByte_ThrowsArgumentException()
    {
        var bitWriter = new BitWriter();
        _ = Assert.Throws<ArgumentException>(() => QrHeaderBlock.WriteHeader(bitWriter, QrVersion.GetVersion(1), ModeIndicator.Numeric, inputDataLength: 0, new EciCode(1)));
    }
}
