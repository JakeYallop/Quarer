using System.Diagnostics;

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

    [Theory]
    [InlineData(1, 1, "X")]
    [InlineData(1, 2, "X-")]
    [InlineData(1, 2, "-X")]
    [InlineData(1, 3, "-X-")]
    [InlineData(3, 3, "-X-X-X-X-")]
    [InlineData(10, 10, "--")]
    public void Equals_ReturnsExpectedResult(int width, int height, string contents)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width, height, contents);
        var result = m1.Equals((object)m2);
        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 1, "X")]
    [InlineData(1, 2, "X-")]
    [InlineData(1, 2, "-X")]
    [InlineData(1, 3, "-X-")]
    [InlineData(3, 3, "-X-X-X-X-")]
    [InlineData(10, 10, "--")]
    public void GetHashCode_Equal_ReturnsSameHashcode(int width, int height, string contents)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width, height, contents);
        var m1HashCode = m1.GetHashCode();
        var m2HashCode = m2.GetHashCode();
        Assert.Equal(m1HashCode, m2HashCode);
    }

    [Theory]
    [InlineData(1, 1, "X", 1, 2, "")]
    [InlineData(1, 3, "", 1, 2, "")]
    [InlineData(2, 1, "", 1, 2, "")]
    [InlineData(2, 2, "XX--", 2, 2, "X-X-")]
    [InlineData(3, 3, "X-X-X-X-X-", 3, 2, "X-X-X-")]
    public void GetHashCode_NotEqual_ReturnsDifferentHashcode(int width, int height, string contents, int width2, int height2, string contents2)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width2, height2, contents2);
        var m1HashCode = m1.GetHashCode();
        var m2HashCode = m2.GetHashCode();
        Assert.NotEqual(m1HashCode, m2HashCode);
    }

    [Theory]
    [InlineData(1, 1, "X")]
    [InlineData(1, 2, "X-")]
    [InlineData(1, 2, "-X")]
    [InlineData(1, 3, "-X-")]
    [InlineData(3, 3, "-X-X-X-X-")]
    [InlineData(10, 10, "--")]
    public void Equals_IEquatable_ReturnsExpectedResult(int width, int height, string contents)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width, height, contents);
        var result = m1.Equals(m2);
        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 1, "X")]
    [InlineData(1, 2, "X-")]
    [InlineData(1, 2, "-X")]
    [InlineData(1, 3, "-X-")]
    [InlineData(3, 3, "-X-X-X-X-")]
    [InlineData(10, 10, "--")]
    public void Op_Equality_ReturnsExpectedResult(int width, int height, string contents)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width, height, contents);
        var result = m1 == m2;
        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 1, "X", 1, 2, "")]
    [InlineData(1, 3, "", 1, 2, "")]
    [InlineData(2, 1, "", 1, 2, "")]
    [InlineData(2, 2, "XX--", 2, 2, "X-X-")]
    [InlineData(3, 3, "X-X-X-X-X-", 3, 2, "X-X-X-")]
    public void Op_Inequality_ReturnsExpectedResult(int width, int height, string contents, int width2, int height2, string contents2)
    {
        var m1 = InputToMatrix(width, height, contents);
        var m2 = InputToMatrix(width2, height2, contents2);
        var result = m1 != m2;
        Assert.True(result);
    }

    private static BitMatrix InputToMatrix(int width, int height, string input)
    {
        var matrix = new BitMatrix(width, height);
        var written = 0;
        for (var y = 0; y < matrix.Height; y++)
        {
            for (var x = 0; x < matrix.Width; x++)
            {
                if (written == input.Length)
                {
                    break;
                }

                Debug.Assert(input[written] is 'X' or '-', "Expected input to consist of only 'X' and '-'.");
                matrix[x, y] = input[written] == 'X';
                written++;
            }
        }

        return matrix!;
    }
}
