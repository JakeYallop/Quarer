using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Quarer;

/// <summary>
/// An unsafe class that enables manipulation and direct access to the internals of a <see cref="ByteMatrix"/>.
/// </summary>
public static class ByteMatrixMarshal
{
    /// <summary>
    /// Get a writable span of the row-major representation of the byte matrix. Be sure to call <see cref="UpdateColumnMajorOrder"/>
    /// after modifying the span to ensure the internal representation of the byte matrix is kept up to date.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public static Span<byte> GetWritableRow(ByteMatrix matrix, int row)
    {
        if (unchecked((uint)row >= matrix.Height))
        {
            ThrowArgumentOutOfRangeException(nameof(row));
        }
        return _matrixRowMajorOrder(matrix)[row];
    }

    /// <summary>
    /// Get a writable span of the column-major representation of the byte matrix. Be sure to call <see cref="UpdateRowMajorOrder"/>
    /// after modifying the span to ensure the internal representation of the byte matrix is kept up to date.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static Span<byte> GetWritableColumn(ByteMatrix matrix, int column)
    {
        if (unchecked((uint)column >= matrix.Width))
        {
            ThrowArgumentOutOfRangeException(nameof(column));
        }
        return _matrixColumnMajorOrder(matrix)[column];
    }

    /// <summary>
    /// Updates the column-major representation of the byte matrix with the data provided for the given <paramref name="row"/>.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="row"></param>
    /// <param name="offset"></param>
    /// <param name="data"></param>
    public static void UpdateColumnMajorOrder(ByteMatrix matrix, int row, int offset, ReadOnlySpan<byte> data)
    {
        var length = offset + data.Length;
        var m = _matrixColumnMajorOrder(matrix);
        for (var i = offset; i < length; i++)
        {
            m[i][row] = data[i - offset];
        }
    }

    /// <summary>
    /// Updates the row-major representation of the byte matrix with the data provided for the given <paramref name="column"/>.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="column"></param>
    /// <param name="offset"></param>
    /// <param name="data"></param>
    public static void UpdateRowMajorOrder(ByteMatrix matrix, int column, int offset, ReadOnlySpan<byte> data)
    {
        var length = offset + data.Length;
        var m = _matrixRowMajorOrder(matrix);
        for (var i = offset; i < length; i++)
        {
            m[i][column] = data[i - offset];
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref byte[][] _matrixRowMajorOrder(ByteMatrix @this);

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref byte[][] _matrixColumnMajorOrder(ByteMatrix @this);

    [DoesNotReturn]
    private static void ThrowArgumentOutOfRangeException(string paramName) => throw new ArgumentOutOfRangeException(paramName);
}
