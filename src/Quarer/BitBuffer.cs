using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Quarer;

/// <summary>
/// Holds a collection of bits, allowing for efficient read and write operations.
/// </summary>
public sealed class BitBuffer : IEquatable<BitBuffer>
{
    internal const int BitsPerElement = 8;

    private readonly List<byte> _buffer;

    /// <summary>
    /// Creates a new instance of the <see cref="BitBuffer"/> class.
    /// </summary>
    public BitBuffer()
    {
        _buffer = new(2);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BitBuffer"/> class that can hold the specified number of bits before requiring a resize.
    /// </summary>
    /// <param name="initialBitCapacity"></param>
    public BitBuffer(int initialBitCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialBitCapacity);
        _buffer = new(GetElementLengthFromBitsCeil(initialBitCapacity));
    }

    /// <summary>
    /// The total number of bits currently in this buffer.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// The total number of bytes in this buffer, rounded up to the nearest full byte.
    /// </summary>
    public int ByteCount
    {
        get
        {
            var (fullElements, remainder) = int.DivRem(Count, 8);
            return fullElements + (remainder > 0 ? 1 : 0);
        }
    }

    /// <summary>
    /// Gets or sets the bit at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var elementIndex = GetElementIndexFromBitsFloor(index);
            var bitIndex = GetBitIndex(index);
            return ReadBit(_buffer[elementIndex], bitIndex);
        }

        set
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var elementIndex = GetElementIndexFromBitsFloor(index);
            var bitIndex = GetBitIndex(index);
            var mask = GetBitMask(bitIndex);
            if (value)
            {
                _buffer[elementIndex] |= mask;
            }
            else
            {
                // we can't just use ~mask as this causes an overflow (as ~ uses the int~ operator, not the byte one)
                _buffer[elementIndex] &= op_OnesComplement(mask);
            }

            static T op_OnesComplement<T>(T value) where T : IBinaryInteger<T> => ~value;
        }
    }

    private static int GetBitIndex(int index) => index & (BitsPerElement - 1);

    /// <summary>
    /// Writes a value to this <see cref="BitBuffer"/> at the specified position, in big endian order.
    /// <para>
    /// Logically, this means for a value <c>111000</c> written to an empty buffer at position 0: <c>buffer[0] == 1</c>, <c>buffer[3] == 0</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The numeric type to write.</typeparam>
    /// <param name="position">The start position.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="bitCount">The number of bits from <paramref name="value"/> to use. For example, using <paramref name="value"/>: 0 and <paramref name="bitCount"/>: 10, we would write 10 <c>0</c> bits to the buffer.</param>
    public void WriteBitsBigEndian<T>(int position, T value, int bitCount) where T : IBinaryInteger<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 0);
        ThrowForInvalidValue(value, bitCount);
        var remainingBitsInValue = bitCount;
        EnsureCapacity(checked(position + remainingBitsInValue));
        var (elementPosition, bitIndex) = (GetElementIndexFromBitsFloor(position), GetBitIndex(position));
        var buffer = CollectionsMarshal.AsSpan(_buffer);

        while (remainingBitsInValue > 0)
        {
            var remainingBitsForCurrentPosition = BitsPerElement - bitIndex;
            var bitsToRead = int.Min(remainingBitsForCurrentPosition, remainingBitsInValue);
            remainingBitsInValue -= bitsToRead;

            ref var segment = ref buffer[elementPosition];
            var bitsFromValue = GetBitsFromValue(value, bitsToRead, remainingBitsInValue);
            segment &= GetClearBitsMask<byte>(bitIndex, bitsToRead);
            segment |= (byte)(bitsFromValue << (8 - bitsToRead - bitIndex));
            elementPosition++;
            bitIndex = 0;
        }

        Count = Math.Max(Count, position + bitCount);
    }

    /// <summary>
    /// Returns bytes from this <see cref="BitWriter"/>. The number of bytes returned is rounded up to the nearest full byte,
    /// with the final byte padded with zeros if necessary
    /// </summary>
    /// <returns></returns>
    public IEnumerable<byte> AsByteEnumerable()
    {
        // do not use foreach here in case Count changes.
        for (var i = 0; i < GetElementLengthFromBitsCeil(Count); i++)
        {
            yield return _buffer[i];
        }
    }

    /// <summary>
    /// Ensure this buffer has space to store <paramref name="capacity"/> bits.
    /// </summary>
    /// <param name="capacity"></param>
    public void SetCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity < Count)
        {
            throw new ArgumentException("Cannot set capacity to less than the current count.", nameof(capacity));
        }

        var oldCapacity = _buffer.Capacity;
        EnsureCapacity(capacity);
        var newCapacity = GetElementLengthFromBitsCeil(capacity);
        if (newCapacity < oldCapacity)
        {
            // we can just use TrimExcess here, as we use CollectionsMarshal.SetCount inside EnsureCapacity
            _buffer.TrimExcess();
        }
    }

    /// <summary>
    /// Copy this <see cref="BitBuffer"/> to the destination <see cref="BitBuffer"/>.
    /// </summary>
    /// <param name="destination"></param>
    /// <exception cref="ArgumentException"></exception>
    public void CopyTo(BitBuffer destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (Count > destination.Count)
        {
            throw new ArgumentException("The destination buffer is too short.", nameof(destination));
        }
        var source = CollectionsMarshal.AsSpan(_buffer);
        var destinationBuffer = CollectionsMarshal.AsSpan(destination._buffer);
        source.CopyTo(destinationBuffer);
    }

    private static byte GetBitsFromValue<T>(T value, int bitCount, int remainingBitsInValue) where T : IBinaryInteger<T>
        => byte.CreateTruncating((value >> remainingBitsInValue) & GetAllSetBitMask<T>(bitCount));

    private static T GetAllSetBitMask<T>(int bitCount) where T : IBinaryInteger<T>
    {
        var maxBits = GetMaxBits<T>();
        var bitCountBits = T.CreateTruncating(bitCount);
        Debug.Assert(bitCountBits <= maxBits);
        return maxBits == bitCountBits ? T.AllBitsSet : unchecked((T.One << bitCount) - T.One);
    }

    private static T GetMaxBits<T>() where T : IBinaryInteger<T>
    {
        var maxBitsForT = T.LeadingZeroCount(T.One) + T.One;
        Debug.Assert(T.IsPositive(maxBitsForT) || T.IsZero(maxBitsForT));
        return maxBitsForT;
    }

    private static T GetClearBitsMask<T>(int bitIndex, int length) where T : IBinaryInteger<T>
    {
        Debug.Assert(int.CreateChecked(GetMaxBits<T>()) == BitsPerElement);
        // Example:
        // bitIndex = 3
        // length = 4
        // m = AllBitsSet >> (12 - 4)
        // m = 0000_0000_1111
        // m = m << (12 - 4 - 3)
        // m = 0001_1110_0000
        // ~m
        // 1110_0001_1111

        var mask = GetAllSetBitMask<T>(BitsPerElement);
        mask >>= BitsPerElement - length;
        mask <<= BitsPerElement - length - bitIndex;
        return ~mask;
    }

    private static void ThrowForInvalidValue<T>(T value, int bitsToUse) where T : IBinaryInteger<T>
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

    private void EnsureCapacity(int requestedCapacity)
    {
        var currentBitCapacity = _buffer.Count * BitsPerElement;
        if (requestedCapacity > currentBitCapacity)
        {
            var newBitCapacity = Math.Max(currentBitCapacity, BitsPerElement);
            while (newBitCapacity < requestedCapacity)
            {
                newBitCapacity = (int)Math.Min(((long)newBitCapacity) * 2, int.MaxValue);
            };
            Debug.Assert(newBitCapacity % BitsPerElement == 0);

            var bufferCapacity = GetElementLengthFromBitsCeil(newBitCapacity);
            CollectionsMarshal.SetCount(_buffer, bufferCapacity);
        }
    }

    private static bool ReadBit(byte element, int offset) => (element & GetBitMask(offset)) != 0;
    private static byte GetBitMask(int bitIndex) => (byte)(1 << (BitsPerElement - bitIndex - 1));

    /// <summary>
    /// Returns a value indicating whether two <see cref="BitBuffer" /> instances are equal.
    /// </summary>
    public static bool operator ==(BitBuffer? left, BitBuffer? right) => left is null ? right is null : left.Equals(right);
    /// <summary>
    /// Returns a value indicating whether two <see cref="BitBuffer" /> instances are not equal.
    /// </summary>
    public static bool operator !=(BitBuffer? left, BitBuffer? right) => !(left == right);
    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BitBuffer other && Equals(other);
    /// <inheritdoc />
    public bool Equals(BitBuffer? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (Count == 0)
        {
            return true;
        }

        var buffer = CollectionsMarshal.AsSpan(_buffer);
        var otherBuffer = CollectionsMarshal.AsSpan(other._buffer);

        var byteLength = GetElementIndexFromBitsFloor(Count);
        var fullElements = buffer[..byteLength];
        var otherFullElements = otherBuffer[..byteLength];
        if (byteLength > 0 && !fullElements.SequenceEqual(otherFullElements))
        {
            return false;
        }

        var remainingLength = Count - (byteLength * 8);
        if (remainingLength == 0)
        {
            return true;
        }

        var finalElement = buffer[byteLength];
        var otherFinalElement = otherBuffer[byteLength];
        for (var i = 0; i < remainingLength; i++)
        {
            var bit = ReadBit(finalElement, i);
            var otherBit = ReadBit(otherFinalElement, i);
            if (bit != otherBit)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var buffer = CollectionsMarshal.AsSpan(_buffer);
        var hashCode = new HashCode();
        hashCode.Add(Count);

        if (Count <= 0)
        {
            return hashCode.ToHashCode();
        }

        var fullElementCount = GetElementIndexFromBitsFloor(Count);
        var fullElements = buffer[..fullElementCount];
        hashCode.AddBytes(fullElements);

        var bitsInFinalElement = GetBitIndex(Count);
        if (bitsInFinalElement > 0)
        {
            var finalElement = buffer[fullElementCount];
            var remainingLength = bitsInFinalElement;

            for (var i = 0; i < remainingLength; i++)
            {
                var bit = ReadBit(finalElement, i);
                hashCode.Add(bit);
            }
        }

        return hashCode.ToHashCode();
    }

    internal static int GetElementLengthFromBitsCeil(int bits) => (bits + 7) >> 3;
    internal static int GetElementIndexFromBitsFloor(int bits) => bits >> 3;
}
