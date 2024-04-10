namespace Quarer.Tests;

public sealed class AlphanumericEncoderTests
{
    [Fact]
    public void Encode_WithEvenInput_WritesCorrectValues()
    {
        var writer = new BitWriter();
        AlphanumericEncoder.Encode(writer, [(byte)'A', (byte)'B', (byte)'C', (byte)'0', (byte)':', (byte)'T']);

        //3 pairs of 11 bits each
        Assert.Equal(33, writer.Count);

        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{(44 * 45) + 29:B11}", writer.GetBitStream());
    }

    [Fact]
    public void Encode_WithOddInput_WritesCorrectValues()
    {
        var writer = new BitWriter();
        AlphanumericEncoder.Encode(writer, [(byte)'A', (byte)'B', (byte)'C', (byte)'0', (byte)':']);

        //2 pairs of 11 bits each, 1 extra of 6 bits.
        Assert.Equal(28, writer.Count);

        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{44:B6}", writer.GetBitStream());
    }
}
