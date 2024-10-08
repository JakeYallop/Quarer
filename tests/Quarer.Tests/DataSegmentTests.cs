namespace Quarer.Tests;

public class DataSegmentTests
{
    [Fact]
    public void DataSegment_FullSegmentLength_ReturnsExpectedResult()
    {
        var dataLength = Random.Shared.Next(1, 100);
        var version = QrVersion.GetVersion(1);
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var segment = DataSegment.Create(characterCount, ModeIndicator.Numeric, dataLength, .., EciCode.Empty);
        Assert.Equal(dataLength + 4 + characterCount, segment.FullSegmentLength);
    }

    [Fact]
    public void DataSegment_FullSegmentLength_WithEciCode_ReturnsExpectedResult()
    {
        var dataLength = Random.Shared.Next(1, 100);
        var version = QrVersion.GetVersion(1);
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, ModeIndicator.Numeric);
        var segment = DataSegment.Create(characterCount, ModeIndicator.Numeric, dataLength, .., new EciCode(26));
        Assert.Equal(dataLength + 4 + 4 + 8 + characterCount, segment.FullSegmentLength);
    }

    [Fact]
    public void Create_EciCodeGreaterThan127_ThrowsArgumentOutOfRangeException()
    {
        ushort characterCount = 10;
        var dataLength = 10;
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => DataSegment.Create(characterCount, ModeIndicator.Byte, dataLength, .., new EciCode(128)));
    }
}
