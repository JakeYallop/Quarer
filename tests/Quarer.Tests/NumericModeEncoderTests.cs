namespace Quarer.Tests;

public sealed class NumericModeEncoderTests
{
    [Fact]
    public void Encode_WithNoRemainder_WritesCorrectValues()
    {
        var buffer = new BitBuffer();
        NumericEncoder.Encode(buffer, "123456");

        //2 triples of 10 bits each, for 20 bits total
        Assert.Equal(20, buffer.Count);

        var bitStream = buffer.GetBitStream();
        AssertExtensions.BitsEqual($"{123:B10}{456:B10}", bitStream);
    }

    [Fact]
    public void Encode_WithOneRemainder_WritesCorrectValues()
    {
        var buffer = new BitBuffer();
        NumericEncoder.Encode(buffer, "1234");

        Assert.Equal(14, buffer.Count);
        AssertExtensions.BitsEqual($"{123:B10}{4:B4}", buffer.GetBitStream());
    }

    [Fact]
    public void Encode_WithTwoRemainders_WritesCorrectValues()
    {
        var buffer = new BitBuffer();
        NumericEncoder.Encode(buffer, "12345");

        Assert.Equal(17, buffer.Count);
        AssertExtensions.BitsEqual($"{123:B10}{45:B7}", buffer.GetBitStream());
    }

    [Fact]
    public void GetBitStreamLength_NoRemainder() => Assert.Equal(20, NumericEncoder.GetBitStreamLength("123456"));
    [Fact]
    public void GetBitStreamLength_OneRemainder() => Assert.Equal(24, NumericEncoder.GetBitStreamLength("1234567"));
    [Fact]
    public void GetBitStreamLength_TwoRemainder() => Assert.Equal(27, NumericEncoder.GetBitStreamLength("12345678"));
}
