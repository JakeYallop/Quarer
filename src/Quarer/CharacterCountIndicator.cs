namespace Quarer;

internal static class CharacterCountIndicator
{
    internal static ReadOnlySpan<short> CharacterCountNumeric => [10, 12, 14];
    internal static ReadOnlySpan<short> CharacterCountAlphanumeric => [9, 11, 13];
    internal static ReadOnlySpan<short> CharacterCountByte => [8, 16, 16];
    internal static ReadOnlySpan<short> CharacterCountKanji => [8, 10, 12];

    public static short GetCharacterCount(QrModeIndicator mode, QrVersion version)
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
            QrModeIndicator.Numeric => CharacterCountNumeric[offset],
            QrModeIndicator.Alphanumeric => CharacterCountAlphanumeric[offset],
            QrModeIndicator.Byte => CharacterCountByte[offset],
            QrModeIndicator.Kanji => CharacterCountKanji[offset],
            _ => throw new NotSupportedException($"Unexpected QrModeIndicator '{mode}'. Character counts are only required by {QrModeIndicator.Numeric}, {QrModeIndicator.Alphanumeric}, {QrModeIndicator.Byte}, {QrModeIndicator.Kanji} modes."),
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
