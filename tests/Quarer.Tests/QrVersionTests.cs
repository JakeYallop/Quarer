namespace Quarer.Tests;

public class QrVersionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(23)]
    [InlineData(40)]
    public void GetVersion_ReturnsVersion(int version)
    {
        var qrVersion = QrVersion.GetVersion((byte)version);
        Assert.Equal(version, qrVersion.Version);
    }

    [Fact]
    public void Equality_IEquatable()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version);
        var v2 = QrVersion.GetVersion(version);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equals_op_Equality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version);
        var v2 = QrVersion.GetVersion(version);
        Assert.True(v1 == v2);
    }

    [Fact]
    public void Equals_op_Inequality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version);
        var v2 = QrVersion.GetVersion((byte)((version % 40) + 1));
        Assert.True(v1 != v2);
    }

    [Fact]
    public void Equality_Equals()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version);
        var v2 = (object)QrVersion.GetVersion(version);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equality_GetHashCode()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = QrVersion.GetVersion(version);
        var v2 = QrVersion.GetVersion(version);
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }
}
