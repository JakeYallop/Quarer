using System.Text;

namespace Quarer.Tests;
public class ModeIndicatorExtensionsTests
{
    [Theory]
    [InlineData("1234567890", ModeIndicator.Numeric, 34)]
    [InlineData("HELLO123", ModeIndicator.Alphanumeric, 44)]
    [InlineData("HELLO WORLD", ModeIndicator.Alphanumeric, 61)]
    [InlineData("Hello, World!", ModeIndicator.Byte, 104)]
    public void GetBitStreamLength_ValidData_ReturnsCorrectLength(string data, ModeIndicator mode, int expectedLength)
    {
        var length = mode.GetBitStreamLength(Encoding.UTF8.GetBytes(data));
        Assert.Equal(expectedLength, length);
    }

    static ModeIndicatorExtensionsTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public void GetBitStreamLength_KanjiData_ReturnsCorrectLength()
    {
        var s = Encoding.GetEncoding("shift_jis").GetBytes("ハローワールド");
        Assert.Equal(ModeIndicator.Kanji, QrDataEncoder.DeriveMode(s));
        var length = ModeIndicator.Kanji.GetBitStreamLength(s);
        Assert.Equal(91, length);
    }

    [Theory]
    [InlineData("1234567890", ModeIndicator.Numeric, 10)]
    [InlineData("HELLO123", ModeIndicator.Alphanumeric, 8)]
    [InlineData("HELLO WORLD", ModeIndicator.Alphanumeric, 11)]
    [InlineData("Hello, World!", ModeIndicator.Byte, 13)]
    public void GetDataCharacterLength_ValidData_ReturnsCorrectLength(string data, ModeIndicator mode, int expectedLength)
    {
        var length = mode.GetDataCharacterLength(Encoding.UTF8.GetBytes(data));
        Assert.Equal(expectedLength, length);
    }

    [Fact]
    public void GetDataCharacterLength_KanjiData_ReturnsCorrectLength()
    {
        var s = Encoding.GetEncoding("shift_jis").GetBytes("ハローワールド");
        Assert.Equal(ModeIndicator.Kanji, QrDataEncoder.DeriveMode(s));
        var length = ModeIndicator.Kanji.GetDataCharacterLength(s);
        Assert.Equal(7, length);
    }
}
