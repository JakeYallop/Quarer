namespace Quarer;

internal static class CharacterCountIndicator
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

internal readonly struct NumericModeEncoder(BitWriter writer)
{
    private readonly BitWriter _writer = writer;

    public void Encode(ReadOnlySpan<byte> data)
    {
        var position = 0;
        for (; position + 3 <= data.Length; position += 3)
        {
            var digits = data[position..(position + 3)];
            var v = GetDigitsAsValue(digits);
            _writer.WriteBits(v, 10);
        }

        if (data.Length != position)
        {
            var remaining = data.Length - position;
            var remainingDigits = data[position..(position + remaining)];
            var numberOfBits = remaining switch
            {
                1 => 4,
                2 => 7,
                _ => throw new InvalidOperationException("Expected only 1 or 2 digits as a remainder after encoding all other 10 bit triples.")
            };

            _writer.WriteBits(GetDigitsAsValue(remainingDigits), numberOfBits);
        }
    }

    private static ushort GetDigitsAsValue(ReadOnlySpan<byte> slicedDigits)
    {
        return slicedDigits.Length switch
        {
            3 => (ushort)((slicedDigits[0] * 100) + (slicedDigits[1] * 10) + slicedDigits[2]),
            2 => (ushort)((slicedDigits[0] * 10) + slicedDigits[1]),
            1 => slicedDigits[0],
            _ => throw new InvalidOperationException($"Expected 1, 2 or 3 digits, found '{slicedDigits.Length}' digits instead.")
        };
    }
}

internal readonly struct QrHeader(in ModeIndicator modeIndicator, in ushort characterCountBitCount, in int inputDataCharacters)
{
    public static QrHeader Create(in QrVersion version, in ModeIndicator mode, in int inputDataLength)
    {
        if (mode is ModeIndicator.Eci or ModeIndicator.Fnc1FirstPosition or ModeIndicator.Fnc1SecondPosition or ModeIndicator.StructuredAppend)
        {
            throw new NotSupportedException($"Mode '{mode}' is not supported.");
        }

        var characterBitCount = CharacterCountIndicator.GetCharacterCountBitCount(in version, in mode);
        return new QrHeader(in mode, in characterBitCount, in inputDataLength);
    }

    public ModeIndicator ModeIndicator { get; } = modeIndicator;
    public ushort CharacterCountBitCount { get; } = characterCountBitCount;
    public int InputDataLength { get; } = inputDataCharacters;

    public void WriteHeader(BitWriter writer)
    {
        writer.WriteBits((byte)ModeIndicator, 4);
        writer.WriteBits(InputDataLength, CharacterCountBitCount);
    }

    //TODO: Support ECI, FNC, StructuredAppend etc.
}
