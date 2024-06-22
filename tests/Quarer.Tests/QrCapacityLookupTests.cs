namespace Quarer.Tests;
public sealed class QrCapacityLookupTests
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
        Assert.True(QrCapacityLookup.TryGetVersionForDataCapacity(characters, mode, errorCorrectionLevel, out var version));
        Assert.Equal(expectedVersion, version);
    }

    [Theory]
    [InlineData((int)ErrorCorrectionLevel.L - 1)]
    [InlineData((int)ErrorCorrectionLevel.H + 1)]
    public void InvalidErrorLevel_ThrowsArgumentOutOfRangeException(int errorCorrectionLevel)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrCapacityLookup.TryGetVersionForDataCapacity(1, ModeIndicator.Numeric, (ErrorCorrectionLevel)errorCorrectionLevel, out _));

    [Theory]
    [InlineData(ModeIndicator.Terminator)]
    [InlineData(ModeIndicator.Eci)]
    [InlineData(ModeIndicator.Fnc1FirstPosition)]
    [InlineData(ModeIndicator.Fnc1SecondPosition)]
    [InlineData(ModeIndicator.StructuredAppend)]
    public void InvalidMode_ThrowsNotSupportedException(ModeIndicator mode)
        => Assert.Throws<NotSupportedException>(() => QrCapacityLookup.TryGetVersionForDataCapacity(1, mode, ErrorCorrectionLevel.L, out _));

    [Theory]
    [InlineData(5, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 255)]
    [InlineData(13, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 483)]
    [InlineData(25, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 715)]
    [InlineData(35, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 605)]

    [InlineData(QrVersion.MinVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 34)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 27)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 17)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 25)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 20)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 16)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 10)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Byte, ErrorCorrectionLevel.L, 17)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Byte, ErrorCorrectionLevel.M, 14)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 11)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Byte, ErrorCorrectionLevel.H, 7)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 10)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 8)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 7)]
    [InlineData(QrVersion.MinVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 4)]

    [InlineData(QrVersion.MaxVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 7134)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 5469)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 3993)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 3009)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 4318)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 3313)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 2420)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 1823)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Byte, ErrorCorrectionLevel.L, 2953)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Byte, ErrorCorrectionLevel.M, 2245)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 1628)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Byte, ErrorCorrectionLevel.H, 1246)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 1829)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 1390)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 1003)]
    [InlineData(QrVersion.MaxVersion, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 779)]
    public void GetCapacity_WithValidArguments_ReturnsCorrectCapacity(int version, ModeIndicator mode, ErrorCorrectionLevel errorLevel, int expectedCapacity)
    {
        var capacity = QrCapacityLookup.GetCapacity((QrVersion)version, mode, errorLevel);
        Assert.Equal(expectedCapacity, capacity);
    }

    [Theory]
    [InlineData(QrVersion.MinVersion - 1)]
    [InlineData(QrVersion.MaxVersion + 1)]
    public void GetCapacity_WithInvalidVersion_ThrowsArgumentOutOfRangeException(int version)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrCapacityLookup.GetCapacity((QrVersion)version, ModeIndicator.Numeric, ErrorCorrectionLevel.L));

    [Theory]
    [InlineData(ModeIndicator.Terminator)]
    [InlineData(ModeIndicator.Eci)]
    [InlineData(ModeIndicator.Fnc1FirstPosition)]
    [InlineData(ModeIndicator.Fnc1SecondPosition)]
    [InlineData(ModeIndicator.StructuredAppend)]
    public void GetCapacity_WithInvalidMode_ThrowsNotSupportedException(ModeIndicator mode)
        => Assert.Throws<NotSupportedException>(() => QrCapacityLookup.GetCapacity(QrVersion.Min, mode, ErrorCorrectionLevel.L));

    [Theory]
    [InlineData((int)ErrorCorrectionLevel.L - 1)]
    [InlineData((int)ErrorCorrectionLevel.H + 1)]
    public void GetCapacity_WithInvalidErrorLevel_ThrowsArgumentOutOfRangeException(int errorLevel)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrCapacityLookup.GetCapacity(QrVersion.Min, ModeIndicator.Numeric, (ErrorCorrectionLevel)errorLevel));
}
