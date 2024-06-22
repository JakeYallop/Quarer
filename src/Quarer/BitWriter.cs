using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Quarer;

//TODO: Add ReadX APIs, maybe rename to BitStream
internal sealed class BitWriter
{
    private const int BitsPerElement = 32;
    private const int BitShiftPerInt32 = 5;
    private int _position;
    private int _bitsWritten;
    private int _capacity;
    //TODO: Allow using a pre-allocated buffer, maybe add a value builder and use a stackalloc'd buffer.
    private readonly List<int> _buffer = new(16);

    public BitWriter()
    {
    }

    /// <summary>
    /// The total number of bits written.
    /// </summary>
    public int Count { get; set; }

    public void WriteBits<T>(T value, int bitCount) where T : IBinaryInteger<T>
    {
        ThrowForInvalidValue(value, bitCount);
        var remainingBitsInValue = bitCount;
        EnsureCapacity(checked(Count + remainingBitsInValue));
        var buffer = CollectionsMarshal.AsSpan(_buffer);
        while (remainingBitsInValue > 0)
        {
            var remainingBitsForCurrentPosition = BitsPerElement - _bitsWritten;
            var bitsToWrite = Math.Min(remainingBitsInValue, remainingBitsForCurrentPosition);

            remainingBitsInValue -= bitsToWrite;
            ref var segment = ref buffer[_position];
            segment |= GetBitsFromValue(value, bitsToWrite, remainingBitsInValue) << (BitsPerElement - bitsToWrite - _bitsWritten);
            Advance(bitsToWrite);
        }
    }

    public IEnumerable<bool> GetBitStream()
    {
        //TODO: Return an enumerable that implements ICollection so that TryGetNonEnumeratedCount etc works
        var current = 0;
        foreach (var i in _buffer)
        {
            for (var currentBit = 0; currentBit < BitsPerElement; currentBit++)
            {
                if (current == Count)
                {
                    yield break;
                }

                var mask = 1 << (BitsPerElement - currentBit - 1);
                yield return (i & mask) != 0;
                current++;
            }
        }
    }

    //public int ReadBlock(int index, int count)

    private static int GetBitsFromValue<T>(T value, int bitCount, int remainingBitsInValue) where T : IBinaryInteger<T>
        => int.CreateChecked((value >> remainingBitsInValue) & GetAllSetBitMask<T>(bitCount));
    private static T GetAllSetBitMask<T>(int bitCount) where T : IBinaryInteger<T>
        => (T.One << bitCount) - T.One;

    private static void ThrowForInvalidValue<T>(T value, int lowBitsToUse) where T : IBinaryInteger<T>
    {
        var maxValue = T.One << lowBitsToUse;
        if (value >= maxValue || T.PopCount(value) > T.CreateSaturating(lowBitsToUse))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Expected value to be less than or equal to {maxValue}, to fit within {lowBitsToUse}-bits.");
        }
    }

    private void Advance(int bitsWritten)
    {
        Debug.Assert(bitsWritten is >= 0 and <= BitsPerElement);
        Debug.Assert(_bitsWritten + bitsWritten is >= 0 and <= BitsPerElement);

        _bitsWritten += bitsWritten;
        Count += bitsWritten;
        if (_bitsWritten is BitsPerElement)
        {
            _bitsWritten = 0;
            _position++;
        }

        Debug.Assert(_position <= _buffer.Count);
    }

    private void EnsureCapacity(int requestedCapacity)
    {
        if (requestedCapacity > _capacity)
        {
            var currentBitCapactity = _buffer.Count << BitShiftPerInt32;
            var newBitCapacity = Math.Max(currentBitCapactity, BitsPerElement);
            while (newBitCapacity < requestedCapacity)
            {
                newBitCapacity = (int)Math.Min(((long)newBitCapacity) * 2, int.MaxValue);
            };

            var intCapacity = GetInt32LengthFromBits(newBitCapacity);
            Debug.Assert(ComputeMod32(newBitCapacity) == 0);
            _capacity = newBitCapacity;
            _buffer.EnsureCapacity(intCapacity);
            CollectionsMarshal.SetCount(_buffer, intCapacity);
        }
    }

    private static int GetInt32LengthFromBits(int bits) => (int)((uint)(bits - 1 + (1 << BitShiftPerInt32)) >> BitShiftPerInt32);
    private static int ComputeMod32(int bits) => bits & (BitShiftPerInt32 - 1);
}
