using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

[DebuggerTypeProxy(typeof(BitMatrixDebugView))]
public class BitMatrix : IEquatable<BitMatrix>
{
    private readonly BitBuffer _matrix;
    public BitMatrix(int width, int height)
    {
        Width = width;
        Height = height;
        _matrix = new BitBuffer(width * height);
        _matrix.SetCountUnsafe(width * height);
    }

    public int Width { get; }
    public int Height { get; }
    public virtual bool this[int x, int y]
    {
        get
        {
            if ((uint)x >= Width)
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            if ((uint)y >= Height)
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            return _matrix[x + (y * Width)];
        }

        set
        {
            if ((uint)x >= Width)
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            if ((uint)y >= Height)
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            _matrix[x + (y * Width)] = value;
        }
    }

    [DoesNotReturn]
    private void ThrowArgumentOutOfRangeException(string paramName) => throw new ArgumentOutOfRangeException(paramName);

    //TODO: Provide an indexable read only view into the matrix so we avoid copying here. We shouldn't return a BitBuffer as its writeable.
    public virtual BitBuffer GetRow(int row)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, Height - 1);

        var buffer = new BitBuffer(Width);
        buffer.SetCountUnsafe(Width);

        for (var i = 0; i < Width; i++)
        {
            buffer[i] = this[i, row];
        }

        return buffer;
    }

    //TODO: Provide an indexable read only view into the matrix so we avoid copying here. We shouldn't return a BitBuffer as its writeable.
    public virtual BitBuffer GetColumn(int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(column, Width - 1);

        var buffer = new BitBuffer(Height);
        buffer.SetCountUnsafe(Height);
        for (var i = 0; i < Height; i++)
        {
            buffer[i] = this[column, i];
        }
        return buffer;
    }

    public static bool operator ==(BitMatrix? left, BitMatrix? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(BitMatrix? left, BitMatrix? right) => !(left == right);
    public bool Equals([NotNullWhen(true)] BitMatrix? other) => other is not null &&
        Width == other.Width &&
        Height == other.Height &&
        _matrix == other._matrix;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BitMatrix other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height, _matrix);
}

internal sealed class BitMatrixDebugView(BitMatrix bitMatrix)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly BitMatrix _matrix = bitMatrix;

    public string Matrix
    {
        get
        {
            var sb = new StringBuilder(_matrix.Width * _matrix.Height);
            for (var y = 0; y < _matrix.Height; y++)
            {
                for (var x = 0; x < _matrix.Width; x++)
                {
                    sb.Append(_matrix[x, y] ? 'X' : '-');
                    if (x + 1 < _matrix.Width)
                    {
                        sb.Append(' ');
                    }
                }

                if (y + 1 < _matrix.Height)
                {
                    sb.AppendLine();
                }
            }
            var s = sb.ToString();
            sb.Clear();
            return s;
        }
    }

    public override string ToString() => base.ToString()!;
}
