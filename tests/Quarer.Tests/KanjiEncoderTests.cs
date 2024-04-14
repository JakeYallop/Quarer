namespace Quarer.Tests;
public sealed class KanjiEncoderTests
{
    [Theory]
    [InlineData((char)0x8140, (char)0x9FFC, "0 0000 0000 0000 1 0111 0011 1100")] //first range
    [InlineData((char)0xE040, (char)0xEBBF, "1 0111 0100 0000 1 1111 1111 1111")] //second range
    [InlineData((char)0x935F, (char)0xE4AA, "0 1101 1001 1111 1 1010 1010 1010")] //test case from the spec
    public void Encode_ValidKanji_EncodesCorrectly(char first, char second, string expected)
    {
        var bitWriter = new BitWriter();
        KanjiEncoder.Encode(bitWriter, [first, second]);

        var bitStream = bitWriter.GetBitStream();
        AssertExtensions.BitsEqual(expected, bitStream);
    }

    [Fact]
    public void Encode_InvalidKanji_ThrowsArgumentException()
    {
        var bitWriter = new BitWriter();
        var data = new[] { (char)0x7FFF, (char)0xFFFF };

        Assert.Throws<ArgumentException>(() => KanjiEncoder.Encode(bitWriter, data));
    }

    [Fact]
    public void Encode_MixedKanji_EncodesCorrectly()
    {
        var bitWriter = new BitWriter();
        var data = new[] { (char)0x8140, (char)0xE040, (char)0x9FFC, (char)0xEBBF };

        KanjiEncoder.Encode(bitWriter, data);

        var bitStream = bitWriter.GetBitStream();
        AssertExtensions.BitsEqual("""
            0 0000 0000 0000
            1 0111 0100 0000
            1 0111 0011 1100
            1 1111 1111 1111
            """, bitStream);
    }

    [Theory]
    [InlineData("\u9FFC", false )]
    [InlineData("\u9FFCa", true )]
    [InlineData("\u8140", false )]
    [InlineData("\u8140!", true )]
    [InlineData("\uE040", false )]
    [InlineData("\uE040 ", true)]
    [InlineData("\uEBBF", false )]
    [InlineData("\uEBBFb", true )]
    public void ContainsAnyExceptKanji_ReturnsExpectedResult(string s, bool expected)
        => Assert.Equal(expected, KanjiEncoder.ContainsAnyExceptKanji(s));

    [Fact]
    public void GetBitStreamLength_ReturnsExpectedCount()
        => Assert.Equal(39, KanjiEncoder.GetBitStreamLength("\u9FFC\u935F\uE4AA"));
}
