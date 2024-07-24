namespace Quarer.Tests;

public class DataSegmentTests
{
    [Fact]
    public void DataSegment_FullSegmentLength_ReturnsExpectedResult()
    {
        var dataLength = Random.Shared.Next(1, 100);
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var segment = DataSegment.Create(characterCount, ModeIndicator.Numeric, dataLength, ..);
        Assert.Equal(dataLength + 4 + characterCount, segment.FullSegmentLength);
    }
}
