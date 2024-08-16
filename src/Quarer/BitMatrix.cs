namespace Quarer;
public sealed class BitMatrix
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
                arr[i] = new BitBuffer(width);
            }

            return arr;
        }
    }

    public int Width { get; }
    public int Height { get; }
    public bool this[int x, int y]
    {
        get => _values[y][x];
        set => _values[y][x] = value;
    }
}
