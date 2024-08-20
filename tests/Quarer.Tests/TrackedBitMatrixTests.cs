namespace Quarer.Tests;
public class TrackedBitMatrixTests
{
    [Fact]
    public void Ctor_WrapsExistingBitMatrix()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);
        matrix[0, 0] = true;
        matrix[width - 1, height - 1] = true;
        var trackedMatrix = new TrackedBitMatrix(matrix);
        Assert.True(trackedMatrix[0, 0]);
        Assert.True(trackedMatrix[width - 1, height - 1]);
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
}
