namespace Quarer;

public sealed class TrackedBitMatrix : BitMatrix
{
    private readonly BitMatrix _changes;

    private TrackedBitMatrix(BitMatrix original, BitMatrix changes) : base(GetValues(original), original.Width, original.Height)
    {
        _changes = changes;
    }

    public TrackedBitMatrix(int width, int height) : base(width, height)
    {
        _changes = new BitMatrix(width, height);
    }

    public BitMatrix Original => this;

    public override bool this[int x, int y]
    {
        get => base[x, y];
        set
        {
            _changes[x, y] = true;
            base[x, y] = value;
        }
    }

    public static TrackedBitMatrix Wrap(BitMatrix bitMatrix) => new(bitMatrix, new(bitMatrix.Width, bitMatrix.Height));

    public override TrackedBitMatrix Clone() => new(base.Clone(), _changes.Clone());

    public bool IsEmpty(int x, int y) => !_changes[x, y];
}
