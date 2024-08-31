namespace Quarer.Tests;
public class TrackedBitMatrixTests
{
    [Fact]
    public void Wrap_WrapsExistingBitMatrix()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);
        matrix[0, 0] = true;
        matrix[width - 1, height - 1] = true;
        var trackedMatrix = TrackedBitMatrix.Wrap(matrix);
        matrix[0, 1] = true;
        Assert.True(trackedMatrix[0, 0]);
        Assert.True(trackedMatrix[width - 1, height - 1]);
        Assert.True(matrix[0, 1]);
        Assert.True(trackedMatrix[0, 1]);
        Assert.False(trackedMatrix.Changes[0, 0]);
        Assert.False(trackedMatrix.Changes[width - 1, height - 1]);
    }

    [Fact]
    public void Indexer_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;

        var matrix = new TrackedBitMatrix(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                Assert.False(matrix[x, y]);
            }
        }

        matrix[0, 0] = true;
        Assert.True(matrix[0, 0]);

        matrix[width - 1, height - 1] = true;
        Assert.True(matrix[width - 1, height - 1]);
    }

    [Fact]
    public void Indexer_Original_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;

        var matrix = new TrackedBitMatrix(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                Assert.False(matrix.Original[x, y]);
            }
        }

        matrix[0, 0] = true;
        Assert.True(matrix.Original[0, 0]);

        matrix.Original[width - 1, height - 1] = true;
        Assert.True(matrix.Original[width - 1, height - 1]);
    }

    [Fact]
    public void Indexer_Changes_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;

        var matrix = new TrackedBitMatrix(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                Assert.False(matrix.Changes[x, y]);
            }
        }

        matrix[0, 0] = true;
        Assert.True(matrix.Changes[0, 0]);

        matrix.Changes[width - 1, height - 1] = true;
        Assert.True(matrix.Changes[width - 1, height - 1]);
    }

    [Fact]
    public void IsEmpty_ReturnsExpectedResult()
    {
        var m = new TrackedBitMatrix(33, 31);
        Assert.True(m.IsEmpty(0, 0));

        m[0, 0] = true;
        m[0, 1] = false;
        m[32, 1] = false;
        m[0, 30] =  true;

        Assert.False(m.IsEmpty(0, 0));
        Assert.False(m.IsEmpty(0, 1));
        Assert.False(m.IsEmpty(32, 1));
        Assert.False(m.IsEmpty(0, 30));
    }

    [Fact]
    public void Clone_ReturnsClonedMatrix()
    {
        var m = new TrackedBitMatrix(33, 33);
        m[0, 0] = true;
        m[32, 32] = true;

        var clone = m.Clone();
        Assert.True(clone[0, 0]);
        Assert.True(clone[32, 32]);
        m[0, 0] = false;
        m[32, 32] = false;
        Assert.True(clone[0, 0]);
        Assert.True(clone[32, 32]);
        clone[1, 1] = true;
        clone[2, 2] = true;
        Assert.False(m[1, 1]);
        Assert.False(m[2, 2]);
    }
}
