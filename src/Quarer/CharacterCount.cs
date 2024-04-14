namespace Quarer;

internal static class CharacterCount
{
    internal static ReadOnlySpan<ushort> CharacterCountNumeric => [10, 12, 14];
    internal static ReadOnlySpan<ushort> CharacterCountAlphanumeric => [9, 11, 13];
    internal static ReadOnlySpan<ushort> CharacterCountByte => [8, 16, 16];
    internal static ReadOnlySpan<ushort> CharacterCountKanji => [8, 10, 12];

    public static ushort GetCharacterCountBitCount(in QrVersion version, in ModeIndicator mode)
    {
        var offset = version.Version switch
        {
            >= 1 and <= 9 => 0,
            > 9 and <= 26 => 1,
            >= 27 and <= 40 => 2,
            _ => throw new NotSupportedException($"Invalid QrVersion found. Expected a version from 1 and 40, but found '{version.Version}'.")
        };

#pragma warning disable IDE0072 // Add missing cases
        return mode switch
        {
            ModeIndicator.Numeric => CharacterCountNumeric[offset],
            ModeIndicator.Alphanumeric => CharacterCountAlphanumeric[offset],
            ModeIndicator.Byte => CharacterCountByte[offset],
            ModeIndicator.Kanji => CharacterCountKanji[offset],
            _ => throw new NotSupportedException($"Unexpected QrModeIndicator '{mode}'. Character counts are only required by {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte}, {ModeIndicator.Kanji} modes."),
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
