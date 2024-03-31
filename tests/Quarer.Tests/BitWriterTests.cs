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
    [InlineData(128, 7)]
    [InlineData(16, 4)]
    public void WriteBits_ValueLargerThanSpecifiedBits_ShouldThrowArgumentOutOfRangeException(ushort value, int bitsToUse)
    {
        var bitWriter = new BitWriter();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitWriter.WriteBits(value, bitsToUse));
    }

    [Fact]
    public void WriteBits_WritesCorrectValues()
    {
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_0000_00;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 10);
        bitWriter.WriteBits(value3, 10);

        // Get the raw buffer
        ReadOnlySpan<byte> bufferSpan = bitWriter.UnsafeGetRawBuffer();

        //|10000000|00
        //010000|0000
        //0010|000000
        //00| (trailing bits)
        var expectedBitPattern = new byte[] { 0b1000_0000, 0b0001_0000, 0b0000_0010, 0b0000_0000 };
        Assert.Equal(expectedBitPattern, bufferSpan.ToArray());
    }
}
