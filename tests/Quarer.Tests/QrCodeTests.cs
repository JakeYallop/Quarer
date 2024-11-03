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
        Assert.Throws<QrCodeException>(() => QrCode.Create(data, new(ErrorCorrectionLevel.H)));
    }

    [Fact]
    public void Create_DataTooLarge_ThrowsQrCodeException3()
    {
        var version = QrVersion.GetVersion(1);
        var data = new string('A', 21); // max 20 characters for 1M alphanumeric QR Code
        Assert.Throws<QrCodeException>(() => QrCode.Create(data, new(version, ErrorCorrectionLevel.M)));
    }

    [Fact]
    public void Create_Char_EciCodeProvided_ThrowsQrCodeException()
    {
        var eciCode = new EciCode(1);
        Assert.Throws<QrCodeException>(() => QrCode.Create("AAA", new() { EciCode = eciCode }));
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
        var result = QrCode.TryCreate(data, new(ErrorCorrectionLevel.H));
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void TryCreate_DataTooLarge_ReturnsDataTooLargeResult3()
    {
        var version = QrVersion.GetVersion(1);
        var data = new string('A', 21); // max 20 characters for 1M alphanumeric QR Code
        var result = QrCode.TryCreate(data, new(version, ErrorCorrectionLevel.M));
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void TryCreate_Byte_EciCodeProvided_ReturnsEciCodeNotAllowedForCharacterDataResult()
    {
        var eciCode = new EciCode(1);
        var result = QrCode.TryCreate("AAA", new() { EciCode = eciCode });
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.EciCodeNotAllowedForCharacterData, result.Reason);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Equals_Equal()
    {
        var version = QrVersion.GetVersion(1);
        var data = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version, data, ErrorCorrectionLevel.M);
        var qrCode2 = new QrCode(version, data, ErrorCorrectionLevel.M);

        var result = qrCode1.Equals(qrCode2);
        Assert.True(result);
    }

    [Fact]
    public void Equals_Version_NotEqual()
    {
        var version1 = QrVersion.GetVersion(1);
        var data1 = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version1, data1, ErrorCorrectionLevel.L);

        var version2 = QrVersion.GetVersion(2);
        var qrCode2 = new QrCode(version2, data1, ErrorCorrectionLevel.L);

        var result = qrCode1.Equals(qrCode2);
        Assert.False(result);
    }

    [Fact]
    public void Equals_Data_NotEqual()
    {
        var version1 = QrVersion.GetVersion(1);
        var data1 = new ByteMatrix(21, 21);
        var data2 = new ByteMatrix(10, 10);
        var qrCode1 = new QrCode(version1, data1, ErrorCorrectionLevel.L);
        var qrCode2 = new QrCode(version1, data2, ErrorCorrectionLevel.L);

        var result = qrCode1.Equals(qrCode2);
        Assert.False(result);
    }

    [Fact]
    public void Equals_ErrorCorrectionLevel_NotEqual()
    {
        var version1 = QrVersion.GetVersion(1);
        var data1 = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version1, data1, ErrorCorrectionLevel.L);
        var qrCode2 = new QrCode(version1, data1, ErrorCorrectionLevel.M);

        var result = qrCode1.Equals(qrCode2);
        Assert.False(result);
    }

    [Fact]
    public void Op_Equality_ReturnsExpectedResult()
    {
        var version = QrVersion.GetVersion(1);
        var data = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version, data, ErrorCorrectionLevel.L);
        var qrCode2 = new QrCode(version, data, ErrorCorrectionLevel.L);

        var result = qrCode1 == qrCode2;
        Assert.True(result);
    }

    [Fact]
    public void Op_Inequality_ReturnsExpectedResult()
    {
        var version1 = QrVersion.GetVersion(1);
        var data1 = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version1, data1, ErrorCorrectionLevel.L);

        var version2 = QrVersion.GetVersion(2);
        var data2 = new ByteMatrix(25, 25);
        var qrCode2 = new QrCode(version2, data2, ErrorCorrectionLevel.M);

        var result = qrCode1 != qrCode2;
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_Equal()
    {
        var version = QrVersion.GetVersion(1);
        var data = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version, data, ErrorCorrectionLevel.L);
        var qrCode2 = new QrCode(version, data, ErrorCorrectionLevel.L);

        var hashCode1 = qrCode1.GetHashCode();
        var hashCode2 = qrCode2.GetHashCode();

        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_NotEqual()
    {
        // Arrange
        var version1 = QrVersion.GetVersion(1);
        var data1 = new ByteMatrix(21, 21);
        var qrCode1 = new QrCode(version1, data1, ErrorCorrectionLevel.L);

        var version2 = QrVersion.GetVersion(2);
        var data2 = new ByteMatrix(25, 25);
        var qrCode2 = new QrCode(version2, data2, ErrorCorrectionLevel.M);

        var hashCode1 = qrCode1.GetHashCode();
        var hashCode2 = qrCode2.GetHashCode();

        Assert.NotEqual(hashCode1, hashCode2);
    }
}
