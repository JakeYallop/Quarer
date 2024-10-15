using System.Diagnostics;

namespace Quarer.Tests;

public class QrVersionQrErrorCorrectionBlocksTests
{
    private static ErrorCorrectionLevel RandomErrorCorrectionLevel()
    {
        var errorCorrectionLevel = (ErrorCorrectionLevel)Random.Shared.Next(0, 4);
        Debug.Assert(Enum.IsDefined(errorCorrectionLevel), "ErrorCorrectionLevel is invalid.");
        return errorCorrectionLevel;
    }

    [Fact]
    public void Equality_IEquatable()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = RandomErrorCorrectionLevel();
        var v1 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equals_op_Equality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = RandomErrorCorrectionLevel();
        var v1 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        Assert.True(v1 == v2);
    }

    [Fact]
    public void Equals_op_Inequality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(ErrorCorrectionLevel.L);
        var v2 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(ErrorCorrectionLevel.M);
        Assert.True(v1 != v2);
    }

    [Fact]
    public void Equality_Equals()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = RandomErrorCorrectionLevel();
        var v1 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        var v2 = (object)QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equality_GetHashCode()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var errorCorrectionLevel = RandomErrorCorrectionLevel();
        var v1 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        var v2 = QrVersion.GetVersion(version).GetErrorCorrectionBlocks(errorCorrectionLevel);
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }
}
