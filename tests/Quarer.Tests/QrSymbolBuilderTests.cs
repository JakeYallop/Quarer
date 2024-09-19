using static Quarer.Tests.MatrixTestUtilities;

namespace Quarer.Tests;

public class QrSymbolBuilderTests
{
    [Fact]
    public void Encode_PositionDetectionAndSeparators_EncodesExpectedSymbol()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);

        Assert.Equal("""
            X X X X X X X - - - - - - - X X X X X X X
            X - - - - - X - - - - - - - X - - - - - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - - - - - X - - - - - - - X - - - - - X
            X X X X X X X - - - - - - - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Encode_PositionDetectionAndSeparators_Changes_SetsExpectedModules()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);

        Assert.Equal("""
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            X X X X X X X X - - - - - X X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            X X X X X X X X - - - - - - - - - - - - -
            """, ChangesMatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Encode_PositionDetectionAndSeparatorsAndTimingPatterns_EncodesExpectedSymbol()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodeTimingPatterns(m);

        Assert.Equal("""
            X X X X X X X - - - - - - - X X X X X X X
            X - - - - - X - - - - - - - X - - - - - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - - - - - X - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Encode_PositionDetectionAndSeparatorsAndTimingPatternsAndStaticDarkModule_EncodesExpectedSymbol()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeStaticDarkModule(m);

        Assert.Equal("""
            X X X X X X X - - - - - - - X X X X X X X
            X - - - - - X - - - - - - - X - - - - - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - - - - - X - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - X - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Encode_PositionDetectionAndSeparatorsAndTimingPatternsAndFormatInformation_EncodesExpectedSymbol()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, MaskPattern.PatternZero_Checkerboard);

        Assert.Equal("""
            X X X X X X X - - - - - - - X X X X X X X
            X - - - - - X - X - - - - - X - - - - - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - X - - - - - X - X X X - X
            X - - - - - X - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            X - X - X - X - - - - - - - - - X - - X -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X - X X X - X - X - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - X - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - -
            X X X X X X X - X - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void EncodeVersionInformation_VersionLessThan7_DoesNotEncodeAnyVersionInformation()
    {
        var version = QrVersion.GetVersion(3, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);
        QrSymbolBuilder.EncodeVersionInformation(m, version);
        var size = version.ModulesPerSide;
        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                Assert.True(m.IsEmpty(j, size -  11 + i));
                Assert.True(m.IsEmpty(size -  11 + i, j));
            }
        }
    }

    [Fact]
    public void EncodeVersionInformation_VersionMoreThanOrEqualTo7_DoesNotEncodeAnyVersionInformation()
    {
        var version = QrVersion.GetVersion(7, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);
        QrSymbolBuilder.EncodeVersionInformation(m, version);
        var size = version.ModulesPerSide;
        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                Assert.False(m.IsEmpty(j, size -  11 + i));
                Assert.False(m.IsEmpty(size -  11 + i, j));
            }
        }
    }

    [Fact]
    public void Encode_PositionDetectionAndSeparatorsAndTimingPatternsStaticDarkModuleAndAdjustmentPatterns_EncodesExpectedSymbol()
    {
        var version = QrVersion.GetVersion(7, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodeStaticDarkModule(m);
        QrSymbolBuilder.EncodePositionAdjustmentPatterns(m, version);
        QrSymbolBuilder.EncodeTimingPatterns(m);

        Assert.Equal("""
            X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - X X X X X X X
            X - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - X - - - - - X
            X - X X X - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - - - - - - - X X X X X - - - - - - - - - - - - - X - X X X - X
            X - - - - - X - - - - - - - - - - - - - X - - - X - - - - - - - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - X - - - X - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - X X X X X - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - X - - - X - - - - - - - - - - - X - - - X - - - - - - - - - - - X - - - X - - - -
            - - - - X - X - X - - - - - - - - - - - X - X - X - - - - - - - - - - - X - X - X - - - -
            - - - - X - - - X - - - - - - - - - - - X - - - X - - - - - - - - - - - X - - - X - - - -
            - - - - X X X X X - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            - - - - - - - - X - - - - - - - - - - - X - - - X - - - - - - - - - - - X - - - X - - - -
            X X X X X X X - - - - - - - - - - - - - X - X - X - - - - - - - - - - - X - X - X - - - -
            X - - - - - X - - - - - - - - - - - - - X - - - X - - - - - - - - - - - X - - - X - - - -
            X - X X X - X - - - - - - - - - - - - - X X X X X - - - - - - - - - - - X X X X X - - - -
            X - X X X - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Encode_EncodeDataBits_EncodesExpectedSymbol2M()
    {
        var version = QrVersion.GetVersion(2, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodePositionAdjustmentPatterns(m, version);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeStaticDarkModule(m);
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, MaskPattern.PatternZero_Checkerboard);
        QrSymbolBuilder.EncodeVersionInformation(m, version);

        Assert.Equal("""
            X X X X X X X - - - - - - - - - - - X X X X X X X
            X - - - - - X - X - - - - - - - - - X - - - - - X
            X - X X X - X - - - - - - - - - - - X - X X X - X
            X - X X X - X - - - - - - - - - - - X - X X X - X
            X - X X X - X - X - - - - - - - - - X - X X X - X
            X - - - - - X - - - - - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - - - - - -
            X - X - X - X - - - - - - - - - - - - - X - - X -
            - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - - - - - - - - - -
            - - - - - - - - - - - - - - - - - - - - - - - - -
            - - - - - - X - - - - - - - - - X X X X X - - - -
            - - - - - - - - X - - - - - - - X - - - X - - - -
            X X X X X X X - - - - - - - - - X - X - X - - - -
            X - - - - - X - - - - - - - - - X - - - X - - - -
            X - X X X - X - X - - - - - - - X X X X X - - - -
            X - X X X - X - - - - - - - - - - - - - - - - - -
            X - X X X - X - X - - - - - - - - - - - - - - - -
            X - - - - - X - - - - - - - - - - - - - - - - - -
            X X X X X X X - X - - - - - - - - - - - - - - - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);

        var codewordsBuffer = AlternatingCodewordsBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, maskPattern: null);

        Assert.Equal("""
            X X X X X X X - - X - - - X - - - - X X X X X X X
            X - - - - - X - X X X - - X X - - - X - - - - - X
            X - X X X - X - - X X - - X X - - - X - X X X - X
            X - X X X - X - - X X - X X X - X - X - X X X - X
            X - X X X - X - X - X X X - X X X - X - X X X - X
            X - - - - - X - - - - X X - - X X - X - - - - - X
            X X X X X X X - X - X - X - X - X - X X X X X X X
            - - - - - - - - - - - X X - - X X - - - - - - - -
            X - X - X - X - - - - X - - - X - - - - X - - X -
            X - - - X - - - - X - - - X - - - - - - - - - X X
            X X - - X X X - - X X - - X X - - X X - - - - X X
            X X - - X X - - - X X - - X X - - X X - - - - X X
            X X - X X X X - X X X - X X X - X X X X X - - X X
            - X X X - X - X X - X X X - X X X X X X X X X - -
            - - X X - - X X X - - X X - - X X - - X X X X - -
            - - X X - - - X X - - X X - - X X - - X X X X - -
            - - X - - - X X - - - X - - - X X X X X X X X - -
            - - - - - - - - X X - - - X - - X - - - X - - X X
            X X X X X X X - - X X - - X X - X - X - X - - X X
            X - - - - - X - - X X - - X X - X - - - X - - X X
            X - X X X - X - X X X - X X X - X X X X X - - X X
            X - X X X - X - - - X X X - X - - - - - - X X - -
            X - X X X - X - X - - X X - - - - - - - - X X - -
            X - - - - - X - - - - X X - - X X X X - - X X - -
            X X X X X X X - X - - X - - - X X X X - - X X - -
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithSpecificPatternInCodewords_EncodesExpectedSymbol1M()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodePositionAdjustmentPatterns(m, version);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeStaticDarkModule(m);
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, MaskPattern.PatternZero_Checkerboard);
        QrSymbolBuilder.EncodeVersionInformation(m, version);
        var codewordsBuffer = Buffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, maskPattern: null);

        Assert.Equal("""
            X X X X X X X - - - - - - - X X X X X X X
            X - - - - - X - X X - - - - X - - - - - X
            X - X X X - X - - - - X - - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - X - - - - - X - X X X - X
            X - - - - - X - - X - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - X - - - - - - - - -
            X - X - X - X - - - - - - - - - X - - X -
            - - - - - - - - - - - - - - - - - - - - -
            X - - - X - X - - X - - - X - - - X - - -
            - - X - - - - X - - - X - - - X - - - X -
            - - - - - - X - - - - - - - - - - - - - -
            - - - - - - - - X - - - - - - - - - - - -
            X X X X X X X - - X - - - X - - - X - - -
            X - - - - - X - - - - X - - - X - - - X -
            X - X X X - X - X - - - - - - - - - - - -
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - X X - - - X - - - X - - -
            X - - - - - X - - - - X - - - X - - - X -
            X X X X X X X - X - - - - - - - - - - - -
            """, MatrixToString(m));

        static BitBuffer Buffer(int n)
        {
            var buffer = new BitBuffer(n * 8);
            var writer = new BitWriter(buffer);
            for (var i = 0; i < n; i++)
            {
                var b = (byte)0b00010000;
                writer.WriteBitsBigEndian(b, 8);
            }
            return buffer;
        }
    }

    [Fact]
    public void Encode_EncodeDataBits_EncodesExpectedSymbol7H()
    {
        var version = QrVersion.GetVersion(7, ErrorCorrectionLevel.H);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodeStaticDarkModule(m);
        QrSymbolBuilder.EncodePositionAdjustmentPatterns(m, version);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.H, MaskPattern.PatternZero_Checkerboard);
        QrSymbolBuilder.EncodeVersionInformation(m, version);

        var codewordsBuffer = AlternatingCodewordsBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, maskPattern: null);

        Assert.Equal("""
            X X X X X X X - X X X X X X X X X X X X X X X - - - - - - - - - - X - - X - X X X X X X X
            X - - - - - X - - - - X X - - X X - - X X X X - - - - - - - - - - X - X - - X - - - - - X
            X - X X X - X - - - - X X - - X X - - X X X X - - X X X X X X X X X - X - - X - X X X - X
            X - X X X - X - X - - - - - - - - - - - - X X - - X X X X X X X X X - X X - X - X X X - X
            X - X X X - X - - - - - - - - - - - - - X X X X X X X X X X X X X X X X X - X - X X X - X
            X - - - - - X - - X X - - X X - - X X - X - - - X X X X X X X X X X - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - - X X - - X X - - X X - X - - - X - - - - - - - - - - X X - - - - - - - -
            - - X - X X X - X X X X X X X X X X X - X X X X X - - - - - - - - - - - - X - - - X - - X
            X X X X - - - - - X X X X X X X X X X - - - - X X - - - - - - - - - - - - - - - - X X - -
            - - X X - - X - - - - X X - - X X - - X X - - X X - - - - - - - - - - - - - - - - X X - -
            - - X X X X - X X - - X X - - X X - - X X - - X X X X X X X X X X X X - - X X X X X X - -
            - - - - X X X X X - - - - - - - - - - X X - - X X X X X X X X X X X X X X X X X X X X - -
            - - - - X X - X X - - - - - - - - - - X X X X - - X X X X X X X X X X X X X X X X - - X X
            X X - - X X X X X X X - - X X - - X X - - X X - - X X X X X X X X X X X X X X X X - - X X
            X X - - - - - - - X X - - X X - - X X - - X X - - - - - - - - - - - - X X - - - - - - X X
            X X X X - - X - - X X X X X X X X X X - - X X - - - - - - - - - - - - - - - - - - - - X X
            X X X X - - - - - X X X X X X X X X X - - - - X X - - - - - - - - - - - - - - - - X X - -
            - - X X - - X - - - - X X - - X X - - X X - - X X - - - - - - - - - - - - - - - - X X - -
            - - X X X X - X X - - X X - - X X - - X X - - X X X X X X X X X X X X - - X X X X X X - -
            - - - - X X X X X - - - - - - - - - - X X X X X X X X X X X X X X X X X X X X X X X X - -
            - - - - X - - - X - - - - - - - - - - X X - - - X X X X X X X X X X X X X - - - X - - X X
            X X - - X - X - X X X - - X X - - X X X X - X - X X X X X X X X X X X X X - X - X - - X X
            X X - - X - - - X X X - - X X - - X X X X - - - X - - - - - - - - - - X X - - - X - - X X
            X X X X X X X X X X X X X X X X X X X - X X X X X - - - - - - - - - - X X X X X X - - X X
            X X X X X X - X X X X X X X X X X X X - - - - X X - - - - - - - - - - X X X X X X X X - -
            - - X X X X X X X - - X X - - X X - - - - X X - - - - - - - - - - - - X - X X X X X X - -
            - - X X X X - X X - - X X - - X X - - - - X X - - X X X X X X X X X X - - X X X X X X - -
            - - - - - - X - - - - - - - - - - - - - X X X - - X X X X X X X X X X - - - - - - X X - -
            - - - - - - - - - - - - - - - - - - - X X X X - - X X X X X X X X X X - - - - - - - - X X
            X X - - - - X - - X X - - X X - - X X X X - - X X X X X X X X X X X X - X - - - - - - X X
            X X - - - - - - - X X - - X X - - X X X X - - X X - - - - - - - - - - X X - - - - - - X X
            X X X X X X X X X X X X X X X X X X X X - - - X X - - - - - - - - - - X X X X X X - - X X
            X X X X X X - X X X X X X X X X X X X - - - - X X - - - - - - - - - - X X X X X X X X - -
            - - - - X - X X X - - X X - - X X - - - - X X - - - - - - - - - - - - X - X X X X X X - -
            - X X X X - - X X - - X X - - X X - - - - X X - - X X X X X X X X X X - - X X X X X X - -
            X - - X X - X - - - - - - - - - - - - - X X X X X X X X X X X X X X X - X X X X X X X - -
            - - - - - - - - X - - - - - - - - - - X X - - - X X X X X X X X X X X - X - - - X - - X X
            X X X X X X X - - X X - - X X - - X X X X - X - X X X X X X X X X X X - X - X - X - - X X
            X - - - - - X - X X X - - X X - - X X X X - - - X - - - - - - - - - - - X - - - X - - X X
            X - X X X - X - X X X X X X X X X X X X X X X X X - - - - - - - - - - - X X X X X - - X X
            X - X X X - X - - X X X X X X X X X X X X X X - - - - - - - - - - - - X X - - - - X X - -
            X - X X X - X - X - - X X - - X X - - X X X X - - - - - - - - - - - - X X - - - - X X - -
            X - - - - - X - - - - X X - - X X - - - - - - X X X X X X X X X X X X X X - - - - X X - -
            X X X X X X X - - - - - - - - - - - - - - - - X X X X X X X X X X X X X X - - - - X X - -
            """, MatrixToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
    }

    private static (TrackedBitMatrix, QrVersion) Get1MSymbolWithoutData(MaskPattern maskPattern)
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var m = new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide);

        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopLeft);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.TopRight);
        QrSymbolBuilder.EncodePositionDetectionPattern(m, PositionDetectionPatternLocation.BottomLeft);
        QrSymbolBuilder.EncodePositionAdjustmentPatterns(m, version);
        QrSymbolBuilder.EncodeTimingPatterns(m);
        QrSymbolBuilder.EncodeStaticDarkModule(m);
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, maskPattern);
        QrSymbolBuilder.EncodeVersionInformation(m, version);

        return (m, version);
    }

    [Fact]
    public void Encode_EncodeDataBits_WithCheckerboardMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternZero_Checkerboard);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternZero_Checkerboard);
        Assert.Equal("""
            X X X X X X X - - - X - X - X X X X X X X
            X - - - - - X - X X - X - - X - - - - - X
            X - X X X - X - - - X - X - X - X X X - X
            X - X X X - X - - X - X - - X - X X X - X
            X - X X X - X - X - X - X - X - X X X - X
            X - - - - - X - - X - X - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - X - X - - - - - - - - -
            X - X - X - X - - - X - X - - - X - - X -
            - X - X - X - X - X - X - X - X - X - X -
            X - X - X - X - X - X - X - X - X - X - X
            - X - X - X - X - X - X - X - X - X - X -
            X - X - X - X - X - X - X - X - X - X - X
            - - - - - - - - X X - X - X - X - X - X -
            X X X X X X X - - - X - X - X - X - X - X
            X - - - - - X - - X - X - X - X - X - X -
            X - X X X - X - X - X - X - X - X - X - X
            X - X X X - X - - X - X - X - X - X - X -
            X - X X X - X - X - X - X - X - X - X - X
            X - - - - - X - - X - X - X - X - X - X -
            X X X X X X X - X - X - X - X - X - X - X
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithHorizontalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternOne_HorizontalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternOne_HorizontalLines);
        Assert.Equal("""
            X X X X X X X - X X X X X - X X X X X X X
            X - - - - - X - - - - - - - X - - - - - X
            X - X X X - X - X X X X X - X - X X X - X
            X - X X X - X - - - - - - - X - X X X - X
            X - X X X - X - - X X X X - X - X X X - X
            X - - - - - X - X - - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            X - X - - - X X - X X X X - - X - - X - X
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X X X X X X X X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            X X X X X X X X X X X X X X X X X X X X X
            - - - - - - - - X - - - - - - - - - - - -
            X X X X X X X - X X X X X X X X X X X X X
            X - - - - - X - - - - - - - - - - - - - -
            X - X X X - X - - X X X X X X X X X X X X
            X - X X X - X - - - - - - - - - - - - - -
            X - X X X - X - X X X X X X X X X X X X X
            X - - - - - X - - - - - - - - - - - - - -
            X X X X X X X - X X X X X X X X X X X X X
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithVerticalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternTwo_VerticalLines);
        Assert.Equal("""
            X X X X X X X - - X - - X - X X X X X X X
            X - - - - - X - - X - - X - X - - - - - X
            X - X X X - X - X X - - X - X - X X X - X
            X - X X X - X - X X - - X - X - X X X - X
            X - X X X - X - X X - - X - X - X X X - X
            X - - - - - X - X X - - X - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X X - - X - - - - - - - -
            X - X X X X X - - X - - X - X X X X X - -
            X - - X - - - - - X - - X - - X - - X - -
            X - - X - - X - - X - - X - - X - - X - -
            X - - X - - - - - X - - X - - X - - X - -
            X - - X - - X - - X - - X - - X - - X - -
            - - - - - - - - X X - - X - - X - - X - -
            X X X X X X X - - X - - X - - X - - X - -
            X - - - - - X - X X - - X - - X - - X - -
            X - X X X - X - X X - - X - - X - - X - -
            X - X X X - X - X X - - X - - X - - X - -
            X - X X X - X - X X - - X - - X - - X - -
            X - - - - - X - - X - - X - - X - - X - -
            X X X X X X X - X X - - X - - X - - X - -
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithDiagonalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternThree_DiagonalLines);
        Assert.Equal("""
            X X X X X X X - - X - - X - X X X X X X X
            X - - - - - X - - - - X - - X - - - - - X
            X - X X X - X - X - X - - - X - X X X - X
            X - X X X - X - X X - - X - X - X X X - X
            X - X X X - X - X - - X - - X - X X X - X
            X - - - - - X - X - X - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X - - X - - - - - - - - -
            X - X X X X X - - - X - - - X X X X X - -
            X - - X - - - - - X - - X - - X - - X - -
            - - X - - X X - X - - X - - X - - X - - X
            - X - - X - - X - - X - - X - - X - - X -
            X - - X - - X - - X - - X - - X - - X - -
            - - - - - - - - X - - X - - X - - X - - X
            X X X X X X X - - - X - - X - - X - - X -
            X - - - - - X - X X - - X - - X - - X - -
            X - X X X - X - X - - X - - X - - X - - X
            X - X X X - X - X - X - - X - - X - - X -
            X - X X X - X - X X - - X - - X - - X - -
            X - - - - - X - - - - X - - X - - X - - X
            X X X X X X X - X - X - - X - - X - - X -
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithLargeCheckboardMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternFour_LargeCheckerboard);
        Assert.Equal("""
            X X X X X X X - - - - - X - X X X X X X X
            X - - - - - X - - - - - X - X - - - - - X
            X - X X X - X - X X X X - - X - X X X - X
            X - X X X - X - X X X X - - X - X X X - X
            X - X X X - X - X - - - X - X - X X X - X
            X - - - - - X - X - - - X - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X X X X - - - - - - - - -
            X - X X X X X - - - - - X - X X X X X - -
            X X X - - - - X X - - - X X X - - - X X X
            - - - X X X X - - X X X - - - X X X - - -
            - - - X X X - - - X X X - - - X X X - - -
            X X X - - - X X X - - - X X X - - - X X X
            - - - - - - - - X - - - X X X - - - X X X
            X X X X X X X - - X X X - - - X X X - - -
            X - - - - - X - X X X X - - - X X X - - -
            X - X X X - X - X - - - X X X - - - X X X
            X - X X X - X - X - - - X X X - - - X X X
            X - X X X - X - X X X X - - - X X X - - -
            X - - - - - X - - X X X - - - X X X - - -
            X X X X X X X - X - - - X X X - - - X X X
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithFieldsMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternFive_Fields);
        Assert.Equal("""
            X X X X X X X - - X X X X - X X X X X X X
            X - - - - - X - - - - - X - X - - - - - X
            X - X X X - X - X X - - X - X - X X X - X
            X - X X X - X - X - X - X - X - X X X - X
            X - X X X - X - X X - - X - X - X X X - X
            X - - - - - X - X - - - X - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X - - - X - - - - - - - -
            X - X X X X X - - X - - X - X X X X X - -
            X - X - X - - - X - X - X - X - X - X - X
            X - - X - - X - - X - - X - - X - - X - -
            X - - - - - - - - - - - X - - - - - X - -
            X X X X X X X X X X X X X X X X X X X X X
            - - - - - - - - X - - - X - - - - - X - -
            X X X X X X X - - X - - X - - X - - X - -
            X - - - - - X - X - X - X - X - X - X - X
            X - X X X - X - X X - - X - - X - - X - -
            X - X X X - X - X - - - X - - - - - X - -
            X - X X X - X - X X X X X X X X X X X X X
            X - - - - - X - - - - - X - - - - - X - -
            X X X X X X X - X X - - X - - X - - X - -
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithDiamondsMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternSix_Diamonds);
        Assert.Equal("""
            X X X X X X X - - X X X X - X X X X X X X
            X - - - - - X - - - - - X - X - - - - - X
            X - X X X - X - X X X - X - X - X X X - X
            X - X X X - X - X - X - X - X - X X X - X
            X - X X X - X - X X - X X - X - X X X - X
            X - - - - - X - X - X X X - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X - - - X - - - - - - - -
            X - X X X X X - - X X - X - X X X X X - -
            X - X - X - - - X - X - X - X - X - X - X
            X - X X - X X - X X - X X - X X - X X - X
            X - - - X X - - - - X X X - - - X X X - -
            X X X X X X X X X X X X X X X X X X X X X
            - - - - - - - - X - - - X X X - - - X X X
            X X X X X X X - - X X - X X - X X - X X -
            X - - - - - X - X - X - X - X - X - X - X
            X - X X X - X - X X - X X - X X - X X - X
            X - X X X - X - X - X X X - - - X X X - -
            X - X X X - X - X X X X X X X X X X X X X
            X - - - - - X - - - - - X X X - - - X X X
            X X X X X X X - X X X - X X - X X - X X -
            """, MatrixToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithMeadowMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternSeven_Meadow);
        Assert.Equal("""
            X X X X X X X - - - X - X - X X X X X X X
            X - - - - - X - - X X X - - X - - - - - X
            X - X X X - X - X - X X X - X - X X X - X
            X - X X X - X - X X - X - - X - X X X - X
            X - X X X - X - X - - - X - X - X X X - X
            X - - - - - X - X X - - - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - X X X X - - - - - - - - -
            X - X X X X X - - - X X X - X X X X X - -
            - X - X - X - X - X - X - X - X - X - X -
            X X X - - - X X X - - - X X X - - - X X X
            - X X X - - - X X X - - - X X X - - - X X
            X - X - X - X - X - X - X - X - X - X - X
            - - - - - - - - X X X X - - - X X X - - -
            X X X X X X X - - - X X X - - - X X X - -
            X - - - - - X - X X - X - X - X - X - X -
            X - X X X - X - X - - - X X X - - - X X X
            X - X X X - X - X X - - - X X X - - - X X
            X - X X X - X - X - X - X - X - X - X - X
            X - - - - - X - - X X X - - - X X X - - -
            X X X X X X X - X - X X X - - - X X X - -
            """, MatrixToString(m));
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("XXXX----XXXX----XXXX", 0)]
    [InlineData("XXXXX", 3)]
    [InlineData("-----", 3)]
    [InlineData("XXXXXX", 4)]
    [InlineData("""
        X X X X X X 
        X X X X X X 
        """, 8)]
    [InlineData("""
        X X X X X X 
        X X X X X X 
        - - - - - X 
        """, 11)]
    [InlineData("------", 4)]
    [InlineData("XXXX-XXX-X----", 40)]
    [InlineData("XXX----X-XXX-X", 40)]
    [InlineData("XXX----X-XXX-X----X-XXX-X", 80)]
    [InlineData("XXXXX-----X-XXX-X----X-XXX-X", 86)]
    [InlineData("XXXXXXX-XX---X-X--X-X-XXXXXXX", 10)]
    public void CalculateRowPenalty_ReturnsExpectedPenalty(string input, int expectedValue)
    {
        var m = InputToMatrix(input);
        var (modulePenalty, patternPenalty) = QrSymbolBuilder.CalculateRowPenalty(m);
        Assert.Equal(expectedValue, modulePenalty + patternPenalty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("XXXXX-----", 0)]
    [InlineData("XXXX-XXX-X----", 0)]
    [InlineData("""
        X - X - X
        X X - X -
        X - X - X
        X X - X -
        X - X - X
        """, 3)]
    [InlineData("""
        - - X - X
        - X - X -
        - - X - X
        - X - X -
        - - X - X
        - - - - -
        """, 4)]
    [InlineData("""
        - - X
        - - -
        - - X
        - - X
        X - X
        - X -
        X X X
        X X -
        X X -
        - X -
        X X -
        """, 87)]
    public void CalculateColumnPenalty_ReturnsExpectedPenalty(string input, int expectedValue)
    {
        var m = InputToMatrix(input);
        var (modulePenalty, patternPenalty) = QrSymbolBuilder.CalculateColumnPenalty(m);
        Assert.Equal(expectedValue, modulePenalty + patternPenalty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("X X X X X - - - - - ", 0)]
    [InlineData("X - X - X - X - X - X - ", 0)]
    [InlineData("""
        X X X - X -
        X X X - X -
        """, 6)]
    [InlineData("""
        X - - - X X - 
        X - - - X X - 
        """, 9)]
    [InlineData("""
        X X X
        X X X
        X X X
        """, 12)]
    [InlineData("""
        - - - -
        - - - -
        - - - -
        - - - -
        """, 27)]
    [InlineData("""
        X X X - - - X X - X
        X X X - - - X X - -
        X X X - - - X - - -
        """, 30)]
    public void CalculateBlocksPenalty_ReturnsExpectedPenalty(string input, int expectedValue)
    {
        var m = InputToMatrix(input);
        var penalty = QrSymbolBuilder.CalculateBlocksPenalty(m);
        Assert.Equal(expectedValue, penalty);
    }

    [Fact]
    public void CalculateRatioPenalty_ReturnsExpectedPenalty()
    {
        var totalModules = 100;
        for (var i = 0; i <= totalModules; i++)
        {
            var m = new BitMatrix(10, 10);

            var darkModuleCount = 0;
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var value = false;
                    if (darkModuleCount < i)
                    {
                        value = true;
                        darkModuleCount++;
                    }
                    m[x, y] = value;
                }
            }

            var chunks = Enumerable.Range(0, Math.Max(darkModuleCount, totalModules -  darkModuleCount) -  (totalModules / 2)).Chunk(5).ToArray();
            var expectedPenalty = chunks.Length == 0 ? 0 : (chunks.Length -  1) * 10;
            var penalty = QrSymbolBuilder.CalculateRatioPenalty(m);
            Assert.Equal(expectedPenalty, penalty);
        }
    }

    [Theory]
    [InlineData(200, 400, 0)]
    [InlineData(220, 400, 0)]
    [InlineData(180, 400, 0)]
    [InlineData(126, 841, 70)]
    [InlineData(563, 841, 30)]
    [InlineData(0, 841, 90)]
    [InlineData(841, 841, 90)]
    public void CalculateRatioPenalty_ReturnsExpectedPenalty2(int darkModules, int totalModules, int expectedPenalty)
    {
        var size = (int)double.Sqrt(totalModules);
        var m = new BitMatrix(size, size);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                m[x, y] = darkModules-- > 0;
            }
        }

        var penalty = QrSymbolBuilder.CalculateRatioPenalty(m);
        Assert.Equal(expectedPenalty, penalty);
    }

    [Fact]
    public void CalculatePenalty_ReturnsExpectedPenalty()
    {
        var matrix = InputToMatrix("""
            X X X X X X X - X X - - - X - X - - X - X - X X X X X X X
            X - - - - - X - X X X - - X - X - - X X - - X - - - - - X
            X - X X X - X - X - X - X - X X - - - X X - X - X X X - X
            X - X X X - X - - X - X - - X - - X - X - - X - X X X - X
            X - X X X - X - - - - - X X X X X - X X X - X - X X X - X
            X - - - - - X - X X X X X - - X - - X - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - X X - X X X X X - - - - - - - - - - - - -
            - - X X X - X - X - - - - - - - X - X X X X X X - - X X X
            - - - - - X - X X X X X X - X - X - - - - X X X X X X - X
            X - X X - X X - X X X X X X X X X - X - X X X X - - - - -
            X - X - X X - X X X - X X - X - X X X - - X X X - X - X -
            - X - X - X X X X - X - - X - - - - - - X X - X - - X X X
            - X X - X X - - X - - - X - X - X - - X X X X - X X - X X
            X X - X - X X - X X X - X - - X X - X - X - X X - - - - -
            X - X X X - - - X X - - X - - - - X X - X - - - X X - X -
            - X X - X X X - - X X - X - - X X - - - - X - - - X X X -
            X - X X X - - - X - X - X X - X X - - - X X X X X X - X X
            X - - X X - X - - X - X - X - X X X - X - - X X - X - - -
            X - X - - X - X - X X X X - - X X X - X X - X - X - - X -
            X - X X - X X - X - X X X X X X X - X X X X X X X - X - -
            - - - - - - - - X - - - X - X - X X X - X - - - X X - - X
            X X X X X X X - - - X - X X X X - X X X X - X - X - - - -
            X - - - - - X - - X X X X X - X X - X X X - - - X X - - -
            X - X X X - X - X X - - X - - - X X X - X X X X X X X - X
            X - X X X - X - X X - X X X X - - - X X X X - X - X X - -
            X - X X X - X - X X X - X - X - X X X X X X X X X - X X -
            X - - - - - X - - X - - - - X X X X - - - - X - X X - X -
            X X X X X X X - - - X X X X X X - - X - - - X - X - X - -
            """);
        var expectedPenalty = 641;
        var penalty = QrSymbolBuilder.CalculatePenalty(matrix);
        Assert.Equal(expectedPenalty, penalty);
    }

    private static BitMatrix InputToMatrix(string input)
    {
        BitMatrix? matrix = null;
        var span = input.AsSpan();
        var height = span.Count("\n") + 1;
        var y = 0;
        var lineLength = 0;
        foreach (var line in span.EnumerateLines())
        {
            matrix ??= new BitMatrix(line.ToString().Replace(" ", "").Length, height);
            lineLength = lineLength == 0 ? line.Length : lineLength;

            if (lineLength != line.Length)
            {
                throw new ArgumentException("All lines must have the same length.");
            }

            var spaceCount = 0;
            for (var x = 0; x < line.Length; x++)
            {
                if (line[x] == ' ')
                {
                    spaceCount++;
                    continue;
                }

                matrix[x - spaceCount, y] = line[x] == 'X';
            }
            y++;
        }
        return matrix!;
    }
}
