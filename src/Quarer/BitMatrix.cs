﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

internal sealed class BitMatrixDebugView(BitMatrix bitMatrix)
{
    private readonly BitMatrix _bitMatrix = bitMatrix;

    public string Matrix
    {
        get
        {
            var sb = new StringBuilder(_bitMatrix.Width * _bitMatrix.Height);
            for (var y = 0; y < _bitMatrix.Height; y++)
            {
                for (var x = 0; x < _bitMatrix.Width; x++)
                {
                    sb.Append(_bitMatrix[x, y] ? 'X' : '-');
                    if (x + 1 < _bitMatrix.Width)
                    {
                        sb.Append(' ');
                    }
                }

                if (y + 1 < _bitMatrix.Height)
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

[DebuggerTypeProxy(typeof(BitMatrixDebugView))]
public class BitMatrix : IEquatable<BitMatrix>
{
    private readonly BitBuffer[] _values;

    // I don't love this, but its the only way to get the values out of the matrix without copying
    // and without exposing the underlying BitBuffers publicly
    protected internal static BitBuffer[] GetValues(BitMatrix bitMatrix) => bitMatrix._values;
    protected internal BitMatrix(BitBuffer[] values, int width, int height)
    {
        _values = values;
        Width = width;
        Height = height;
    }

    public BitMatrix(int width, int height)
    {
        Width = width;
        Height = height;
        _values = Init(width, height);

        static BitBuffer[] Init(int width, int height)
        {
            var arr = new BitBuffer[height];
            for (var i = 0; i < height; i++)
            {
                var buffer = new BitBuffer(width);
                arr[i] = buffer;
                buffer.SetCountUnsafe(width);
            }

            return arr;
        }
    }

    public int Width { get; }
    public int Height { get; }
    public virtual bool this[int x, int y]
    {
        get => _values[y][x];
        set => _values[y][x] = value;
    }

    //TODO: Provide an indexable read only view into the matrix so we avoid copying here. We shouldn't return a BitBuffer as its writeable.
    public virtual BitBuffer GetRow(int row)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, Height - 1);

        var buffer = new BitBuffer(Width);
        buffer.SetCountUnsafe(Width);
        _values[row].CopyTo(buffer);
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
            buffer[i] = _values[i][column];
        }
        return buffer;
    }

    public virtual BitMatrix Clone()
    {
        var buffers = new BitBuffer[_values.Length];
        for (var i = 0; i < _values.Length; i++)
        {
            buffers[i] = new BitBuffer(_values[i].Count);
            buffers[i].SetCountUnsafe(Width);
            _values[i].CopyTo(buffers[i]);
        }
        return new BitMatrix(buffers, Width, Height);
    }

    public static bool operator ==(BitMatrix? left, BitMatrix? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(BitMatrix? left, BitMatrix? right) => !(left == right);
    public bool Equals([NotNullWhen(true)] BitMatrix? other) => other is not null &&
        Width == other.Width &&
        Height == other.Height &&
        _values.SequenceEqual(other._values);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BitMatrix other && Equals(other);
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Width);
        hashCode.Add(Height);
        foreach (var buffer in _values)
        {
            hashCode.Add(buffer);
        }
        return hashCode.ToHashCode();
    }
}
