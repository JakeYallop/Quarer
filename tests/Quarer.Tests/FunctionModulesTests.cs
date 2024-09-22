using System.Runtime.CompilerServices;
using static Quarer.Tests.MatrixTestUtilities;

namespace Quarer.Tests;
public class FunctionModulesTests
{
    [Theory]
    [InlineData(1, 0, 0, true)] // top-left finder pattern
    [InlineData(1, 15, 0, true)] // top-right finder pattern
    [InlineData(1, 0, 8, true)] // bottom-left finder pattern
    [InlineData(1, 7, 0, true)] // top-left right edge separator pattern
    [InlineData(1, 5, 7, true)] // top-left bottom edge separator pattern
    [InlineData(1, 6, 13, true)] // bottom-right top edge separator pattern
    [InlineData(1, 8, 16, true)] // bottom-right right edge separator pattern
    [InlineData(1, 16, 8, true)] // top-right bottom edge separator pattern
    [InlineData(1, 13, 5, true)] // top-right left edge separator pattern
    [InlineData(1, 8, 6, true)] // timing pattern
    [InlineData(1, 10, 6, true)] // timing pattern
    [InlineData(1, 6, 9, true)] // timing pattern
    [InlineData(1, 6, 11, true)] // timing pattern
    [InlineData(1, 8, 3, true)] // format info
    [InlineData(1, 8, 15, true)] // format info
    [InlineData(1, 1, 8, true)] // format info
    [InlineData(1, 15, 8, true)] // format info
    [InlineData(2, 17, 17, true)] // alignment pattern
    [InlineData(2, 18, 17, true)] // alignment pattern
    [InlineData(2, 19, 17, true)] // alignment pattern
    [InlineData(2, 15, 8, false)] // alignment pattern blocked by position detection pattern, resulting in a free spot
    [InlineData(2, 15, 7, false)] // alignment pattern blocked by position detection pattern, resulting in a free spot
    [InlineData(2, 15, 5, false)] // alignment pattern blocked by position detection pattern, resulting in a free spot
    [InlineData(7, 0, 36, true)] // version info
    [InlineData(7, 5, 36, true)] // version info
    [InlineData(7, 0, 38, true)] // version info
    [InlineData(7, 5, 38, true)] // version info
    public void IsFunctionModule_ReturnsExpectedResult(byte versionNumber, int x, int y, bool expected)
    {
        var version = QrVersion.GetVersion(versionNumber);
        var functionModules = FunctionModules.GetForVersion(version);
        Assert.Equal(expected, functionModules.IsFunctionModule(x, y));
    }

    [Fact]
    public void FunctionModuleCache_ReturnsSameInstance()
    {
        var version = QrVersion.GetVersion(1);
        var functionModules1 = FunctionModules.GetForVersion(version);
        var functionModules2 = FunctionModules.GetForVersion(version);
        Assert.Same(functionModules1, functionModules2);
    }

    [Fact]
    public void FunctionModuleCache_MultipleThreads()
    {
        var barrier = new Barrier(2);
        FunctionModules? matrix1 = null;
        FunctionModules? matrix2 = null;

        var t1 = new Thread(() =>
        {
            barrier.SignalAndWait();
            matrix1 = FunctionModules.GetForVersion(QrVersion.GetVersion(1));
        });

        var t2 = new Thread(() =>
        {
            barrier.SignalAndWait();
            matrix2 = FunctionModules.GetForVersion(QrVersion.GetVersion(1));
        });

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        Assert.NotNull(matrix1);
        Assert.NotNull(matrix2);

        Assert.Same(matrix1, matrix2);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref BitMatrix _matrix(FunctionModules @this);

    [Fact]
    public void FunctionModulesMatrix_SetsExpectedPositions_Version1()
    {
        var version = QrVersion.GetVersion(1);
        var functionModules = FunctionModules.GetForVersion(version);
        var m = _matrix(functionModules);

        Assert.Equal("""
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X X X X X X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            X X X X X X X X X - - - - X X X X X X X X
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - -
            """, MatrixToString(m));
    }

    [Fact]
    public void FunctionModulesMatrix_SetsExpectedPositions_Version2()
    {
        var version = QrVersion.GetVersion(2);
        var functionModules = FunctionModules.GetForVersion(version);
        var m = _matrix(functionModules);

        Assert.Equal("""
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X X X X X X X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - X X X X X X X X
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - -
            """, MatrixToString(m));
    }

    [Fact]
    public void FunctionModulesMatrix_SetsExpectedPositions_Version7()
    {
        var version = QrVersion.GetVersion(7);
        var functionModules = FunctionModules.GetForVersion(version);
        var m = _matrix(functionModules);

        Assert.Equal("""
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - X X X X X X X X X X X
            X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - - X X X X X X X X
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - - X X X X X X X X
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            """, MatrixToString(m));
    }
}
