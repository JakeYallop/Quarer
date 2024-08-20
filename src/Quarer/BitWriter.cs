using System.Numerics;

namespace Quarer;

public sealed class BitWriter
{
    private readonly int _startPosition;

    public BitWriter() : this(new())
    {
    }

    public BitWriter(BitBuffer writer) : this(writer, 0)
    {
    }

    public BitWriter(BitBuffer writer, int startPosition)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(startPosition);
        Buffer = writer;
        _startPosition = startPosition;
    }

    public BitBuffer Buffer { get; }
    public int BitsWritten { get; private set; }
    public int BytesWritten => GetByteCount(BitsWritten);

    public void WriteBitsBigEndian<T>(T value, int bitCount) where T : IBinaryInteger<T>
    {
        Buffer.WriteBitsBigEndian(_startPosition + BitsWritten, value, bitCount);
        BitsWritten += bitCount;
    }

    private const int Shift = 3;
    internal static int GetByteCount(int bits) => (int)((uint)(bits - 1 + (1 << Shift)) >> Shift);
}
