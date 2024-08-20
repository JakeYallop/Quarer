namespace Quarer.Tests;

public class BitWriterTests
{
    [Fact]
    public void WriteBitsBigEndian_ReturnsExpectedResult()
    {
        var value = 0b0110_0110_11;
        var bits = 10;

        var writer = new BitWriter();
        writer.WriteBitsBigEndian(value, bits);

        Assert.Equal(bits, writer.BitsWritten);
        Assert.Equal(2, writer.BytesWritten);

        var buffer = new BitBuffer();
        buffer.WriteBitsBigEndian(0, value, bits);
        Assert.Equal(buffer.AsBitEnumerable(), writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void WriteBitsBigEndian_MultipleWrites_IncrementsBitCountAndBytesWritten()
    {
        var value = 0b0110_0110_11;
        var bits = 10;
        var writer = new BitWriter();
        writer.WriteBitsBigEndian(value, bits);
        writer.WriteBitsBigEndian(value, bits);
        Assert.Equal(20, writer.BitsWritten);
        Assert.Equal(3, writer.BytesWritten);
        AssertExtensions.BitsEqual($"{value:B10}{value:B10}", writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void WriteBitsBigEndian_WithStartPosition_WritesCorrectBits()
    {
        var value = 0b0110_0110_11;
        var bits = 10;
        var writer = new BitWriter(new BitBuffer(), startPosition: 7);
        writer.WriteBitsBigEndian(value, bits);
        Assert.Equal(10, writer.BitsWritten);
        Assert.Equal(2, writer.BytesWritten);
        Assert.Equal(17, writer.Buffer.Count);
        Assert.Equal(3, writer.Buffer.ByteCount);
        AssertExtensions.BitsEqual($"0000000{value:B10}", writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void WriteBitsBigEndian_WithStartPosition_MultipleValues_WritesCorrectBits()
    {
        var value = 0b0110_0110_11;
        var bits = 10;
        var writer = new BitWriter(new BitBuffer(), startPosition: 7);
        writer.WriteBitsBigEndian(value, bits);
        writer.WriteBitsBigEndian(value, bits);
        Assert.Equal(20, writer.BitsWritten);
        Assert.Equal(3, writer.BytesWritten);
        Assert.Equal(27, writer.Buffer.Count);
        Assert.Equal(4, writer.Buffer.ByteCount);
        AssertExtensions.BitsEqual($"0000000{value:B10}{value:B10}", writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void Ctor_NullBuffer_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(static () => new BitWriter(null!));

    [Fact]
    public void Ctor_PositionNegative_ThrowsArgumentOutOfRangeException() => Assert.Throws<ArgumentOutOfRangeException>(static () => new BitWriter(new(), -1));
}
