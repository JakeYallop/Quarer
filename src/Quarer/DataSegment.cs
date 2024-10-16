namespace Quarer;

/// <summary>
/// Represents a segment of data encoded in a specific mode in a QR code.
/// </summary>
public readonly record struct DataSegment
{
    private DataSegment(ModeIndicator mode, Range range, int fullSegmentLength, EciCode eciCode)
    {
        Mode = mode;
        Range = range;
        FullSegmentLength = fullSegmentLength;
        EciCode = eciCode;
    }

    /// <summary>
    /// Creates a new data segment with the specified parameters.
    /// </summary>
    /// <param name="characterCount">The number of characters in this segment.</param>
    /// <param name="mode">The mode this segment of data is encoded in.</param>
    /// <param name="bitstreamLength">The length in bits of the encoded data.</param>
    /// <param name="range">A range representing the span of bytes in the original stream of data that this segment consists of.</param>
    /// <param name="eciCode">An ECI code providing more information on the exact format of <see cref="ModeIndicator.Byte"/> encoded data. This must be empty if <paramref name="mode"/> is not <see cref="ModeIndicator.Byte"/>.</param>
    public static DataSegment Create(ushort characterCount, ModeIndicator mode, int bitstreamLength, Range range, EciCode eciCode)
    {
        if (mode != ModeIndicator.Byte && !eciCode.IsEmpty())
        {
            throw new ArgumentException("ECI codes are only supported for byte mode.", nameof(eciCode));
        }

        var fullLength = 4 + bitstreamLength + characterCount + eciCode.GetDataSegmentLength();

        return new DataSegment(mode, range, fullLength, eciCode);
    }

    /// <summary>
    /// The mode this segment of data is encoded in.
    /// </summary>
    public ModeIndicator Mode { get; }
    /// <summary>
    /// The range of bytes in the original data stream that this segment consists of.
    /// </summary>
    public Range Range { get; }
    /// <summary>
    /// Segment length in bits including mode indicator and character count.
    /// </summary>
    public int FullSegmentLength { get; }
    /// <summary>
    /// The ECI code for this segment. May be empty.
    /// </summary>
    public EciCode EciCode { get; }
}
