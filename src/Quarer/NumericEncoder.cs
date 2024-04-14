using System.Diagnostics;

namespace Quarer;

//TODO: Consider making this a class (or struct) with instance methods (even though it has no state) for a tiny bit of extra perf
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

            writer.WriteBits(GetDigitsAsValue(in remainingDigits), in numberOfBits);
        }
    }

    /// <summary>
    /// Gets the length of a numeric bitstream created from the provided data, excluding the mode and character count indicator bits.
    /// </summary>
    /// <returns></returns>
    public static int GetBitStreamLength(scoped in ReadOnlySpan<byte> numericData) => (numericData.Length / 3 * 10) + GetRemainderBitCount(numericData.Length);
    private static int GetRemainderBitCount(int length) => (length % 3) switch
    {
        0 => 0,
        1 => 4,
        2 => 7,
        _ => throw new UnreachableException()
    };

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
