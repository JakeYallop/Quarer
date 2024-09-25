﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Quarer;

[DebuggerDisplay("Count = {ByteCount}")]
internal sealed class BitBufferDebugView
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly BitBuffer _bitBuffer;
    public BitBufferDebugView(BitBuffer bitWriter)
    {
        ArgumentNullException.ThrowIfNull(bitWriter);
        _bitBuffer = bitWriter;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    public static extern ref List<ulong> _buffer(BitBuffer @this);

    public int Count => _bitBuffer.Count;
    public int ByteCount => _bitBuffer.ByteCount;

    public byte[] ByteView
    {
        get
        {
            var bytes = new byte[_bitBuffer.ByteCount];
            BitBufferMarshal.GetBytes(_bitBuffer, 0, _bitBuffer.ByteCount, bytes);
            return bytes;
        }
    }

    public string BitView
    {
        get
        {
            var sb = new StringBuilder(_bitBuffer.Count);
            foreach (var i in _buffer(_bitBuffer))
            {
                sb.AppendFormat("{0:B32}", i);
            }
            return sb.ToString(0, _bitBuffer.Count);
        }
    }
}

[DebuggerTypeProxy(typeof(BitBufferDebugView))]
public sealed class BitBuffer : IEquatable<BitBuffer>
{
    internal const int BitsPerElement = sizeof(ulong) * 8;
    internal const int BitShiftPerElement = 6;
    internal const int BytesPerElement = sizeof(ulong);
    internal const int BytesShiftPerElement = 3;

    private readonly List<ulong> _buffer;

    public BitBuffer()
    {
        _buffer = new(2);
    }

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

    public bool this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var elementIndex = GetElementLengthFromBitsFloor(index);
            var bitIndex = index & (BitsPerElement - 1);
            return ReadBit(_buffer[elementIndex], bitIndex);
        }

        set
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var elementIndex = GetElementLengthFromBitsFloor(index);
            var bitIndex = index & (BitsPerElement - 1);
            var mask = GetBitMask(bitIndex);
            if (value)
            {
                _buffer[elementIndex] |= mask;
            }
            else
            {
                _buffer[elementIndex] &= ~mask;
            }
        }
    }

    public void WriteBitsBigEndian<T>(int position, T value, int bitCount) where T : IBinaryInteger<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 0);
        ThrowForInvalidValue(value, bitCount);
        var remainingBitsInValue = bitCount;
        EnsureCapacity(checked(position + remainingBitsInValue));
        var elementPosition = GetElementLengthFromBitsFloor(position);

        var bitIndex = position & (BitsPerElement - 1);
        var buffer = CollectionsMarshal.AsSpan(_buffer);

        while (remainingBitsInValue > 0)
        {
            var remainingBitsForCurrentPosition = BitsPerElement - bitIndex;
            var bitsToWrite = Math.Min(remainingBitsInValue, remainingBitsForCurrentPosition);
            remainingBitsInValue -= bitsToWrite;

            ref var segment = ref buffer[elementPosition];
            var bitsInValue = GetBitsFromValue(value, bitsToWrite, remainingBitsInValue);
            segment &= GetClearBitsMask<ulong>(bitIndex, bitsToWrite);
            segment |= bitsInValue << (BitsPerElement - bitsToWrite - bitIndex);
            elementPosition += 1;
            bitIndex = 0;
            Debug.Assert(remainingBitsInValue == 0 || (position + bitsToWrite) % BitsPerElement == 0, $"Expected to be aligned to a {BitsPerElement}-bit boundary.");
        }

        Count = Math.Max(Count, position + bitCount);
    }

    public IEnumerable<bool> AsBitEnumerable()
    {
        //TODO: Return an enumerable that implements ICollection so that TryGetNonEnumeratedCount etc works
        var current = 0;
        foreach (var i in _buffer)
        {
            for (var currentBit = 1; currentBit <= BitsPerElement; currentBit++)
            {
                if (current == Count)
                {
                    yield break;
                }

                var mask = 1UL << (BitsPerElement - currentBit);
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
    public IEnumerable<byte> AsByteEnumerable()
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

    private static ulong GetBitsFromValue<T>(T value, int bitCount, int remainingBitsInValue) where T : IBinaryInteger<T>
        => ulong.CreateTruncating((value >> remainingBitsInValue) & GetAllSetBitMask<T>(bitCount));

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
        if (requestedCapacity > _buffer.Count << BitShiftPerElement)
        {
            var currentBitCapactity = _buffer.Count << BitShiftPerElement;
            var newBitCapacity = Math.Max(currentBitCapactity, BitsPerElement);
            while (newBitCapacity < requestedCapacity)
            {
                newBitCapacity = (int)Math.Min(((long)newBitCapacity) * 2, int.MaxValue);
            };
            Debug.Assert(newBitCapacity % BitsPerElement == 0);

            var bufferCapacity = GetElementLengthFromBitsCeil(newBitCapacity);
            CollectionsMarshal.SetCount(_buffer, bufferCapacity);
        }
    }

    private static bool ReadBit(ulong element, int offset) => (element & GetBitMask(offset)) != 0;
    private static ulong GetBitMask(int bitIndex) => 1UL << (BitsPerElement - bitIndex - 1);

    public static bool operator ==(BitBuffer? left, BitBuffer? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(BitBuffer? left, BitBuffer? right) => !(left == right);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BitBuffer other && Equals(other);
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

        var buffer = CollectionsMarshal.AsSpan(_buffer);
        var otherBuffer = CollectionsMarshal.AsSpan(other._buffer);

        if (((ulong)Count & (BitsPerElement - 1)) == 0u)
        {
            return buffer.SequenceEqual(otherBuffer);
        }

        var elementCount = GetElementLengthFromBitsCeil(Count);
        if (!buffer[..(elementCount - 1)].SequenceEqual(otherBuffer[..(elementCount - 1)]))
        {
            return false;
        }

        var finalElement = buffer[elementCount - 1];
        var otherFinalElement = otherBuffer[elementCount - 1];

        var bitsInFinalElement = Count & (BitsPerElement - 1);
        var remainingLength = bitsInFinalElement;

        Debug.Assert(remainingLength < BitsPerElement, "Expected remaining length to be less than BitsPerElement, as any length above it should have already been handled.");
        if (remainingLength >= 32)
        {
            const int ShiftForUInt = BitsPerElement - 32;
            if (uint.CreateTruncating(finalElement >> ShiftForUInt) != uint.CreateTruncating(otherFinalElement >> ShiftForUInt))
            {
                return false;
            }
            finalElement <<= 32;
            otherFinalElement <<= 32;
            remainingLength -= 32;
        }
        Debug.Assert(remainingLength < 32, "Expected remaining length to be < 32, as any length above it should have already been handled.");

        if (remainingLength >= 16)
        {
            const int ShiftForUshort = BitsPerElement - 16;
            if (ushort.CreateTruncating(finalElement >> ShiftForUshort) != ushort.CreateTruncating(otherFinalElement >> ShiftForUshort))
            {
                return false;
            }
            finalElement <<= 16;
            otherFinalElement <<= 16;
            remainingLength -= 16;
        }
        Debug.Assert(remainingLength < 16, "Expected remaining length to be < 16, as any length above it should have already been handled.");

        if (remainingLength >= 8)
        {
            const int ShiftForByte = BitsPerElement - 8;
            if (byte.CreateTruncating(finalElement >> ShiftForByte) != byte.CreateTruncating(otherFinalElement >> ShiftForByte))
            {
                return false;
            }
            finalElement <<= 8;
            otherFinalElement <<= 8;
            remainingLength -= 8;
        }
        Debug.Assert(remainingLength < 8, "Expected remaining length to be < 8, as any length above it should have already been handled.");

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

    public override int GetHashCode()
    {
        var buffer = CollectionsMarshal.AsSpan(_buffer);
        var hashCode = new HashCode();

        if (((uint)Count & (BitsPerElement - 1)) == 0u)
        {
            foreach (var i in buffer)
            {
                hashCode.Add(i);
            }
            return hashCode.ToHashCode();
        }

        var elementCount = GetElementLengthFromBitsCeil(Count);
        var fullElements = buffer[..(elementCount - 1)];
        foreach (var i in fullElements)
        {
            hashCode.Add(i);
        }

        var finalElement = buffer[elementCount - 1];

        var bitsInFinalElement = Count & (BitsPerElement - 1);
        var remainingLength = bitsInFinalElement;

        Debug.Assert(remainingLength < BitsPerElement, "Expected remaining length to be less than BitsPerElement, as any length above it should have already been handled.");
        if (remainingLength >= 32)
        {
            const int ShiftForUint = BitsPerElement - 32;
            hashCode.Add(ushort.CreateTruncating(finalElement >> ShiftForUint));
            finalElement <<= 32;
            remainingLength -= 32;
        }
        Debug.Assert(remainingLength < 32, "Expected remaining length to be < 32, as any length above it should have already been handled.");

        if (remainingLength >= 16)
        {
            const int ShiftForUshort = BitsPerElement - 16;
            hashCode.Add(ushort.CreateTruncating(finalElement >> ShiftForUshort));
            finalElement <<= 16;
            remainingLength -= 16;
        }
        Debug.Assert(remainingLength < 16, "Expected remaining length to be < 16, as any length above it should have already been handled.");

        if (remainingLength >= 8)
        {
            const int ShiftForByte = BitsPerElement - 8;
            hashCode.Add(byte.CreateTruncating(finalElement >> ShiftForByte));
            finalElement <<= 8;
            remainingLength -= 8;
        }
        Debug.Assert(remainingLength < 8, "Expected remaining length to be < 8, as any length above it should have already been handled.");

        for (var i = 0; i < remainingLength; i++)
        {
            var bit = ReadBit(finalElement, i);
            hashCode.Add(bit);
        }

        return hashCode.ToHashCode();
    }

    internal static int GetElementLengthFromBitsCeil(int bits) => (int)((uint)(bits - 1 + (1u << BitShiftPerElement)) >> BitShiftPerElement);
    internal static int GetElementLengthFromBitsFloor(int bits) => bits >> BitShiftPerElement;
    internal static int GetElementLengthFromBytesCeil(int bytes) => (int)((uint)(bytes - 1 + (1u << BytesShiftPerElement)) >> BytesShiftPerElement);
    internal static int GetElementLengthFromBytesFloor(int bytes) => bytes >> BytesShiftPerElement;
}
