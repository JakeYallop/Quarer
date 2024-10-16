using System.Runtime.CompilerServices;

namespace Quarer.Tests;

public static class BitBufferExtensions
{
    public static IEnumerable<bool> AsBitEnumerable(this BitBuffer bitBuffer)
    {
        var current = 0;
        var buffer = _buffer(bitBuffer);
        foreach (var element in buffer)
        {
            for (var currentBit = 0; currentBit < BitBuffer.BitsPerElement; currentBit++)
            {
                if (current == bitBuffer.Count)
                {
                    yield break;
                }
                yield return ReadBit(element, currentBit);
                current++;
            }
        }
    }
    private static bool ReadBit(byte element, int offset) => (element & GetBitMask(offset)) != 0;
    private static byte GetBitMask(int bitIndex) => (byte)(1 << (BitBuffer.BitsPerElement - bitIndex - 1));

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref List<byte> _buffer(BitBuffer @this);
}
