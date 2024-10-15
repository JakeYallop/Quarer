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
    private static extern ref ByteMatrix _matrix(FunctionModules @this);

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

    [Fact]
    public void GetMaskableSegments_V2_ReturnsExpectedResults()
    {
        var version = QrVersion.GetVersion(2);
        var functionModules = FunctionModules.GetForVersion(version);

        /*
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
        */

        AssertMaskableSegmentInvariants(functionModules, version);
        // above horizontal timing pattern
        AssertContainsSegmentRange(functionModules, 9, version.Width - 8, 0, numberOfRows: 6, singleSegmentOnly: true);
        // below horizontal timing pattern, between position detection patterns
        AssertContainsSegmentRange(functionModules, 9, version.Width - 8, 7, numberOfRows: 2, singleSegmentOnly: true);
        // left side of vertical timing pattern
        AssertContainsSegmentRange(functionModules, 0, 6, 9, 8);

        var patternCenter = version.AlignmentPatternCenters[1];
        // right side of vertical timing pettern, avoiding overlapping with alignment pattern 
        AssertContainsSegmentRange(functionModules, 7, version.Width, 9, 7);
        // final row, which intersects with the alignment pattern
        AssertContainsSegmentRange(functionModules, 7, patternCenter - 2, 16, numberOfRows: 1);
        // remaining rows between position detection pattern and alignment pattern
        AssertContainsSegmentRange(functionModules, 9, patternCenter - 2, 17, numberOfRows: 4);
        // segments to the right of the alignment pattern
        AssertContainsSegmentRange(functionModules, patternCenter + 3, version.Width, 16, numberOfRows: 5);
        // segments between the position pattern and the end of the symbol
        AssertContainsSegmentRange(functionModules, 9, version.Width, 21, numberOfRows: 4, singleSegmentOnly: true);
    }

    [Fact]
    public void GetMaskableSegments_V7_ReturnsExpectedResults()
    {
        var version = QrVersion.GetVersion(7);
        var functionModules = FunctionModules.GetForVersion(version);

        /*
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
        */

        var topOrLeftPatternCenter = version.AlignmentPatternCenters[0];
        var middlePatternCenter = version.AlignmentPatternCenters[1];
        var bottomOrRightPatternCenter = version.AlignmentPatternCenters[2];
        AssertMaskableSegmentInvariants(functionModules, version);

        // above horizontal timing pattern, avoiding version information
        AssertContainsSegmentRange(functionModules, 9, version.Width - 11, 0, numberOfRows: 4, singleSegmentOnly: true);
        // intersecting with first alignment pattern
        AssertContainsSegmentRange(functionModules, 9, middlePatternCenter - 2, 4, numberOfRows: 2);
        AssertContainsSegmentRange(functionModules, middlePatternCenter + 3, version.Width - 11, 4, numberOfRows: 2);
        // after horizontal timing pattern, intersecting with alignment pattern
        AssertContainsSegmentRange(functionModules, 9, middlePatternCenter - 2, 7, numberOfRows: 2);
        AssertContainsSegmentRange(functionModules, middlePatternCenter + 3, version.Width - 8, 7, numberOfRows: 2);
        // up to second row of three alignment patterns
        AssertContainsSegmentRange(functionModules, 0, 6, 9, numberOfRows: 11);
        AssertContainsSegmentRange(functionModules, 7, version.Width, 9, numberOfRows: 11);
        // intersecting with second row of alignment patterns
        AssertContainsSegmentRange(functionModules, 0, topOrLeftPatternCenter - 2, 20, numberOfRows: 4);
        AssertContainsSegmentRange(functionModules, topOrLeftPatternCenter + 3, middlePatternCenter - 2, 20, numberOfRows: 5);
        AssertContainsSegmentRange(functionModules, middlePatternCenter + 3, bottomOrRightPatternCenter - 2, 20, numberOfRows: 5);
        AssertContainsSegmentRange(functionModules, bottomOrRightPatternCenter + 3, version.Width, 20, numberOfRows: 5);
        // down to start of format information in lower half of symbol
        AssertContainsSegmentRange(functionModules, 0, 6, 25, numberOfRows: 9);
        // right side of vertical timing pattern, ignoring final row which intersects with third row of timing pattenrs
        AssertContainsSegmentRange(functionModules, 7, version.Width, 25, numberOfRows: 11);
        // row which intersects timing pattern
        AssertContainsSegmentRange(functionModules, 7, middlePatternCenter - 2, 36, numberOfRows: 1);
        AssertContainsSegmentRange(functionModules, middlePatternCenter + 3, bottomOrRightPatternCenter - 2, 36, numberOfRows: 1);
        AssertContainsSegmentRange(functionModules, bottomOrRightPatternCenter + 3, version.Width, 36, numberOfRows: 1);
        // remaining rows intersecting with timing patterns
        AssertContainsSegmentRange(functionModules, 9, middlePatternCenter - 2, 37, numberOfRows: 4);
        AssertContainsSegmentRange(functionModules, middlePatternCenter + 3, bottomOrRightPatternCenter - 2, 37, numberOfRows: 4);
        AssertContainsSegmentRange(functionModules, bottomOrRightPatternCenter + 3, version.Width, 37, numberOfRows: 4);
        // final rows
        AssertContainsSegmentRange(functionModules, 9, version.Width, 41, numberOfRows: 4, singleSegmentOnly: true);
    }

    private static void AssertContainsSegmentRange(FunctionModules functionModules, int startX, int exclusiveEndX, int startY, int numberOfRows, bool singleSegmentOnly = false)
    {
        for (var i = startY; i < (startY + numberOfRows); i++)
        {
            var segments = functionModules.GetMaskableSegments(i);

            AssertContainsRange(segments, startX, exclusiveEndX);

            if (singleSegmentOnly)
            {
                Assert.Equal(1, segments.Length);
            }
        }
    }

    private static void AssertContainsRange(ReadOnlySpan<Range> ranges, int start, int end) => Assert.Contains(new Range(start, end), ranges.ToArray());

    private static void AssertMaskableSegmentInvariants(FunctionModules functionModules, QrVersion version)
    {
        // horizontal timing pattern
        var segment = functionModules.GetMaskableSegments(6);
        Assert.Equal(0, segment.Length);

        // vertical timing pattern
        for (var i = 9; i < version.Width - 8; i++)
        {
            var segments = functionModules.GetMaskableSegments(i);
            foreach (var range in segments)
            {
                AssertRangeDoesNotOverlap(range, 6);
            }
        }

        // version information
        if (version.Version >= 7)
        {
            for (var i = 0; i <= 3; i++)
            {
                var segments = functionModules.GetMaskableSegments(i);
                foreach (var range in segments)
                {
                    AssertRangeDoesNotOverlap(range, version.Width - 11, version.Width - 9);
                }
            }

            for (var i = version.Width - 11; i < version.Width - 8; i++)
            {
                var segments = functionModules.GetMaskableSegments(i);
                foreach (var range in segments)
                {
                    AssertRangeDoesNotOverlap(range, 0, 3);
                }
            }
        }
    }

    private static void AssertRangeDoesNotOverlap(Range range, int x) => Assert.False(range.Start.Value <= x && range.End.Value > x);
    private static void AssertRangeDoesNotOverlap(Range range, int startX, int endX) => Assert.False(range.Start.Value <= endX && range.End.Value > startX);
}
