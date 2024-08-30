namespace Quarer;
public class BitMatrix
{
    // I don't love this, but its the only way to get the values out of the matrix without copying
    // and without exposing the underlying BitBuffers publicly
    protected internal static BitBuffer[] GetValues(BitMatrix bitMatrix) => bitMatrix._values;

    private readonly BitBuffer[] _values;

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
    /// <summary>
    /// Return a new <see cref="BitMatrix"/> with a copy of the contents in <paramref name="bitMatrix"/>.
    /// </summary>
    /// <param name="bitMatrix">The BitMatrix to copy.</param>
    public BitMatrix(BitMatrix bitMatrix)
    {
        var arr = new BitBuffer[bitMatrix._values.Length];

        //TODO: Tests
        Array.Copy(bitMatrix._values, 0, arr, 0, bitMatrix._values.Length);

        //for (var i = 0; i < bitMatrix._values.Length; i++)
        //{
        //    var values = bitMatrix._values[i];
        //    var buffer = new BitBuffer(values.Count);
        //    values.CopyTo(buffer);
        //    arr[i] = buffer;
        //}
        _values = arr;
        Width = bitMatrix.Width;
        Height = bitMatrix.Height;
    }

    public int Width { get; }
    public int Height { get; }
    public virtual bool this[int x, int y]
    {
        get => _values[y][x];
        set => _values[y][x] = value;
    }
}
