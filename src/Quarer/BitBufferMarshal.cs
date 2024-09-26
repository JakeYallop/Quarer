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
    /// <see cref="GetBytes(BitBuffer, Range, Span{byte})"/> overload to avoid this allocation.
    /// </para>
    /// </summary>
    /// <param name="bitBuffer">The <see cref="BitBuffer"/> to read from.</param>
    /// <param name="start">The byte index to start from.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> GetBytes(BitBuffer bitBuffer, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(bitBuffer.ByteCount);
        return GetBytes(bitBuffer, offset, length);
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

        var bytes = CollectionsMarshal.AsSpan(_buffer(bitBuffer));
        return bytes.Slice(start, length);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void EnsureCapacity(BitBuffer @this, int bitCount);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Count")]
    private static extern void SetBufferCount(BitBuffer @this, int bitCount);

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref List<byte> _buffer(BitBuffer @this);
}
