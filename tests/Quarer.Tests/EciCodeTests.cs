namespace Quarer.Tests;
public class EciCodeTests
{
    [Fact]
    public void Create()
    {
        var eciCode = new EciCode(26);
        Assert.Equal((byte)26, eciCode.Value);
    }

    [Fact]
    public void Create_ThrowsArgumentOutOfRangeException() => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new EciCode(128));

    [Theory]
    [InlineData(null, true)]
    [InlineData((byte)0, true)]
    [InlineData((byte)1, false)]
    [InlineData((byte)3, false)]
    [InlineData((byte)127, false)]
    public void IsEmpty_ReturnsExpectedResult(byte? value, bool expected)
    {
        var eciCode = new EciCode(value);
        Assert.Equal(expected, eciCode.IsEmpty());
    }

    [Fact]
    public void GetDataSegmentLength_ReturnsExpectedCount()
    {
        var eciCode = new EciCode(26);
        Assert.Equal(12, eciCode.GetDataSegmentLength());
    }
}
