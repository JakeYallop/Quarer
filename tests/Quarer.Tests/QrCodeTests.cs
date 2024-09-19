using System.Diagnostics;

namespace Quarer.Tests;

public partial class QrCodeTests
{
    [Fact]
    public void Create_DataTooLarge_ThrowsQrCodeException()
    {
        var data = new string('A', 4297); //4296 alphanumeric characters for 40L QR Code
        Assert.Throws<QrCodeException>(() => QrCode.Create(data));
    }

    [Fact]
    public void Create_DataTooLarge_ThrowsQrCodeException2()
    {
        var data = new string('1', 3058); //3058 numeric characters for 40H QR Code
        Assert.Throws<QrCodeException>(() => QrCode.Create(data, ErrorCorrectionLevel.H));
    }

    [Fact]
    public void Create_DataTooLarge_ThrowsQrCodeException3()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var mode = ModeIndicator.Alphanumeric;
        var data = new string('A', 21); // max 20 characters for 1M alphanumeric QR Code
        Assert.Throws<QrCodeException>(() => QrCode.Create(data, version));
    }

    [Fact]
    public void TryCreate_DataTooLarge_ReturnsDataTooLargeResult()
    {
        var data = new string('A', 4297); //4296 alphanumeric characters for 40L QR Code
        var result = QrCode.TryCreate(data);
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void TryCreate_DataTooLarge_ReturnsDataTooLargeResult2()
    {
        var data = new string('1', 3058); //3058 numeric characters for 40H QR Code
        var result = QrCode.TryCreate(data, ErrorCorrectionLevel.H);
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void TryCreate_DataTooLarge_ReturnsDataTooLargeResult3()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var mode = ModeIndicator.Alphanumeric;
        var data = new string('A', 21); // max 20 characters for 1M alphanumeric QR Code
        var result = QrCode.TryCreate(data, version);
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Equals_Equal()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern = MaskPattern.PatternZero_Checkerboard;
        var data = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version, maskPattern, data);
        var qrCode2 = new QrCode(version, maskPattern, data);

        var result = qrCode1.Equals(qrCode2);
        Assert.True(result);
    }

    [Fact]
    public void Equals_NotEqual()
    {
        var version1 = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern1 = MaskPattern.PatternZero_Checkerboard;
        var data1 = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version1, maskPattern1, data1);

        var version2 = QrVersion.GetVersion(2, ErrorCorrectionLevel.M);
        var maskPattern2 = MaskPattern.PatternOne_HorizontalLines;
        var data2 = new BitMatrix(25, 25);
        var qrCode2 = new QrCode(version2, maskPattern2, data2);

        var result = qrCode1.Equals(qrCode2);
        Assert.False(result);
    }

    [Fact]
    public void Op_Equality_ReturnsExpectedResult()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern = MaskPattern.PatternZero_Checkerboard;
        var data = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version, maskPattern, data);
        var qrCode2 = new QrCode(version, maskPattern, data);

        var result = qrCode1 == qrCode2;
        Assert.True(result);
    }

    [Fact]
    public void Op_Inequality_ReturnsExpectedResult()
    {
        var version1 = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern1 = MaskPattern.PatternZero_Checkerboard;
        var data1 = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version1, maskPattern1, data1);

        var version2 = QrVersion.GetVersion(2, ErrorCorrectionLevel.M);
        var maskPattern2 = MaskPattern.PatternOne_HorizontalLines;
        var data2 = new BitMatrix(25, 25);
        var qrCode2 = new QrCode(version2, maskPattern2, data2);

        var result = qrCode1 == qrCode2;
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Equal()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern = MaskPattern.PatternZero_Checkerboard;
        var data = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version, maskPattern, data);
        var qrCode2 = new QrCode(version, maskPattern, data);

        var hashCode1 = qrCode1.GetHashCode();
        var hashCode2 = qrCode2.GetHashCode();

        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_NotEqual()
    {
        // Arrange
        var version1 = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var maskPattern1 = MaskPattern.PatternZero_Checkerboard;
        var data1 = new BitMatrix(21, 21);
        var qrCode1 = new QrCode(version1, maskPattern1, data1);

        var version2 = QrVersion.GetVersion(2, ErrorCorrectionLevel.M);
        var maskPattern2 = MaskPattern.PatternOne_HorizontalLines;
        var data2 = new BitMatrix(25, 25);
        var qrCode2 = new QrCode(version2, maskPattern2, data2);

        var hashCode1 = qrCode1.GetHashCode();
        var hashCode2 = qrCode2.GetHashCode();

        Assert.NotEqual(hashCode1, hashCode2);
    }

    private static int CalculateApproximateCharacterCapacityWithinRange(QrVersion version, ModeIndicator mode)
    {
#pragma warning disable IDE0072 // Add missing cases
        const int ModeIndicatorBits = 4;
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var capacity = (version.DataCodewordsCapacity << 3) - ModeIndicatorBits - characterCount;

        return mode switch
        {
            // 10 bits per 3 characters, 3 / 10, integer division allows us to ignore the remainder cases (4 or 7 bits left over)
            ModeIndicator.Numeric => (int)(capacity * 0.3),
            // 11 bits per 2 characters, 2.0 / 11.0, integer division allows us to ignore the remainder case (1 character in 6 bits)
            ModeIndicator.Alphanumeric => (int)(capacity * 0.18181818181818182d),
            // 8 bits per character
            ModeIndicator.Byte => capacity >> 3,
            // 13 bits per character, 1.0 / 13.0,
            ModeIndicator.Kanji => (int)(capacity * 0.07692307692307693),
            _ => throw new UnreachableException()
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
