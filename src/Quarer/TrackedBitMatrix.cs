namespace Quarer;

public sealed class TrackedBitMatrix : BitMatrix
{
    private TrackedBitMatrix(BitMatrix original, BitMatrix changes) : base(GetValues(original), original.Width, original.Height)
    {
        Changes = changes;
    }

    public TrackedBitMatrix(int width, int height) : base(width, height)
    {
        Changes = new BitMatrix(width, height);
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

    public static TrackedBitMatrix Wrap(BitMatrix bitMatrix) => new(bitMatrix, new(bitMatrix.Width, bitMatrix.Height));

    public override TrackedBitMatrix Clone() => new(base.Clone(), Changes.Clone());

    public bool IsEmpty(int x, int y) => !Changes[x, y];
}
