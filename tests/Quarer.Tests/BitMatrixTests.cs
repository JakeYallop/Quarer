namespace Quarer.Tests;

public class BitMatrixTests
{
    [Fact]
    public void Indexer_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;

        var matrix = new BitMatrix(width, height);

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
}
