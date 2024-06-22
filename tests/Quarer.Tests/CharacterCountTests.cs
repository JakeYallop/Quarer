namespace Quarer.Tests;
public class CharacterCountTests
{
    [Theory]
    [InlineData(1, ModeIndicator.Numeric, 10)]
    [InlineData(9, ModeIndicator.Numeric, 10)]
    [InlineData(10, ModeIndicator.Numeric, 12)]
    [InlineData(26, ModeIndicator.Numeric, 12)]
    [InlineData(27, ModeIndicator.Numeric, 14)]
    [InlineData(40, ModeIndicator.Numeric, 14)]
    public void GetCharacterCount_ForNumericMode_ReturnsCorrectCount(int version, ModeIndicator mode, ushort expected)
    {
        var result = CharacterCount.GetCharacterCountBitCount((QrVersion)version, mode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, ModeIndicator.Alphanumeric, 9)]
    [InlineData(9, ModeIndicator.Alphanumeric, 9)]
    [InlineData(10, ModeIndicator.Alphanumeric, 11)]
    [InlineData(26, ModeIndicator.Alphanumeric, 11)]
    [InlineData(27, ModeIndicator.Alphanumeric, 13)]
    [InlineData(40, ModeIndicator.Alphanumeric, 13)]
    public void GetCharacterCount_ForAlphanumericMode_ReturnsCorrectCount(int version, ModeIndicator mode, ushort expected)
    {
        var result = CharacterCount.GetCharacterCountBitCount((QrVersion)version, mode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, ModeIndicator.Byte, 8)]
    [InlineData(9, ModeIndicator.Byte, 8)]
    [InlineData(10, ModeIndicator.Byte, 16)]
    [InlineData(26, ModeIndicator.Byte, 16)]
    [InlineData(27, ModeIndicator.Byte, 16)]
    [InlineData(40, ModeIndicator.Byte, 16)]
    public void GetCharacterCount_ForByteMode_ReturnsCorrectCount(int version, ModeIndicator mode, ushort expected)
    {
        var result = CharacterCount.GetCharacterCountBitCount((QrVersion)version, mode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, ModeIndicator.Kanji, 8)]
    [InlineData(9, ModeIndicator.Kanji, 8)]
    [InlineData(10, ModeIndicator.Kanji, 10)]
    [InlineData(26, ModeIndicator.Kanji, 10)]
    [InlineData(27, ModeIndicator.Kanji, 12)]
    [InlineData(40, ModeIndicator.Kanji, 12)]
    public void GetCharacterCount_ForKanjiMode_ReturnsCorrectCount(int version, ModeIndicator mode, ushort expected)
    {
        var result = CharacterCount.GetCharacterCountBitCount((QrVersion)version, mode);
        Assert.Equal(expected, result);
    }
}
