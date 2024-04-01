using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Quarer;

internal class BitWriter
{
    private const byte PrimitiveSize = sizeof(byte) * 8;

    private int _position;
    private int _bitsWritten;
    //TODO: use bigger element, e.g int or long.
    //TODO: Allowing using a pre-allocated buffer, maybe add a value builder and use a stackalloc'd buffer.
    protected readonly List<byte> Buffer = new(16);

    public BitWriter()
    {
    }

    //TODO: Replace with some kind of complete method? Items cannot be written to the list once this is done
    public ReadOnlySpan<byte> UnsafeGetRawBuffer()
    {
        if (Buffer.Capacity != Buffer.Count)
        {
            Buffer.TrimExcess();
        }
        return CollectionsMarshal.AsSpan(Buffer);
    }

    public virtual void WriteBits<T>(T value, int lowBitsToUse) where T : IBinaryInteger<T>
    {
        ThrowForInvalidValue(value, lowBitsToUse);
        int remainingBitsInValue = lowBitsToUse;
        while (remainingBitsInValue > 0)
        {
            EnsureSpace();
            int remainingBitsForCurrentPosition = PrimitiveSize - _bitsWritten;
            int bitsToWrite = Math.Min(remainingBitsInValue, remainingBitsForCurrentPosition);

            remainingBitsInValue -= bitsToWrite;

            Buffer[_position] = (byte)((byte)(Buffer[_position] << remainingBitsForCurrentPosition) | GetBitsFromValue(value, bitsToWrite, remainingBitsInValue));
            Advance(bitsToWrite);
        }
    }

    private static byte GetBitsFromValue<T>(T value, int bits, int remainingBitsInValue) where T : IBinaryInteger<T>
    => byte.CreateChecked((value >> remainingBitsInValue) & GetBitMask<T>(bits));
    private static T GetBitMask<T>(int bits) where T : IBinaryInteger<T>
        => (T.One << bits) - T.One;

    private static void ThrowForInvalidValue<T>(T value, int lowBitsToUse) where T : IBinaryInteger<T>
    {
        T maxValue = T.One << lowBitsToUse;
        if (value >= maxValue || T.PopCount(value) > T.CreateSaturating(lowBitsToUse))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Expected value to be less than or equal to {maxValue}, to fit within {lowBitsToUse}-bits.");
        }
    }


    //public virtual void WriteBits(ushort value, int lowBitsToUse)
    //{
    //    ThrowForInvalidValue(value, lowBitsToUse);
    //    int remainingBitsInValue = lowBitsToUse;
    //    while (remainingBitsInValue > 0)
    //    {
    //        EnsureSpace();
    //        int remainingBitsForCurrentPosition = PrimitiveSize - _bitsWritten;
    //        int bitsToWrite = Math.Min(remainingBitsInValue, remainingBitsForCurrentPosition);

    //        remainingBitsInValue -= bitsToWrite;

    //        _buffer[_position] = (byte)((byte)(_buffer[_position] << remainingBitsForCurrentPosition) | GetBitsFromValue(value, bitsToWrite, remainingBitsInValue));
    //        Advance(bitsToWrite);
    //    }
    //}

    private void Advance(int bitsWritten)
    {
        Debug.Assert(bitsWritten is >= 0 and <= PrimitiveSize);
        Debug.Assert(_bitsWritten + bitsWritten is >= 0 and <= PrimitiveSize);
        _bitsWritten += bitsWritten;
        if (_bitsWritten is PrimitiveSize)
        {
            _bitsWritten = 0;
            _position++;
        }
    }

    private void EnsureSpace()
    {
        if (_position + 1 > Buffer.Count)
        {
            Buffer.Add(0);
        }
    }
    //private static byte GetBitsFromValue(ushort value, int bits, int remainingBitsInValue)
    //    => (byte)((value >> remainingBitsInValue) & GetBitMask(bits));
    //private static int GetBitMask(int bits) => (1 << bits) - 1;

    //private static void ThrowForInvalidValue(ushort value, int lowBitsToUse)
    //{
    //    int maxValue = 1 << lowBitsToUse;
    //    if (value >= maxValue)
    //    {
    //        throw new ArgumentOutOfRangeException(nameof(value), $"Expected value to be less than or equal to {maxValue}, to fit within {lowBitsToUse}-bits.");
    //    }
    //}
}
