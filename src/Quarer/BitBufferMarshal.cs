using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Quarer;

public static class BitBufferMarshal
{
    /// <summary>
    /// Set the <see cref="Count"/> of this bitBuffer to the specified number of bits. If the underlying buffer is expanded, any new bits are
    /// set to zero.
    /// <para>
    /// Under certain conditions, additional non-zeroed data could be exposed. Writing to the buffer, then calling <c>SetCount</c>
    /// to shrink it will not zero-out the underlying storage used by the buffer.
    /// </para>
    /// </summary>
    /// <param name="bitCount"></param>
    public static void SetCount(BitBuffer bitBuffer, int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitCount);
        EnsureCapacity(bitBuffer, bitCount);
        SetBufferCount(bitBuffer, bitCount);
    }

    /// <summary>
    /// Reads a set of bytes from the buffer, starting from the byte offset given by <paramref name="start"/>.
    /// <para>
    /// On little-endian systems, (where <see cref="BitConverter.IsLittleEndian"/> returns <see langword="true"/>) the bytes need
    /// to be reversed to big-endian order, this requires some storage to be allocated. Prefer calling the
    /// <see cref="GetBytes(BitBuffer, int, int, Span{byte})"/> overload to avoid this allocation.
    /// </para>
    /// </summary>
    /// <param name="bitBuffer">The <see cref="BitBuffer"/> to read from.</param>
    /// <param name="start">The byte index to start from.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> GetBytes(BitBuffer bitBuffer, int start, int length)
    {
        if ((uint)start > bitBuffer.ByteCount || (uint)length > (uint)(bitBuffer.ByteCount - start))
        {
            throw new ArgumentOutOfRangeException(paramName: null, "The start byte and length exceed the buffer size.");
        }

        if (BitConverter.IsLittleEndian)
        {
            Debug.Assert(false, "Unnecessary allocation - check manually for endianness and call the appropriate overload.");
            return GetBytes(bitBuffer, start, length, new byte[length]);
        }
        return ReadBytesBigEndian(bitBuffer, start, length);
    }

    /// <summary>
    /// Reads a set of bytes from the buffer, starting from the byte offset given by <paramref name="start"/>.
    /// <para>
    /// On little-endian systems, (where <see cref="BitConverter.IsLittleEndian"/> returns <see langword="true"/>) the bytes need
    /// to be reversed to big-endian order, this requires some storage to be allocated. Prefer calling the
    /// <see cref="GetBytes(BitBuffer, int, int, Span{byte})"/> overload to avoid this allocation.
    /// </para>
    /// </summary>
    /// <param name="bitBuffer">The <see cref="BitBuffer"/> to read from.</param>
    /// <param name="start">The byte index to start from.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> GetBytes(BitBuffer bitBuffer, int start, int length, Span<byte> destination)
    {
        if ((uint)start > bitBuffer.ByteCount || (uint)length > (uint)(bitBuffer.ByteCount - start))
        {
            throw new ArgumentOutOfRangeException(paramName: null, "The start byte and length exceed the buffer size.");
        }

        if (destination.Length < length)
        {
            throw new ArgumentException("The storage span is too small.", nameof(destination));
        }

        if (!BitConverter.IsLittleEndian)
        {
            var data = ReadBytesBigEndian(bitBuffer, start, length);
            Debug.Assert(data.Length <= destination.Length);
            data.CopyTo(destination);
            return destination;
        }

        return ReadBytesLittleEndian(bitBuffer, start, length, destination);
    }

    internal static ReadOnlySpan<byte> ReadBytesBigEndian(BitBuffer bitBuffer, int start, int length)
    {
        var buffer = _buffer(bitBuffer);
        var span = CollectionsMarshal.AsSpan(buffer);
        ReadOnlySpan<byte> bytesSpan = MemoryMarshal.AsBytes(span);
        return bytesSpan.Slice(start, length);
    }

    internal static ReadOnlySpan<byte> ReadBytesLittleEndian(BitBuffer bitBuffer, int start, int length, Span<byte> destination)
    {
        Debug.Assert(destination.Length >= length, "Destination span is too small.");

        destination = destination[..length];

        if (length is 0)
        {
            return destination;
        }

        var bitBufferContents = _buffer(bitBuffer);
        var buffer = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(bitBufferContents));
        var written = 0;

        var unalignedStart = start & 7;
        if (unalignedStart > 0)
        {
            // we don't want to read more than `lengthInBytes` bytes or beyond the end of the element
            var bytesToRead = int.Min(BitBuffer.BytesPerElement - unalignedStart, length);
            var element = ReadElementContainingByte(buffer, start);
            written += ReadUnalignedBytes(element, unalignedStart, bytesToRead, destination);
        }

        if (written == length)
        {
            return destination;
        }

        var alignedStart = start + written;
        var (fullElementCount, unalignedTrailingBytes) = int.DivRem(length - written, BitBuffer.BytesPerElement);
        var alignedEndByte = fullElementCount * BitBuffer.BytesPerElement;

        if (length - written >= BitBuffer.BytesPerElement)
        {
            var fullElements = buffer.Slice(alignedStart, alignedEndByte);
            Debug.Assert(fullElements.Length % BitBuffer.BytesPerElement == 0, "Expected bytes count to be a multiple of 8 bytes.");
            Debug.Assert((alignedEndByte - alignedEndByte) % BitBuffer.BytesPerElement == 0, "Expected start/end aligned indexes to be along an 8 byte boundary.");
            var destinationSlice = destination[written..];
            fullElements.CopyTo(destinationSlice);
            var destinationSliceAsElement = MemoryMarshal.Cast<byte, ulong>(destinationSlice);
            BinaryPrimitives.ReverseEndianness(destinationSliceAsElement, destinationSliceAsElement);
            written += fullElements.Length;
        }

        if (written == length)
        {
            return destination;
        }

        Debug.Assert(length - written == unalignedTrailingBytes, "Expected the remaining bytes to be the same as the unaligned end bytes.");

        if (unalignedTrailingBytes > 0)
        {
            var endBytes = ReadElementContainingByte(buffer, start + written);
            written += ReadUnalignedBytes(endBytes, 0, unalignedTrailingBytes, destination[written..]);
        }

        Debug.Assert(written == length, "Expected to have written all the bytes.");

        return destination;
    }

    private static ReadOnlySpan<byte> ReadElementContainingByte(Span<byte> bytes, int index)
    {
        var byteIndex = BitBuffer.GetElementLengthFromBytesFloor(index) * BitBuffer.BytesPerElement;
        return bytes[byteIndex..(byteIndex + BitBuffer.BytesPerElement)];
    }

    private static int ReadUnalignedBytes(ReadOnlySpan<byte> element, int offset, int length, Span<byte> destination)
    {
        //For an 8-byte element with bytes MSB 0x07 | 0x06 | 0x05 | 0x04 | 0x03 | 0x02 | 0x01 | 0x00 LSB
        //
        // On a little-endian system, the bytes are stored in memory as:
        //
        //  [0]   [1]   [2]   [3]   [4]   [5]   [6]   [7]
        //  0x00  0x01  0x02  0x03  0x04  0x05  0x06  0x07
        //
        // MSB Byte 7 is at index [0], LSB Byte 0 is at index [7]
        // We want to read the bytes in big-endian order
        // 
        // For example, starting at byte 2 and reading 3 bytes should return
        // (2, 3) = 0x05, 0x04, 0x03
        // some other examples:
        // (1, 4) = 0x06, 0x05, 0x04, 0x03
        // (0, 7) = 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01
        // (5, 2) = 0x02, 0x01
        // (5, 3 =  0x02, 0x01, 0x00
        //
        // For (2, 3)
        //                                End = Start + bytesToRead - 1
        //                                 V
        //  [0]   [1]   [2]   [3]   [4]   [5]   [6]   [7]
        //  0x00  0x01  0x02  0x03  0x04  0x05  0x06  0x07
        //                     ^
        //                    Start = 8 - bytesToRead (3) - startIndex (2)
        //

        Debug.Assert(offset <= 7);
        var destinationSlice = destination[..length];
        var start = BitBuffer.BytesPerElement - length - offset;
        element.Slice(start, length).CopyTo(destinationSlice);
        destinationSlice.Reverse();
        return length;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void EnsureCapacity(BitBuffer @this, int bitCount);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Count")]
    private static extern void SetBufferCount(BitBuffer @this, int bitCount);

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref List<ulong> _buffer(BitBuffer @this);
}
