namespace Quarer;
public class BitMatrix
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

    //TODO: Tests for this
    //TODO: Provide an indexable read only view into the matrix so we avoid copying here.
    //TODO: We shouldn't return a BItBuffer as its writeable.
    public virtual BitBuffer GetRow(int y)
    {
        var buffer = new BitBuffer(Width);
        buffer.SetCountUnsafe(Width);
        _values[y].CopyTo(buffer);
        return buffer;
    }

    //TODO: Tests for this
    //TODO: Provide an indexable read only view into the matrix so we avoid copying here.
    //TODO: We shouldn't return a BItBuffer as its writeable.
    public virtual BitBuffer GetColumn(int x)
    {
        var buffer = new BitBuffer(Height);
        buffer.SetCountUnsafe(Height);
        for (var i = 0; i < Height; i++)
        {
            buffer[i] = _values[i][x];
        }
        return buffer;
    }

    //TODO: Tests for this
    public virtual BitMatrix Clone()
    {
        var buffers = new BitBuffer[_values.Length];
        for (var i = 0; i < _values.Length; i++)
        {
            buffers[i] = new BitBuffer(_values[i].Count);
            _values[i].CopyTo(buffers[i]);
        }
        return new BitMatrix(buffers, Width, Height);
    }

    public int Width { get; }
    public int Height { get; }
    public virtual bool this[int x, int y]
    {
        get => _values[y][x];
        set => _values[y][x] = value;
    }
}
