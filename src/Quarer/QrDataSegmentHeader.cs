namespace Quarer;

internal readonly struct QrDataSegmentHeader
{
    private QrDataSegmentHeader(in ModeIndicator modeIndicator, in ushort characterCountBitCount, in int dataCharactersCount)
    {
        ModeIndicator = modeIndicator;
        CharacterCountBitCount = characterCountBitCount;
        InputDataLength = dataCharactersCount;
    }

    public static QrDataSegmentHeader Create(in QrVersion version, in ModeIndicator mode, in int inputDataLength)
    {
        if (mode is ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend)
        {
            throw new NotSupportedException($"Mode '{mode}' is not supported.");
        }

        var characterBitCount = CharacterCountIndicator.GetCharacterCountBitCount(in version, in mode);
        return new QrDataSegmentHeader(in mode, in characterBitCount, in inputDataLength);
    }

    public ModeIndicator ModeIndicator { get; }
    public ushort CharacterCountBitCount { get; }
    public int InputDataLength { get; }

    public void WriteHeader(BitWriter writer)
    {
        writer.WriteBits((byte)ModeIndicator, 4);
        writer.WriteBits(InputDataLength, CharacterCountBitCount);
    }

    //TODO: Support ECI, FNC, StructuredAppend etc.
}
