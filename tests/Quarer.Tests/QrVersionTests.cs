namespace Quarer.Tests;

public class QrVersionTests
{
    [Fact]
    public void Equality_IEquatable()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        var v1 = QrVersion.GetVersion(version, errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version, errorCorrectionLevel);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equals_op_Equality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        var v1 = QrVersion.GetVersion(version, errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version, errorCorrectionLevel);
    }

    [Fact]
    public void Equals_op_Inequality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        var v1 = QrVersion.GetVersion(version, ErrorCorrectionLevel.M);
        var v2 = QrVersion.GetVersion(version, ErrorCorrectionLevel.H);
        var v3 = QrVersion.GetVersion(1, errorCorrectionLevel);
        var v4 = QrVersion.GetVersion(2, errorCorrectionLevel);
        Assert.True(v1 != v2);
        Assert.True(v3 != v4);
    }

    [Fact]
    public void Equality_Equals()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        var v1 = QrVersion.GetVersion(version, errorCorrectionLevel);
        var v2 = (object)QrVersion.GetVersion(version, errorCorrectionLevel);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equality_GetHashCode()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        var v1 = QrVersion.GetVersion(version, errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version, errorCorrectionLevel);
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }
}
