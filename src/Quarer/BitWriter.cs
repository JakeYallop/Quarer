using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Quarer;

//TODO: Rename to BitArray or similar (BitBuffer, BitArray, BitStream?)
public sealed class BitWriter(int initialCapacity)
{
    private const int BitsPerElement = 32;
    private const int BitShiftPerElement = 5;
    private const int BytesPerElement = 4;
    private const int BytesShiftPerElement = 3;

    private int _position;
    private int _bitsWritten;
    private int _capacity;
    //TODO: Allow using a pre-allocated buffer, maybe add a value builder and use a stackalloc'd buffer.
    private readonly List<uint> _buffer = new(initialCapacity);

    public BitWriter() : this(16)
    {
    }

    /// <summary>
    /// The total number of bits written.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// The total number of bytes, rounded up to the nearest full byte.
    /// </summary>
    public int ByteCount
    {
        get
        {
            var (fullElements, remainder) = int.DivRem(Count, 8);
            return fullElements + (remainder > 0 ? 1 : 0);
        }
    }

    public void WriteBits<T>(T value, int bitCount) where T : INumber<T>, IBinaryInteger<T>
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

    /// <summary>
    /// Returns bytes from this <see cref="BitWriter"/>. The number of bytes returned is rounded up to the nearest full byte,
    /// with the final byte padded with zeros if necessary
    /// </summary>
    /// <returns></returns>
    public IEnumerable<byte> GetByteStream()
    {
        //TODO: Return an enumerable that implements ICollection so that TryGetNonEnumeratedCount etc works
        var current = 0;
        var fullElements = Math.DivRem(Count, BitsPerElement, out var remainder);
        while (current < fullElements)
        {
            var i = _buffer[current];

            for (var currentBit = 0; currentBit < BitsPerElement; currentBit += 8)
            {
                var @byte = GetBitsFromValue(i, 8, BitsPerElement - currentBit - 8);
                yield return (byte)@byte;
            }

            current++;
        }

        if (remainder == 0)
        {
            yield break;
        }

        var lastElement = _buffer[current];
        for (var currentBit = 0; currentBit < remainder; currentBit += 8)
        {
            var @byte = GetBitsFromValue(lastElement, 8, BitsPerElement - currentBit - 8);
            yield return (byte)@byte;
        }
    }

    public int GetBytes(Range range, Span<byte> destination)
    {
        var (byteOffset, length) = range.GetOffsetAndLength(ByteCount);

        if (destination.Length < length)
        {
            throw new ArgumentException("The destination span is too small.", nameof(destination));
        }

        var written = 0;
        var lengthInBits = length << 3;
        var offsetInStartElement = (byteOffset << 3) & (BitsPerElement - 1);
        var currentElement = GetElementLengthFromBytesFloor(byteOffset);

        if (offsetInStartElement > 0)
        {
            var bitsToReadInFirstElement = Math.Min(BitsPerElement - offsetInStartElement, lengthInBits);
            written += ReadBytes(_buffer[currentElement], offsetInStartElement, offsetInStartElement + bitsToReadInFirstElement, destination);
            currentElement++;
        }

        var slicedDestination = destination[written..];
        for (; length - written >= 4; currentElement++)
        {
            Debug.Assert((byteOffset + written) % BytesPerElement == 0, $"Expected expression to be aligned at {BitsPerElement}-bit boundary.");
            BinaryPrimitives.WriteUInt32BigEndian(slicedDestination, _buffer[currentElement]);
            slicedDestination = slicedDestination[BytesPerElement..];
            written += BytesPerElement;
        }

        var bitsRemainingInFinalElement = (length - written) << BytesShiftPerElement;
        if (bitsRemainingInFinalElement > 0)
        {
            //if (written > 0)
            //{
            //    currentElement++;
            //}

            written += ReadBytes(_buffer[currentElement], 0, bitsRemainingInFinalElement, slicedDestination);
        }

        return written;
    }

    private static int ReadBytes(uint element, int start, int end, Span<byte> destination)
    {
        Debug.Assert(start % 8 == 0, "Expected start offset to be byte aligned.");
        Debug.Assert(end % 8 == 0, "Expected end offset to be byte aligned.");
        Debug.Assert(destination.Length >= (end - start) / 8, "Expected destination span to be larger.");
        var written = 0;
        var destinationOffset = 0;
        for (var offset = start; offset < end; offset += 8)
        {
            var byteValue = ReadByte(element, offset);
            destination[destinationOffset] = byteValue;
            destinationOffset++;
            written++;
        }
        return written;
    }

    private static byte ReadByte(uint element, int offset)
    {
        var byteValue = GetBitsFromValue(element, 8, BitsPerElement - offset - 8);
        return (byte)byteValue;
    }

    private static uint GetBitsFromValue<T>(T value, int bitCount, int remainingBitsInValue) where T : INumber<T>, IBinaryInteger<T>
    => uint.CreateTruncating((value >> remainingBitsInValue) & GetAllSetBitMask<T>(bitCount));
    private static T GetAllSetBitMask<T>(int bitCount) where T : INumber<T>, IBinaryInteger<T>
    {
        var maxBits = GetMaxBits<T>();
        var bitCountBits = T.CreateTruncating(bitCount);
        Debug.Assert(bitCountBits <= maxBits);
        return maxBits == bitCountBits ? T.AllBitsSet : unchecked((T.One << bitCount) - T.One);
    }

    private static T GetMaxBits<T>() where T : INumber<T>, IBinaryInteger<T>
    {
        var maxBitsForT = T.LeadingZeroCount(T.One) + T.One;
        Debug.Assert(T.IsPositive(maxBitsForT) || T.IsZero(maxBitsForT));
        return maxBitsForT;
    }

    private static void ThrowForInvalidValue<T>(T value, int bitsToUse) where T : INumber<T>, IBinaryInteger<T>
    {
        if (T.IsZero(value))
        {
            return;
        }

        var maxBitsForT = GetMaxBits<T>();
        if (T.CreateTruncating(bitsToUse) > maxBitsForT)
        {
            throw new ArgumentOutOfRangeException(nameof(bitsToUse), $"The maximum number of bits in a '{typeof(T).Name}' is '{maxBitsForT}', but '{bitsToUse}' was requested.");
        }

        var allSetMask = GetAllSetBitMask<T>(bitsToUse);
        var s1 = T.Sign(allSetMask);
        var s2 = T.Sign(value);
        if ((s1 != s2 ? T.Min(allSetMask, value) : T.Max(allSetMask, value)) != allSetMask)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Expected value to be less than or equal to 0x{allSetMask:x}, to fit within {bitsToUse}-bits, actual 0x{value:x}.");
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
            var currentBitCapactity = _buffer.Count << BitShiftPerElement;
            var newBitCapacity = Math.Max(currentBitCapactity, BitsPerElement);
            while (newBitCapacity < requestedCapacity)
            {
                newBitCapacity = (int)Math.Min(((long)newBitCapacity) * 2, int.MaxValue);
            };

            var intCapacity = GetInt32LengthFromBits(newBitCapacity);
            Debug.Assert(newBitCapacity % BitsPerElement == 0);
            _capacity = newBitCapacity;
            _buffer.EnsureCapacity(intCapacity);
            CollectionsMarshal.SetCount(_buffer, intCapacity);
        }
    }

    private static int GetInt32LengthFromBits(int bits) => (int)((uint)(bits - 1 + (1 << BitShiftPerElement)) >> BitShiftPerElement);
    private static int GetInt32LengthFromBitsFloor(int bits) => bits >> BitShiftPerElement;
    private static int GetElementLengthFromBytesCeil(int bytes) => (int)((uint)(bytes - 1 + (1 << (BytesShiftPerElement - 1))) >> (BytesShiftPerElement - 1));
    private static int GetElementLengthFromBytesFloor(int bytes) => bytes >> (BytesShiftPerElement - 1);
}
