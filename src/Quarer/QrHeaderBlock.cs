namespace Quarer;

internal static class QrHeaderBlock
{
    //TODO: Support ECI, FNC, StructuredAppend etc.
    public static void WriteHeader(BitWriter writer, QrVersion version, ModeIndicator mode, int inputDataLength)
    {
        if (mode is ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend)
        {
            throw new NotSupportedException($"Mode '{mode}' is not supported.");
        }

        var characterBitCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        writer.WriteBitsBigEndian((byte)mode, 4);
        writer.WriteBitsBigEndian(inputDataLength, characterBitCount);
    }
}
