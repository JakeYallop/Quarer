namespace Quarer;
public class BitMatrix
{
    private readonly BitBuffer[] _values;

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

    protected BitMatrix(BitMatrix bitMatrix)
    {
        _values = bitMatrix._values;
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
