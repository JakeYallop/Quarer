namespace Quarer.Tests;

public class Latin1ValidatorTests
{
    [Fact]
    public void ContainsNonLatin1Characters_ValidChararacters_ReturnsFalse()
    {
        var chars = GenerateCharsInRange(0, 0x7F).Concat(GenerateCharsInRange(0xA0, 0xFF)).ToArray();
        Assert.False(Latin1Validator.ContainsNonLatin1Characters(chars));
    }

    [Fact]
    public void ContaisNonLatin1Characters_InvalidChararacters_ReturnsTrue()
    {
        var chars = GenerateCharsInRange(0, 255);
        Assert.True(Latin1Validator.ContainsNonLatin1Characters(chars));
    }

    [Fact]
    public void ContaisNonLatin1Characters_InvalidChararacters_ReturnsTrue2()
    {
        var chars = GenerateCharsInRange(240, 256);
        Assert.True(Latin1Validator.ContainsNonLatin1Characters(chars));
    }

    private static char[] GenerateCharsInRange(ushort inclusiveStart, ushort inclusiveEnd)
    {
        var chars = new char[inclusiveEnd - inclusiveStart + 1];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(inclusiveStart + i);
        }
        return chars;
    }
}
