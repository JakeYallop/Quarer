namespace Quarer;

internal static class NumericEncoder
{
    public static void Encode(BitWriter writer, scoped in ReadOnlySpan<byte> data)
    {
        var position = 0;
        for (; position + 3 <= data.Length; position += 3)
        {
            var digits = data[position..(position + 3)];
            var v = GetDigitsAsValue(digits);
            writer.WriteBits(v, 10);
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

            writer.WriteBits(GetDigitsAsValue(in remainingDigits), numberOfBits);
        }
    }

    private static ushort GetDigitsAsValue(in ReadOnlySpan<byte> slicedDigits)
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
