namespace Quarer;

public sealed class TrackedBitMatrix : BitMatrix
{
    public TrackedBitMatrix(int width, int height) : base(width, height)
    {
        Changes = new BitMatrix(width, height);
    }

    public TrackedBitMatrix(BitMatrix bitMatrix) : base(bitMatrix)
    {
        Changes = new BitMatrix(bitMatrix.Width, bitMatrix.Height);
    }

    public BitMatrix Original => this;
    public BitMatrix Changes { get; }

    public override bool this[int x, int y]
    {
        get => base[x, y];
        set
        {
            Changes[x, y] = true;
            base[x, y] = value;
        }
    }

    public bool HasChanged(int x, int y) => Changes[x, y];
}
