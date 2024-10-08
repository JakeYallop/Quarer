namespace Quarer;

public readonly record struct DataSegment
{
    private DataSegment(ModeIndicator mode, Range range, int fullSegmentLength, EciCode eciCode)
    {
        Mode = mode;
        Range = range;
        FullSegmentLength = fullSegmentLength;
        EciCode = eciCode;
    }

    public static DataSegment Create(ushort characterCount, ModeIndicator mode, int bitstreamLength, Range range, EciCode eciCode)
    {
        var fullLength = 4 + bitstreamLength + characterCount + eciCode.GetDataSegmentLength();

        return new DataSegment(mode, range, fullLength, eciCode);
    }

    public ModeIndicator Mode { get; }
    public Range Range { get; }
    /// <summary>
    /// Segment length in bits including mode indicator and character count.
    /// </summary>
    public int FullSegmentLength { get; }
    public EciCode EciCode { get; }
}
