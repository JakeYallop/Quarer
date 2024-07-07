namespace Quarer.Tests;
public class ModeIndicatorExtensionsTests
{
    [Theory]
    [InlineData("1234567890", ModeIndicator.Numeric, 34)]
    [InlineData("HELLO123", ModeIndicator.Alphanumeric, 44)]
    [InlineData("HELLO WORLD", ModeIndicator.Alphanumeric, 61)]
    [InlineData("こんにちは", ModeIndicator.Kanji, 65)]
    [InlineData("Hello, \u4E16\u754C!", ModeIndicator.Byte, 160)]
    public void GetBitStreamLength_ValidData_ReturnsCorrectLength(string data, ModeIndicator mode, int expectedLength)
    {
        var length = mode.GetBitStreamLength(data);
        Assert.Equal(expectedLength, length);
    }
}
