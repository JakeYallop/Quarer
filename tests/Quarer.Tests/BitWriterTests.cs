using System.Diagnostics;

namespace Quarer.Tests;

public class BitWriterTests
{
    [Fact]
    public void WriteBits_ValidValue_IsSuccessful()
    {
        var bitWriter = new BitWriter();
        ushort validValue = 1023;
        bitWriter.WriteBits(validValue, 10);
    }

    [Theory]
    [InlineData(1024, 10)]
    [InlineData(256, 8)]
    [InlineData(16, 4)]
    public void WriteBits_ValueLargerThanSpecifiedBits_ShouldThrowArgumentOutOfRangeException(ushort value, int bitCount)
    {
        var bitWriter = new BitWriter();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitWriter.WriteBits(value, bitCount));
    }

    [Fact]
    public void WriteBits_WritesCorrectValues()
    {
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_0000_01;
        ushort value4 = 0b0010_0101_01;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 10);
        bitWriter.WriteBits(value3, 10);
        bitWriter.WriteBits(value4, 10);

        var bitStream = bitWriter.GetBitStream();

        AssertExtensions.BitsEqual($"{value1:B10}{value2:B10}{value3:B10}{value4:B10}", bitStream);
    }

    [Fact]
    public void WriteBits_HasCorrectBitCountAfterAddingMultipleValues()
    {
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_0000_00;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 10);
        bitWriter.WriteBits(value3, 10);

        Assert.Equal(30, bitWriter.Count);
    }
}
