namespace Quarer;

internal static class QrHeaderBlock
{
    //TODO: Support FNC, StructuredAppend etc.
    public static void WriteHeader(BitWriter writer, QrVersion version, ModeIndicator mode, int inputDataLength, EciCode eciCode)
    {
        if (mode is ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend)
        {
            throw new NotSupportedException($"Mode '{mode}' is not supported.");
        }

        if (!eciCode.IsEmpty() && mode is not ModeIndicator.Byte)
        {
            throw new ArgumentException("ECI code is only supported for Byte mode.");
        }

        if (!eciCode.IsEmpty())
        {
            writer.WriteBitsBigEndian((byte)ModeIndicator.Eci, 4);
            writer.WriteBitsBigEndian(eciCode.Value.Value, 8);
        }

        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        writer.WriteBitsBigEndian((byte)mode, 4);
        writer.WriteBitsBigEndian(inputDataLength, characterBitCount);
    }
}
