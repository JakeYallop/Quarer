using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Quarer;
internal static partial class KanjiEncoder
{
    public static bool ContainsAnyExceptKanji(this scoped ReadOnlySpan<byte> data)
        => AnyExceptKanji(data);

    [SkipLocalsInit]
    private static unsafe bool AnyExceptKanji(this ReadOnlySpan<byte> data)
    {
        if ((data.Length & 1) == 1)
        {
            // if data.Length not is a multiple of 2, it cannot contain valid kanji character pairs
            return true;
        }

        if (BitConverter.IsLittleEndian)
        {
            var asUshort = MemoryMarshal.Cast<byte, ushort>(data);
            Span<ushort> span = stackalloc ushort[256];
            var current = asUshort;
            while (current.Length > 0)
            {
                var chunk = current[..Math.Min(current.Length, 256)];
                BinaryPrimitives.ReverseEndianness(chunk, span);
                if (ContainsAnyExceptKani(span[..chunk.Length]))
                {
                    return true;
                }
                current = current[chunk.Length..];
            }

            return false;

        }
        else
        {
            var span = MemoryMarshal.Cast<byte, ushort>(data);
            return ContainsAnyExceptKani(span);
        }

        static bool ContainsAnyExceptKani(scoped ReadOnlySpan<ushort> data)
        {
            foreach (var c in data)
            {
                if (c is (< (ushort)0x8140u or > (ushort)0x9FFCu) and (< (ushort)0xE040u or > (ushort)0xEBBFu))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static void Encode(BitWriter writer, scoped ReadOnlySpan<byte> data)
    {
        if ((data.Length & 1) != 0)
        {
            throw new ArgumentException("Expected data length to be a multiple of 2.", nameof(data));
        }

        for (var i = 0; i < data.Length; i += 2)
        {
            var upper = data[i];
            var lower = data[i + 1];
            var c = (ushort)((upper << 8) | lower);

            var offset = c switch
            {
                >= 0x8140 and <= 0x9FFC => (ushort)0x8140,
                >= 0xE040 and <= 0xEBBF => (ushort)0xC140,
                _ => throw new ArgumentException("Non-kanji character detected.", nameof(data)),
            };

            var value = c - offset;
            var finalUpper = (byte)(value >> 8);
            var finalLower = (byte)(value & 0xFF);
            var finalValue = (ushort)((finalUpper * 0xC0) + finalLower);
            writer.WriteBitsBigEndian(finalValue, 13);
        }
    }

    /// <summary>
    /// Gets the length of a kanji bitstream created from the provided data, excluding the mode and character count indicator bits.
    /// </summary>
    /// <returns></returns>
    public static int GetBitStreamLength(scoped ReadOnlySpan<byte> kanjiData)
        => (kanjiData.Length >> 1) * 13;
}
