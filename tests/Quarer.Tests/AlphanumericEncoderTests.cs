namespace Quarer.Tests;

public sealed class AlphanumericEncoderTests
{
    [Fact]
    public void Encode_WithEvenInput_WritesCorrectValues()
    {
        var buffer = new BitBuffer();
        AlphanumericEncoder.Encode(buffer, "ABC0:T");

        //3 pairs of 11 bits each
        Assert.Equal(33, buffer.Count);

        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{(44 * 45) + 29:B11}", buffer.AsBitEnumerable());
    }

    [Fact]
    public void Encode_WithOddInput_WritesCorrectValues()
    {
        var buffer = new BitBuffer();
        AlphanumericEncoder.Encode(buffer, "ABC0:");

        //2 pairs of 11 bits each, 1 extra of 6 bits.
        Assert.Equal(28, buffer.Count);

        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{44:B6}", buffer.AsBitEnumerable());
    }

    [Fact]
    public void GetBitStreamLength_NoRemainder() => Assert.Equal(33, AlphanumericEncoder.GetBitStreamLength("ABCDEF"));
    [Fact]
    public void GetBitStreamLength_OneRemainder() => Assert.Equal(39, AlphanumericEncoder.GetBitStreamLength("ABCDEFG"));
}
