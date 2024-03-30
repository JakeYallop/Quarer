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
        var v1 = new QrVersion(version);
        var v2 = new QrVersion(version);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equals_op_Equality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = new QrVersion(version);
        var v2 = new QrVersion(version);
        Assert.True(v1 == v2);
    }

    [Fact]
    public void Equals_op_Inequality()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = new QrVersion(version);
        var v2 = new QrVersion(version);
        Assert.True(v1 != v2);
    }

    [Fact]
    public void Equality_Equals()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = new QrVersion(version);
        var v2 = (object)new QrVersion(version);
        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equality_GetHashCode()
    {
        var version = (byte)Random.Shared.Next(1, 41);
        var v1 = new QrVersion(version);
        var v2 = new QrVersion(version);
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }
}
