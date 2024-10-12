namespace Quarer.Tests;

public class ByteMatrixMarshalTests
{
    [Fact]
    public void GetWritableRow()
    {
        var matrix = new ByteMatrix(3, 3);
        var row = ByteMatrixMarshal.GetWritableRow(matrix, 1);
        row[0] = 1;
        row[1] = 2;
        row[2] = 3;
        var resultingRow = matrix.GetRow(1);
        Assert.Equal(1, resultingRow[0]);
        Assert.Equal(2, resultingRow[1]);
        Assert.Equal(3, resultingRow[2]);
    }

    [Fact]
    public void GetWritableColumn()
    {
        var matrix = new ByteMatrix(3, 3);
        var column = ByteMatrixMarshal.GetWritableColumn(matrix, 1);
        column[0] = 1;
        column[1] = 2;
        column[2] = 3;
        var resultingColumn = matrix.GetColumn(1);
        Assert.Equal(1, resultingColumn[0]);
        Assert.Equal(2, resultingColumn[1]);
        Assert.Equal(3, resultingColumn[2]);
    }

    [Fact]
    public void UpdateColumnMajorOrderFromRow()
    {
        var matrix = new ByteMatrix(3, 3);
        var row = ByteMatrixMarshal.GetWritableRow(matrix, 1);
        row[0] = 1;
        row[1] = 2;
        row[2] = 3;
        ByteMatrixMarshal.UpdateColumnMajorOrder(matrix, row: 1, offset: 0, row);
        Assert.Equal(1, matrix.GetColumn(0)[1]);
        Assert.Equal(2, matrix.GetColumn(1)[1]);
        Assert.Equal(3, matrix.GetColumn(2)[1]);
    }

    [Fact]
    public void UpdateColumnMajorOrderFromRow_WithOffset()
    {
        var matrix = new ByteMatrix(4, 4);
        var dataToWrite = new byte[] { 2, 3 };
        ByteMatrixMarshal.UpdateColumnMajorOrder(matrix, row: 1, offset: 1, dataToWrite);
        Assert.Equal(0, matrix.GetColumn(0)[1]);
        Assert.Equal(2, matrix.GetColumn(1)[1]);
        Assert.Equal(3, matrix.GetColumn(2)[1]);
        Assert.Equal(0, matrix.GetColumn(3)[1]);
    }

    [Fact]
    public void UpdateRowMajorOrderFromColumn()
    {
        var matrix = new ByteMatrix(3, 3);
        var column = ByteMatrixMarshal.GetWritableColumn(matrix, 1);
        column[0] = 1;
        column[1] = 2;
        column[2] = 3;
        ByteMatrixMarshal.UpdateRowMajorOrder(matrix, column: 1, 0, column);
        Assert.Equal(1, matrix.GetRow(0)[1]);
        Assert.Equal(2, matrix.GetRow(1)[1]);
        Assert.Equal(3, matrix.GetRow(2)[1]);
    }

    [Fact]
    public void UpdateRowMajorOrderFromColumn_WithOffset()
    {
        var matrix = new ByteMatrix(4, 4);
        var dataToWrite = new byte[] { 2, 3 };
        ByteMatrixMarshal.UpdateRowMajorOrder(matrix, column: 1, offset: 1, dataToWrite);
        Assert.Equal(0, matrix.GetRow(0)[1]);
        Assert.Equal(2, matrix.GetRow(1)[1]);
        Assert.Equal(3, matrix.GetRow(2)[1]);
        Assert.Equal(0, matrix.GetRow(3)[1]);
    }

    [Fact]
    public void GetWritableRow_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new ByteMatrix(3, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => ByteMatrixMarshal.GetWritableRow(matrix, 3));
    }

    [Fact]
    public void GetWritableColumn_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new ByteMatrix(3, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => ByteMatrixMarshal.GetWritableColumn(matrix, 3));
    }
}
