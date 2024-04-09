namespace Quarer.Tests;

public sealed partial class NumericModeEncoderTests
{
    [Fact]
    public void Encode_WithNoRemainder_WritesCorrectValues()
    {
        var writer = new BitWriter();
        NumericEncoder.Encode(writer, [1, 2, 3, 4, 5, 6]);

        //2 triples of 10 bits each, for 20 bits total
        Assert.Equal(20, writer.Count);

        var bitStream = writer.GetBitStream();
        AssertExtensions.BitsEqual($"{123:B10}{456:B10}", bitStream);
    }

    [Fact]
    public void Encode_WithOneRemainder_WritesCorrectValues()
    {
        var writer = new BitWriter();
        NumericEncoder.Encode(writer, [1, 2, 3, 4]);

        Assert.Equal(14, writer.Count);
        AssertExtensions.BitsEqual($"{123:B10}{4:B4}", writer.GetBitStream());
    }

    [Fact]
    public void Encode_WithTwoRemainders_WritesCorrectValues()
    {
        var writer = new BitWriter();
        NumericEncoder.Encode(writer, [1, 2, 3, 4, 5]);

        Assert.Equal(17, writer.Count);
        AssertExtensions.BitsEqual($"{123:B10}{45:B7}", writer.GetBitStream());
    }
}
