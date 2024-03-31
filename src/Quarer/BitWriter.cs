using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Quarer;

internal sealed class BitWriter
{
    private const byte PrimitiveSize = sizeof(byte) * 8;

    private int _position;
    private int _bitsWritten;
    //TODO: use bigger element, e.g int or long. Maybe create Value builder and use a stackalloc'd buffer
    //TODO: Allow presizing the list or passing in existing buffer to use.
    //TODO: Allow non-contiguous memory, e.g ReadOnlySequence
    private readonly List<byte> _buffer = new(10);

    public BitWriter()
    {
        _buffer.Add(0);
    }

    //TODO: Replace with some kind of complete method? Items cannot be written to the list once this is done
    public ReadOnlySpan<byte> UnsafeGetRawBuffer()
    {
        if (_buffer.Capacity != _buffer.Count)
        {
            _buffer.TrimExcess();
        }
        return CollectionsMarshal.AsSpan(_buffer);
    }

    public void WriteBits(ushort value, int lowBitsToUse)
    {
        ThrowForInvalidValue(value, lowBitsToUse);
        var unwrittenBits = lowBitsToUse;
        while (unwrittenBits > 0)
        {
            var remainingBits = PrimitiveSize - _bitsWritten;
            var bitsWritten = Math.Min(unwrittenBits, remainingBits);

            _buffer[_position] = (byte)((byte)(_buffer[_position] << remainingBits) | (value & GetBitMask(bitsWritten)));

            Advance(bitsWritten);
            unwrittenBits -= bitsWritten;
        }
    }

    [DoesNotReturn]
    private static void ThrowForInvalidValue(ushort value, int lowBitsToUse)
    {
        var maxValue = 1 << (lowBitsToUse - 1);
        if (value > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Expected value to be less than or equal to {maxValue}, to fit within {lowBitsToUse}-bits.");
        }
    }

    private void Advance(int bitsWritten)
    {
        Debug.Assert(bitsWritten is >= 0 and <= PrimitiveSize);
        Debug.Assert(_bitsWritten + bitsWritten is >= 0 and <= PrimitiveSize);
        _bitsWritten += bitsWritten;
        if (bitsWritten is PrimitiveSize)
        {
            _bitsWritten = 0;
            _position++;
            _buffer.Add(0);
        }
        else
        {
            Debug.Assert(_bitsWritten + bitsWritten <= PrimitiveSize);
            _bitsWritten += bitsWritten;
        }
    }
    private static int GetBitMask(int bitsToWrite) => (1 << bitsToWrite) - 1;
}
