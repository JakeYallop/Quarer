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

    [Fact]
    public void Clone_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);
        matrix[0, 0] = true;
        matrix[width - 1, height - 1] = true;
        var clone = matrix.Clone();
        Assert.True(clone[0, 0]);
        Assert.True(clone[width - 1, height - 1]);

        clone[1, 1] = true;
        Assert.False(matrix[1, 1]);
        Assert.True(clone[1, 1]);
    }

    [Fact]
    public void GetRow_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);

        matrix[0, 0] = true;
        matrix[width - 1, 0] = true;
        var row = matrix.GetRow(0);
        Assert.True(row[0]);
        Assert.False(row[1]);
        Assert.True(row[width - 1]);

        matrix[0, height - 1] = true;
        matrix[width - 1, height - 1] = true;
        row = matrix.GetRow(height - 1);
        Assert.True(row[0]);
        Assert.False(row[1]);
        Assert.True(row[width - 1]);
    }

    [Fact]
    public void GetRow_ReturnsCopy()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);
        matrix[0, 0] = true;
        var row = matrix.GetRow(0);
        Assert.True(row[0]);

        row[0] = false;
        Assert.True(matrix[0, 0]);
    }

    [Fact]
    public void GetRow_WhenRowNegative_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new BitMatrix(33, 31);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetRow(-1));
    }

    [Fact]
    public void GetRow_WhenRowGreaterThanHeight_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new BitMatrix(33, 31);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetRow(31));
    }

    [Fact]
    public void GetColumn_ReturnsExpectedResult()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);

        matrix[0, 0] = true;
        matrix[0, height - 1] = true;
        var column = matrix.GetColumn(0);
        Assert.True(column[0]);
        Assert.False(column[1]);
        Assert.True(column[height - 1]);

        matrix[width - 1, 0] = true;
        matrix[width - 1, height - 1] = true;
        column = matrix.GetColumn(width - 1);
        Assert.True(column[0]);
        Assert.False(column[1]);
        Assert.True(column[height - 1]);
    }

    [Fact]
    public void GetColumn_ReturnsCopy()
    {
        var width = 33;
        var height = 31;
        var matrix = new BitMatrix(width, height);
        matrix[0, 0] = true;
        var column = matrix.GetColumn(0);
        Assert.True(column[0]);
        column[0] = false;
        Assert.True(matrix[0, 0]);
    }

    [Fact]
    public void GetColumn_WhenColumnNegative_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new BitMatrix(33, 31);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetColumn(-1));
    }

    [Fact]
    public void GetColumn_WhenColumnGreaterThanWidth_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new BitMatrix(33, 31);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetColumn(33));
    }
}
