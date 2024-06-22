namespace Quarer.Tests;
public sealed class QrDataCapacityLookupTests
{
    [Theory]
    [InlineData(254, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 5)]
    [InlineData(255, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 5)]
    [InlineData(256, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 6)]
    [InlineData(255, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 6)]
    [InlineData(1270, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 38)]
    [InlineData(370, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 18)]
    [InlineData(154, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 7)]
    [InlineData(282, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 14)]

    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 1)]

    [InlineData(7089, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 40)]
    [InlineData(5596, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 40)]
    [InlineData(3993, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 40)]
    [InlineData(3057, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 40)]
    [InlineData(4296, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 40)]
    [InlineData(3391, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 40)]
    [InlineData(2420, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 40)]
    [InlineData(1852, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 40)]
    [InlineData(2953, ModeIndicator.Byte, ErrorCorrectionLevel.L, 40)]
    [InlineData(2331, ModeIndicator.Byte, ErrorCorrectionLevel.M, 40)]
    [InlineData(1663, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 40)]
    [InlineData(1273, ModeIndicator.Byte, ErrorCorrectionLevel.H, 40)]
    [InlineData(1817, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 40)]
    [InlineData(1435, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 40)]
    [InlineData(1024, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 40)]
    [InlineData(784, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 40)]
    public void TryGetVersionForCapacity_WithValidCapacity_ReturnsTrue(int characters, ModeIndicator mode, ErrorCorrectionLevel errorCorrectionLevel, int expectedVersion)
    {
        Assert.True(QrDataCapacityLookup.TryGetVersionForCapacity(characters, mode, errorCorrectionLevel, out var version));
        Assert.Equal(expectedVersion, version);
    }

    [Theory]
    [InlineData((int)ErrorCorrectionLevel.L - 1)]
    [InlineData((int)ErrorCorrectionLevel.H + 1)]
    public void InvalidErrorLevel_ThrowsArgumentOutOfRangeException(int errorCorrectionLevel)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => QrDataCapacityLookup.TryGetVersionForCapacity(1, ModeIndicator.Numeric, (ErrorCorrectionLevel)errorCorrectionLevel, out _));
    }

    [Theory]
    [InlineData(ModeIndicator.Terminator)]
    [InlineData(ModeIndicator.Eci)]
    [InlineData(ModeIndicator.Fnc1FirstPosition)]
    [InlineData(ModeIndicator.Fnc1SecondPosition)]
    [InlineData(ModeIndicator.StructuredAppend)]
    public void InvalidMode_ThrowsNotSupportedException(ModeIndicator mode)
    {
        Assert.Throws<NotSupportedException>(() => QrDataCapacityLookup.TryGetVersionForCapacity(1, mode, ErrorCorrectionLevel.L, out _));
    }
}
