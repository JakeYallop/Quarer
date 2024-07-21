namespace Quarer;

internal readonly struct QrHeaderBlock
{
    private QrHeaderBlock(ModeIndicator modeIndicator, ushort characterCountBitCount, int dataCharactersCount)
    {
        ModeIndicator = modeIndicator;
        CharacterCountBitCount = characterCountBitCount;
        InputDataLength = dataCharactersCount;
    }

    public static QrHeaderBlock Create(QrVersion version, ModeIndicator mode, int inputDataLength)
    {
        if (mode is ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend)
        {
            throw new NotSupportedException($"Mode '{mode}' is not supported.");
        }

        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        return new QrHeaderBlock(mode, characterBitCount, inputDataLength);
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
    //TODO: Add IEquatable/Equality if this is used in dictionaries or searched for anywhere.
}
