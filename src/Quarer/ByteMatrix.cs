using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

[DebuggerTypeProxy(typeof(ByteMatrixDebugView))]
public class ByteMatrix
{
    private readonly byte[][] _matrixRowMajorOrder;
    private readonly byte[][] _matrixColumnMajorOrder;

    public ByteMatrix(int width, int height)
    {
        Width = width;
        Height = height;
        _matrixRowMajorOrder = new byte[height][];
        _matrixColumnMajorOrder = new byte[width][];
        for (var i = 0; i < height; i++)
        {
            _matrixRowMajorOrder[i] = new byte[width];
        }

        for (var i = 0; i < width; i++)
        {
            _matrixColumnMajorOrder[i] = new byte[height];
        }
    }

    public int Width { get; }
    public int Height { get; }
    public virtual byte this[int x, int y]
    {
        get
        {
            if (unchecked((uint)x >= Width))
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            if (unchecked((uint)y >= Height))
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            return _matrixRowMajorOrder[y][x];
        }

        set
        {
            if (unchecked((uint)x >= Width))
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            if (unchecked((uint)y >= Height))
            {
                ThrowArgumentOutOfRangeException(nameof(x));
            }

            _matrixRowMajorOrder[y][x] = value;
            _matrixColumnMajorOrder[x][y] = value;
        }
    }

    [DoesNotReturn]
    private static void ThrowArgumentOutOfRangeException(string paramName) => throw new ArgumentOutOfRangeException(paramName);

    public ReadOnlySpan<byte> GetRow(int row)
    {
        if (unchecked((uint)row >= Height))
        {
            ThrowArgumentOutOfRangeException(nameof(row));
        }
        return _matrixRowMajorOrder[row];
    }

    public ReadOnlySpan<byte> GetColumn(int column)
    {
        if (unchecked((uint)column >= Width))
        {
            ThrowArgumentOutOfRangeException(nameof(column));
        }
        return _matrixColumnMajorOrder[column];
    }

    public static bool operator ==(ByteMatrix? left, ByteMatrix? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(ByteMatrix? left, ByteMatrix? right) => !(left == right);
    public bool Equals([NotNullWhen(true)] ByteMatrix? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || Width != other.Width || Height != other.Height)
        {
            return false;
        }

        if (Width < Height)
        {
            for (var i = 0; i < _matrixRowMajorOrder.Length; i++)
            {
                var equal = _matrixRowMajorOrder[i].AsSpan().SequenceEqual(other._matrixRowMajorOrder[i].AsSpan());
                if (!equal)
                {
                    return false;
                }
            }
        }
        else
        {
            for (var i = 0; i < _matrixColumnMajorOrder.Length; i++)
            {
                var equal = _matrixColumnMajorOrder[i].AsSpan().SequenceEqual(other._matrixColumnMajorOrder[i].AsSpan());
                if (!equal)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ByteMatrix other && Equals(other);
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Width);
        hashCode.Add(Height);

        if (Width < Height)
        {
            for (var i = 0; i < _matrixRowMajorOrder.Length; i++)
            {
                hashCode.AddBytes(_matrixRowMajorOrder[i]);
            }
        }
        else
        {
            for (var i = 0; i < _matrixColumnMajorOrder.Length; i++)
            {
                hashCode.AddBytes(_matrixColumnMajorOrder[i]);
            }
        }
        return hashCode.ToHashCode();
    }
}

internal sealed class ByteMatrixDebugView(ByteMatrix byteMatrix)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ByteMatrix _matrix = byteMatrix;

    public string Matrix
    {
        get
        {
            var sb = new StringBuilder(_matrix.Width * _matrix.Height);
            for (var y = 0; y < _matrix.Height; y++)
            {
                for (var x = 0; x < _matrix.Width; x++)
                {
                    sb.Append($"{_matrix[x, y]:X2}");
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
}
