using System.Text;

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
        KanjiEncoder.Encode(bitWriter, ToBytes([first, second]));

        var bits = bitWriter.Buffer.AsBitEnumerable();
        AssertExtensions.BitsEqual(expected, bits);
    }

    [Fact]
    public void Encode_InvalidKanji_ThrowsArgumentException()
    {
        var bitWriter = new BitWriter();
        var data = new[] { (char)0x7FFF, (char)0xFFFF };

        Assert.Throws<ArgumentException>(() => KanjiEncoder.Encode(bitWriter, ToBytes(data)));
    }

    [Fact]
    public void Encode_MixedKanji_EncodesCorrectly()
    {
        var bitWriter = new BitWriter();
        var data = new[] { (char)0x8140, (char)0xE040, (char)0x9FFC, (char)0xEBBF };

        KanjiEncoder.Encode(bitWriter, ToBytes(data));

        var bits = bitWriter.Buffer.AsBitEnumerable();
        AssertExtensions.BitsEqual("""
            0 0000 0000 0000
            1 0111 0100 0000
            1 0111 0011 1100
            1 1111 1111 1111
            """, bits);
    }

    static KanjiEncoderTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Theory]
    [InlineData("abcd", true)]
    [InlineData("\u9FFC", false)]
    [InlineData("\u9FFCa", true)]
    [InlineData("\u8140", false)]
    [InlineData("\u8140!", true)]
    [InlineData("\uE040", false)]
    [InlineData("\uE040 ", true)]
    [InlineData("\uEBBF", false)]
    [InlineData("\uEBBFb", true)]
    [InlineData("\u935F\uE4AA", false)]
    [InlineData("\u935F\uE4AAA", true)]
    [InlineData("\u935F\uE4AA\uE4AA\uE4AA\uE4AA\uE4AA", false)]
    public void ContainsAnyExceptKanji_ReturnsExpectedResult(string s, bool expected)
        => Assert.Equal(expected, KanjiEncoder.ContainsAnyExceptKanji(Encoding.BigEndianUnicode.GetBytes(s)));

    [Fact]
    public void GetBitStreamLength_ReturnsExpectedCount()
        => Assert.Equal(39, KanjiEncoder.GetBitStreamLength(Encoding.BigEndianUnicode.GetBytes("\u9FFC\u935F\uE4AA")));

    [Theory]
    [InlineData(10, 130)]
    [InlineData(11, 143)]
    [InlineData(12, 156)]
    public void GetBitStreamLength_ReturnsExpectedCount2(int kanjiCharacters, int expectedLength)
    {
        var bytes = new byte[kanjiCharacters * 2]; //2 bytes per character
        Assert.Equal(expectedLength, KanjiEncoder.GetBitStreamLength(bytes));
    }

    private static byte[] ToBytes(char[] data)
    {
        var bytes = new byte[data.Length * 2];
        for (var i = 0; i < data.Length; i++)
        {
            var c = data[i];
            var decomposedBytes = ToBytes(c);
            bytes[i * 2] = decomposedBytes[0];
            bytes[(i * 2) + 1] = decomposedBytes[1];
        }
        return bytes;
    }
    private static byte[] ToBytes(char c) => [(byte)(c >> 8), (byte)(c & 0xFF)];
}
