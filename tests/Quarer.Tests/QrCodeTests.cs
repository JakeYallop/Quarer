namespace Quarer.Tests;

public class QrCodeTests
{
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
}
