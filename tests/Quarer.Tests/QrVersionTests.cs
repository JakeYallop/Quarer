namespace Quarer.Tests;

public class QrVersionTests
{
    [Fact]
    public void EmptyConstructor_VersionEqualsMinVersion()
    {
        var v = new QrVersion();
        Assert.Equal(QrVersion.MinVersion, v.Version);
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
        var v1 = QrVersion.GetVersion(1);
        var v2 = QrVersion.GetVersion(2);
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
