using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

/// <summary>
/// A matrix of bytes that can be accessed by index, row, or column.
/// </summary>
[DebuggerTypeProxy(typeof(ByteMatrixDebugView))]
public class ByteMatrix
{
    private readonly byte[][] _matrixRowMajorOrder;
    private readonly byte[][] _matrixColumnMajorOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteMatrix"/> class with the specified width and height.
    /// </summary>
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

    /// <summary>
    /// The width of the matrix.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the matrix.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets or sets the byte at the specified <paramref name="x"/> and <paramref name="y"/> coordinates.
    /// </summary>
    /// <remarks>
    /// <paramref name="x"/> is left-to-right.and <paramref name="y"/> is top-to-bottom. Said another way, [0, 0] is the top-left corner of the matrix.
    /// </remarks>
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

    /// <summary>
    /// Returns a read-only view of the row at the specified y value.
    /// </summary>
    public ReadOnlySpan<byte> GetRow(int y)
    {
        if (unchecked((uint)y >= Height))
        {
            ThrowArgumentOutOfRangeException(nameof(y));
        }
        return _matrixRowMajorOrder[y];
    }

    /// <summary>
    /// Returns a read-only view of the column at the specified <paramref name="x"/> value.
    /// </summary>
    public ReadOnlySpan<byte> GetColumn(int x)
    {
        if (unchecked((uint)x >= Width))
        {
            ThrowArgumentOutOfRangeException(nameof(x));
        }
        return _matrixColumnMajorOrder[x];
    }

    /// <summary>
    /// Returns a value indicating if two <see cref="ByteMatrix"/> instances are equal.
    /// </summary>
    public static bool operator ==(ByteMatrix? left, ByteMatrix? right) => left is null ? right is null : left.Equals(right);
    /// <summary>
    /// Returns a value indicating if two <see cref="ByteMatrix"/> instances are not equal.
    /// </summary>
    public static bool operator !=(ByteMatrix? left, ByteMatrix? right) => !(left == right);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ByteMatrix other && Equals(other);
    /// <inheritdoc />
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

[ExcludeFromCodeCoverage]
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
                    sb.Append($"{_matrix[x, y]:X1}");
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
