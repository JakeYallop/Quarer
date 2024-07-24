namespace Quarer;

public readonly record struct DataSegment
{
    private DataSegment(ModeIndicator mode, Range range, int fullSegmentLength)
    {
        Mode = mode;
        Range = range;
        FullSegmentLength = fullSegmentLength;
    }

    //TODO: ECI indicators come before Mode and affect the total length. Can we just pass in a flag?
    //see: https://github.com/zxing/zxing/blob/2fb22b724660b9af7edd22fc0f88358fdaf63aa1/core/src/main/java/com/google/zxing/qrcode/encoder/MinimalEncoder.java#L445
    public static DataSegment Create(ushort characterCount, ModeIndicator mode, int bitstreamLength, Range range)
    {
        var fullLength = 4 + bitstreamLength + characterCount;
        return new DataSegment(mode, range, fullLength);
    }

    public ModeIndicator Mode { get; }
    public Range Range { get; }
    /// <summary>
    /// Segment length including mode indicator and character count.
    /// </summary>
    public int FullSegmentLength { get; }

    public void Deconstruct(out ModeIndicator mode, out Range range, out int fullSegmentLength)
    {
        mode = Mode;
        range = Range;
        fullSegmentLength = FullSegmentLength;
    }

    public override string ToString()
        => $"{nameof(Range)}: {Range}, {nameof(Mode)}: {Mode}, {nameof(FullSegmentLength)}: {FullSegmentLength}";
}

