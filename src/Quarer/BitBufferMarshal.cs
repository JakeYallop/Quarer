using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Quarer;
/// <summary>
/// An unsafe class that enables manipulation and direct access to the internals of a <see cref="BitBuffer"/>.
/// </summary>
public static class BitBufferMarshal
{
    /// <summary>
    /// Set the <see cref="BitBuffer.Count"/> of this bitBuffer to the specified number of bits. If the underlying buffer is expanded, any new bits are
    /// set to zero.
    /// <para>
    /// Under certain conditions, additional non-zeroed data could be exposed. Writing to the buffer, then calling <c>SetCount</c>
    /// to shrink it will not zero-out the underlying storage used by the buffer.
    /// </para>
    /// </summary>
    public static void SetCount(BitBuffer bitBuffer, int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitCount);
        EnsureCapacity(bitBuffer, bitCount);
        SetBufferCount(bitBuffer, bitCount);
    }

    /// <summary>
    /// Reads a set of bytes from the provided <see cref="BitBuffer"/>, spanning the provided range.
    /// </summary>
    public static ReadOnlySpan<byte> GetBytes(BitBuffer bitBuffer, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(bitBuffer.ByteCount);
        return GetBytes(bitBuffer, offset, length);
    }

    /// <summary>
    /// Reads a set of bytes from the provided <see cref="BitBuffer"/>, starting from the byte offset given by <paramref name="start"/>.
    /// </summary>
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
