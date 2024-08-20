namespace Quarer.Tests;

public sealed class AlphanumericEncoderTests
{
    [Fact]
    public void Encode_WithEvenInput_WritesCorrectValues()
    {
        var writer = new BitWriter();
        AlphanumericEncoder.Encode(writer, "ABC0:T");

        //3 pairs of 11 bits each
        Assert.Equal(33, writer.BitsWritten);

        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{(44 * 45) + 29:B11}", writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void Encode_WithOddInput_WritesCorrectValues()
    {
        var writer = new BitWriter();
        AlphanumericEncoder.Encode(writer, "ABC0:");

        //2 pairs of 11 bits each, 1 extra of 6 bits.
        Assert.Equal(28, writer.BitsWritten);

        //A = 10
        //B = 11
        //C = 12
        //: = 44
        AssertExtensions.BitsEqual($"{(10 * 45) + 11:B11}{(12 * 45) + 0:B11}{44:B6}", writer.Buffer.AsBitEnumerable());
    }

    [Fact]
    public void GetBitStreamLength_NoRemainder() => Assert.Equal(33, AlphanumericEncoder.GetBitStreamLength("ABCDEF"));
    [Fact]
    public void GetBitStreamLength_OneRemainder() => Assert.Equal(39, AlphanumericEncoder.GetBitStreamLength("ABCDEFG"));
}
