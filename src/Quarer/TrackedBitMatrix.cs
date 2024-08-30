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

    //TODO: Tests for this
    /// <summary>
    /// Create a copy of the <see cref="TrackedBitMatrix"/> <paramref name="trackedBitMatrix"/>.
    /// </summary>
    /// <param name="trackedBitMatrix">The matrix to copy.</param>
    public TrackedBitMatrix(TrackedBitMatrix trackedBitMatrix) : this(new BitMatrix(trackedBitMatrix.Original), new BitMatrix(trackedBitMatrix.Changes))
    {
    }

    public BitMatrix Original => this;
    public BitMatrix Changes { get; }

    public override bool this[int x, int y]
    {
        get => Original[x, y];
        set
        {
            Changes[x, y] = true;
            Original[x, y] = value;
        }
    }

    public static TrackedBitMatrix Wrap(BitMatrix bitMatrix) => new(bitMatrix, new(bitMatrix.Width, bitMatrix.Height));

    //TODO: Missing tests for this method
    public bool IsEmpty(int x, int y) => !Changes[x, y];
}
