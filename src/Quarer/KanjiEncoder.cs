namespace Quarer;
internal static class KanjiEncoder
{
    public static bool IsValidKanji(scoped in ReadOnlySpan<char> data)
        => data.ContainsAnyInRange((char)0x8140, (char)0x9FFC) && data.ContainsAnyInRange((char)0xE040, (char)0xEBBF);

    public static void Encode(BitWriter writer, scoped in ReadOnlySpan<char> data)
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
}
