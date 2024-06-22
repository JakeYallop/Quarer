namespace Quarer;
internal static partial class KanjiEncoder
{
    public static bool ContainsAnyExceptKanji(this scoped ReadOnlySpan<char> data)
        => AnyExceptKanji(data);

    private static bool AnyExceptKanji(this ReadOnlySpan<char> data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] is (< (char)0x8140u or > (char)0x9FFCu) and (< (char)0xE040u or > (char)0xEBBFu))
            {
                return true;
            }
        }

        return false;
    }

    public static void Encode(BitWriter writer, scoped ReadOnlySpan<char> data)
    {
        foreach (var c in data)
        {
            var offset = c switch
            {
                >= (char)0x8140 and <= (char)0x9FFC => (ushort)0x8140,
                >= (char)0xE040 and <= (char)0xEBBF => (ushort)0xC140,
                _ => throw new ArgumentException("Non-kanji character detected.", nameof(data)),
            };
            var value = c - offset;
            var upper = (byte)(value >> 8);
            var lower = (byte)(value & 0xFF);
            var finalValue = (ushort)((upper * 0xC0) + lower);
            writer.WriteBits(finalValue, 13);
        }
    }

    /// <summary>
    /// Gets the length of a kanji bitstream created from the provided data, excluding the mode and character count indicator bits.
    /// </summary>
    /// <returns></returns>
    public static int GetBitStreamLength(scoped ReadOnlySpan<char> kanjiData)
        => kanjiData.Length * 13;
}
