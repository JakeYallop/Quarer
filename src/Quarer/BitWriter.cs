using System.Numerics;

namespace Quarer;

/// <summary>
/// Provides an abstraction over a <see cref="BitBuffer"/> that allows for sequentially writing bits to it.
/// </summary>
public sealed class BitWriter
{
    private readonly int _startPosition;

    /// <summary>
    /// Constructs a new instance of the <see cref="BitWriter"/> class. Wraps a new, empty <see cref="BitBuffer"/>.
    /// </summary>
    public BitWriter() : this(new())
    {
    }

    /// <summary>
    /// Constructs a new instance of the <see cref="BitWriter"/> class, wrapping the provided <see cref="BitBuffer"/>.
    /// </summary>
    public BitWriter(BitBuffer buffer) : this(buffer, 0)
    {
    }

    /// <summary>
    /// Constructs a new instance of the <see cref="BitWriter"/> class, wrapping the provided <see cref="BitBuffer"/>, starting at the specified bit position.
    /// </summary>
    public BitWriter(BitBuffer writer, int startPosition)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(startPosition);
        Buffer = writer;
        _startPosition = startPosition;
    }

    /// <summary>
    /// The underlying <see cref="BitBuffer"/>.
    /// </summary>
    public BitBuffer Buffer { get; }
    /// <summary>
    /// The number of bits written to the underlying buffer.
    /// </summary>
    public int BitsWritten { get; private set; }
    /// <summary>
    /// The number of bytes written to the underlying buffer, rounded up to the closest full byte.
    /// </summary>
    public int BytesWritten => GetByteCount(BitsWritten);

    /// <summary>
    /// Writes a value to the underlying <see cref="BitBuffer"/> in big endian order.
    /// <para>
    /// Logically, this means for a value <c>111000</c> written to an empty buffer: <c>buffer[0] == 1</c>, <c>buffer[3] == 0</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The numeric type to write.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="bitCount">The number of bits from <paramref name="value"/> to use. For example, using <paramref name="value"/>: 0 and <paramref name="bitCount"/>: 10, we would write 10 <c>0</c> bits to the buffer.</param>
    public void WriteBitsBigEndian<T>(T value, int bitCount) where T : IBinaryInteger<T>
    {
        Buffer.WriteBitsBigEndian(_startPosition + BitsWritten, value, bitCount);
        BitsWritten += bitCount;
    }

    private const int Shift = 3;
    private static int GetByteCount(int bits) => (int)((uint)(bits - 1 + (1 << Shift)) >> Shift);
}
