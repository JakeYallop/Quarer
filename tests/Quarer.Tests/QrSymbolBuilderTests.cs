using System.Text;

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
            XXXXXXX-------XXXXXXX
            X-----X-------X-----X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-----X-------X-----X
            XXXXXXX-------XXXXXXX
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            XXXXXXX--------------
            X-----X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-----X--------------
            XXXXXXX--------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            XXXXXXXX-----XXXXXXXX
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            ---------------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            XXXXXXXX-------------
            """, ChangesToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXX-------XXXXXXX
            X-----X-------X-----X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-----X-------X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            ------X--------------
            ---------------------
            ------X--------------
            ---------------------
            ------X--------------
            ---------------------
            XXXXXXX--------------
            X-----X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-----X--------------
            XXXXXXX--------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXX-------XXXXXXX
            X-----X-------X-----X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-----X-------X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            ------X--------------
            ---------------------
            ------X--------------
            ---------------------
            ------X--------------
            --------X------------
            XXXXXXX--------------
            X-----X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-XXX-X--------------
            X-----X--------------
            XXXXXXX--------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXX-------XXXXXXX
            X-----X-X-----X-----X
            X-XXX-X-------X-XXX-X
            X-XXX-X-------X-XXX-X
            X-XXX-X-X-----X-XXX-X
            X-----X-------X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            X-X-X-X---------X--X-
            ---------------------
            ------X--------------
            ---------------------
            ------X--------------
            ---------------------
            XXXXXXX--------------
            X-----X--------------
            X-XXX-X-X------------
            X-XXX-X--------------
            X-XXX-X-X------------
            X-----X--------------
            XXXXXXX-X------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
                Assert.True(m.IsEmpty(j, size - 11 + i));
                Assert.True(m.IsEmpty(size - 11 + i, j));
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
                Assert.False(m.IsEmpty(j, size - 11 + i));
                Assert.False(m.IsEmpty(size - 11 + i, j));
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
            XXXXXXX-------------------------------XXXXXXX
            X-----X-------------------------------X-----X
            X-XXX-X-------------------------------X-XXX-X
            X-XXX-X-------------------------------X-XXX-X
            X-XXX-X-------------XXXXX-------------X-XXX-X
            X-----X-------------X---X-------------X-----X
            XXXXXXX-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-XXXXXXX
            --------------------X---X--------------------
            ------X-------------XXXXX--------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ----XXXXX-----------XXXXX-----------XXXXX----
            ----X---X-----------X---X-----------X---X----
            ----X-X-X-----------X-X-X-----------X-X-X----
            ----X---X-----------X---X-----------X---X----
            ----XXXXX-----------XXXXX-----------XXXXX----
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X--------------------------------------
            ---------------------------------------------
            ------X-------------XXXXX-----------XXXXX----
            --------X-----------X---X-----------X---X----
            XXXXXXX-------------X-X-X-----------X-X-X----
            X-----X-------------X---X-----------X---X----
            X-XXX-X-------------XXXXX-----------XXXXX----
            X-XXX-X--------------------------------------
            X-XXX-X--------------------------------------
            X-----X--------------------------------------
            XXXXXXX--------------------------------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXX-----------XXXXXXX
            X-----X-X---------X-----X
            X-XXX-X-----------X-XXX-X
            X-XXX-X-----------X-XXX-X
            X-XXX-X-X---------X-XXX-X
            X-----X-----------X-----X
            XXXXXXX-X-X-X-X-X-XXXXXXX
            -------------------------
            X-X-X-X-------------X--X-
            -------------------------
            ------X------------------
            -------------------------
            ------X------------------
            -------------------------
            ------X------------------
            -------------------------
            ------X---------XXXXX----
            --------X-------X---X----
            XXXXXXX---------X-X-X----
            X-----X---------X---X----
            X-XXX-X-X-------XXXXX----
            X-XXX-X------------------
            X-XXX-X-X----------------
            X-----X------------------
            XXXXXXX-X----------------
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);

        var codewordsBuffer = AlternatingCodewordsBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, maskPattern: null);

        Assert.Equal("""
            XXXXXXX--X---X----XXXXXXX
            X-----X-XXX--XX---X-----X
            X-XXX-X--XX--XX---X-XXX-X
            X-XXX-X--XX-XXX-X-X-XXX-X
            X-XXX-X-X-XXX-XXX-X-XXX-X
            X-----X----XX--XX-X-----X
            XXXXXXX-X-X-X-X-X-XXXXXXX
            -----------XX--XX--------
            X-X-X-X----X---X----X--X-
            X---X----X---X---------XX
            XX--XXX--XX--XX--XX----XX
            XX--XX---XX--XX--XX----XX
            XX-XXXX-XXX-XXX-XXXXX--XX
            -XXX-X-XX-XXX-XXXXXXXXX--
            --XX--XXX--XX--XX--XXXX--
            --XX---XX--XX--XX--XXXX--
            --X---XX---X---XXXXXXXX--
            --------XX---X--X---X--XX
            XXXXXXX--XX--XX-X-X-X--XX
            X-----X--XX--XX-X---X--XX
            X-XXX-X-XXX-XXX-XXXXX--XX
            X-XXX-X---XXX-X------XX--
            X-XXX-X-X--XX--------XX--
            X-----X----XX--XXXX--XX--
            XXXXXXX-X--X---XXXX--XX--
            """, ToString(m));
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
            XXXXXXX-XXXXXXXXXXXXXXX----------X--X-XXXXXXX
            X-----X----XX--XX--XXXX----------X-X--X-----X
            X-XXX-X----XX--XX--XXXX--XXXXXXXXX-X--X-XXX-X
            X-XXX-X-X------------XX--XXXXXXXXX-XX-X-XXX-X
            X-XXX-X-------------XXXXXXXXXXXXXXXXX-X-XXX-X
            X-----X--XX--XX--XX-X---XXXXXXXXXX----X-----X
            XXXXXXX-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-XXXXXXX
            ---------XX--XX--XX-X---X----------XX--------
            --X-XXX-XXXXXXXXXXX-XXXXX------------X---X--X
            XXXX-----XXXXXXXXXX----XX----------------XX--
            --XX--X----XX--XX--XX--XX----------------XX--
            --XXXX-XX--XX--XX--XX--XXXXXXXXXXXX--XXXXXX--
            ----XXXXX----------XX--XXXXXXXXXXXXXXXXXXXX--
            ----XX-XX----------XXXX--XXXXXXXXXXXXXXXX--XX
            XX--XXXXXXX--XX--XX--XX--XXXXXXXXXXXXXXXX--XX
            XX-------XX--XX--XX--XX------------XX------XX
            XXXX--X--XXXXXXXXXX--XX--------------------XX
            XXXX-----XXXXXXXXXX----XX----------------XX--
            --XX--X----XX--XX--XX--XX----------------XX--
            --XXXX-XX--XX--XX--XX--XXXXXXXXXXXX--XXXXXX--
            ----XXXXX----------XXXXXXXXXXXXXXXXXXXXXXXX--
            ----X---X----------XX---XXXXXXXXXXXXX---X--XX
            XX--X-X-XXX--XX--XXXX-X-XXXXXXXXXXXXX-X-X--XX
            XX--X---XXX--XX--XXXX---X----------XX---X--XX
            XXXXXXXXXXXXXXXXXXX-XXXXX----------XXXXXX--XX
            XXXXXX-XXXXXXXXXXXX----XX----------XXXXXXXX--
            --XXXXXXX--XX--XX----XX------------X-XXXXXX--
            --XXXX-XX--XX--XX----XX--XXXXXXXXXX--XXXXXX--
            ------X-------------XXX--XXXXXXXXXX------XX--
            -------------------XXXX--XXXXXXXXXX--------XX
            XX----X--XX--XX--XXXX--XXXXXXXXXXXX-X------XX
            XX-------XX--XX--XXXX--XX----------XX------XX
            XXXXXXXXXXXXXXXXXXXX---XX----------XXXXXX--XX
            XXXXXX-XXXXXXXXXXXX----XX----------XXXXXXXX--
            ----X-XXX--XX--XX----XX------------X-XXXXXX--
            -XXXX--XX--XX--XX----XX--XXXXXXXXXX--XXXXXX--
            X--XX-X-------------XXXXXXXXXXXXXXX-XXXXXXX--
            --------X----------XX---XXXXXXXXXXX-X---X--XX
            XXXXXXX--XX--XX--XXXX-X-XXXXXXXXXXX-X-X-X--XX
            X-----X-XXX--XX--XXXX---X-----------X---X--XX
            X-XXX-X-XXXXXXXXXXXXXXXXX-----------XXXXX--XX
            X-XXX-X--XXXXXXXXXXXXXX------------XX----XX--
            X-XXX-X-X--XX--XX--XXXX------------XX----XX--
            X-----X----XX--XX------XXXXXXXXXXXXXX----XX--
            XXXXXXX----------------XXXXXXXXXXXXXX----XX--
            """, ToString(m), ignoreLineEndingDifferences: true, ignoreAllWhiteSpace: true);
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
            XXXXXXX---X-X-XXXXXXX
            X-----X-XX-X--X-----X
            X-XXX-X---X-X-X-XXX-X
            X-XXX-X--X-X--X-XXX-X
            X-XXX-X-X-X-X-X-XXX-X
            X-----X--X-X--X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------X-X---------
            X-X-X-X---X-X---X--X-
            -X-X-X-X-X-X-X-X-X-X-
            X-X-X-X-X-X-X-X-X-X-X
            -X-X-X-X-X-X-X-X-X-X-
            X-X-X-X-X-X-X-X-X-X-X
            --------XX-X-X-X-X-X-
            XXXXXXX---X-X-X-X-X-X
            X-----X--X-X-X-X-X-X-
            X-XXX-X-X-X-X-X-X-X-X
            X-XXX-X--X-X-X-X-X-X-
            X-XXX-X-X-X-X-X-X-X-X
            X-----X--X-X-X-X-X-X-
            XXXXXXX-X-X-X-X-X-X-X
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithHorizontalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternOne_HorizontalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternOne_HorizontalLines);
        Assert.Equal("""
            XXXXXXX-XXXXX-XXXXXXX
            X-----X-------X-----X
            X-XXX-X-XXXXX-X-XXX-X
            X-XXX-X-------X-XXX-X
            X-XXX-X--XXXX-X-XXX-X
            X-----X-X-----X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            X-X---XX-XXXX--X--X-X
            ---------------------
            XXXXXXXXXXXXXXXXXXXXX
            ---------------------
            XXXXXXXXXXXXXXXXXXXXX
            --------X------------
            XXXXXXX-XXXXXXXXXXXXX
            X-----X--------------
            X-XXX-X--XXXXXXXXXXXX
            X-XXX-X--------------
            X-XXX-X-XXXXXXXXXXXXX
            X-----X--------------
            XXXXXXX-XXXXXXXXXXXXX
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithVerticalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternTwo_VerticalLines);
        Assert.Equal("""
            XXXXXXX--X--X-XXXXXXX
            X-----X--X--X-X-----X
            X-XXX-X-XX--X-X-XXX-X
            X-XXX-X-XX--X-X-XXX-X
            X-XXX-X-XX--X-X-XXX-X
            X-----X-XX--X-X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------XX--X--------
            X-XXXXX--X--X-XXXXX--
            X--X-----X--X--X--X--
            X--X--X--X--X--X--X--
            X--X-----X--X--X--X--
            X--X--X--X--X--X--X--
            --------XX--X--X--X--
            XXXXXXX--X--X--X--X--
            X-----X-XX--X--X--X--
            X-XXX-X-XX--X--X--X--
            X-XXX-X-XX--X--X--X--
            X-XXX-X-XX--X--X--X--
            X-----X--X--X--X--X--
            XXXXXXX-XX--X--X--X--
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithDiagonalLinesMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternThree_DiagonalLines);
        Assert.Equal("""
            XXXXXXX--X--X-XXXXXXX
            X-----X----X--X-----X
            X-XXX-X-X-X---X-XXX-X
            X-XXX-X-XX--X-X-XXX-X
            X-XXX-X-X--X--X-XXX-X
            X-----X-X-X---X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------X--X---------
            X-XXXXX---X---XXXXX--
            X--X-----X--X--X--X--
            --X--XX-X--X--X--X--X
            -X--X--X--X--X--X--X-
            X--X--X--X--X--X--X--
            --------X--X--X--X--X
            XXXXXXX---X--X--X--X-
            X-----X-XX--X--X--X--
            X-XXX-X-X--X--X--X--X
            X-XXX-X-X-X--X--X--X-
            X-XXX-X-XX--X--X--X--
            X-----X----X--X--X--X
            XXXXXXX-X-X--X--X--X-
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithLargeCheckboardMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternFour_LargeCheckerboard);
        Assert.Equal("""
            XXXXXXX-----X-XXXXXXX
            X-----X-----X-X-----X
            X-XXX-X-XXXX--X-XXX-X
            X-XXX-X-XXXX--X-XXX-X
            X-XXX-X-X---X-X-XXX-X
            X-----X-X---X-X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------XXXX---------
            X-XXXXX-----X-XXXXX--
            XXX----XX---XXX---XXX
            ---XXXX--XXX---XXX---
            ---XXX---XXX---XXX---
            XXX---XXX---XXX---XXX
            --------X---XXX---XXX
            XXXXXXX--XXX---XXX---
            X-----X-XXXX---XXX---
            X-XXX-X-X---XXX---XXX
            X-XXX-X-X---XXX---XXX
            X-XXX-X-XXXX---XXX---
            X-----X--XXX---XXX---
            XXXXXXX-X---XXX---XXX
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithFieldsMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternFive_Fields);
        Assert.Equal("""
            XXXXXXX--XXXX-XXXXXXX
            X-----X-----X-X-----X
            X-XXX-X-XX--X-X-XXX-X
            X-XXX-X-X-X-X-X-XXX-X
            X-XXX-X-XX--X-X-XXX-X
            X-----X-X---X-X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------X---X--------
            X-XXXXX--X--X-XXXXX--
            X-X-X---X-X-X-X-X-X-X
            X--X--X--X--X--X--X--
            X-----------X-----X--
            XXXXXXXXXXXXXXXXXXXXX
            --------X---X-----X--
            XXXXXXX--X--X--X--X--
            X-----X-X-X-X-X-X-X-X
            X-XXX-X-XX--X--X--X--
            X-XXX-X-X---X-----X--
            X-XXX-X-XXXXXXXXXXXXX
            X-----X-----X-----X--
            XXXXXXX-XX--X--X--X--
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithDiamondsMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternSix_Diamonds);
        Assert.Equal("""
            XXXXXXX--XXXX-XXXXXXX
            X-----X-----X-X-----X
            X-XXX-X-XXX-X-X-XXX-X
            X-XXX-X-X-X-X-X-XXX-X
            X-XXX-X-XX-XX-X-XXX-X
            X-----X-X-XXX-X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------X---X--------
            X-XXXXX--XX-X-XXXXX--
            X-X-X---X-X-X-X-X-X-X
            X-XX-XX-XX-XX-XX-XX-X
            X---XX----XXX---XXX--
            XXXXXXXXXXXXXXXXXXXXX
            --------X---XXX---XXX
            XXXXXXX--XX-XX-XX-XX-
            X-----X-X-X-X-X-X-X-X
            X-XXX-X-XX-XX-XX-XX-X
            X-XXX-X-X-XXX---XXX--
            X-XXX-X-XXXXXXXXXXXXX
            X-----X-----XXX---XXX
            XXXXXXX-XXX-XX-XX-XX-
            """, ToString(m));
    }

    [Fact]
    public void Encode_EncodeDataBits_WithMeadowMasking_EncodesExpectedSymbol()
    {
        var (m, version) = Get1MSymbolWithoutData(MaskPattern.PatternTwo_VerticalLines);
        var codewordsBuffer = AllZeroBuffer(version.TotalCodewords);
        QrSymbolBuilder.EncodeDataBits(m, version, codewordsBuffer, MaskPattern.PatternSeven_Meadow);
        Assert.Equal("""
            XXXXXXX---X-X-XXXXXXX
            X-----X--XXX--X-----X
            X-XXX-X-X-XXX-X-XXX-X
            X-XXX-X-XX-X--X-XXX-X
            X-XXX-X-X---X-X-XXX-X
            X-----X-XX----X-----X
            XXXXXXX-X-X-X-XXXXXXX
            --------XXXX---------
            X-XXXXX---XXX-XXXXX--
            -X-X-X-X-X-X-X-X-X-X-
            XXX---XXX---XXX---XXX
            -XXX---XXX---XXX---XX
            X-X-X-X-X-X-X-X-X-X-X
            --------XXXX---XXX---
            XXXXXXX---XXX---XXX--
            X-----X-XX-X-X-X-X-X-
            X-XXX-X-X---XXX---XXX
            X-XXX-X-XX---XXX---XX
            X-XXX-X-X-X-X-X-X-X-X
            X-----X--XXX---XXX---
            XXXXXXX-X-XXX---XXX--
            """, ToString(m));
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
            matrix ??= new BitMatrix(line.Length, height);
            lineLength = lineLength == 0 ? line.Length : lineLength;

            if (lineLength != line.Length)
            {
                throw new ArgumentException("All lines must have the same length.");
            }

            for (var x = 0; x < line.Length; x++)
            {
                matrix[x, y] = line[x] == 'X';
            }
            y++;
        }
        return matrix!;
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("XXXX----XXXX----XXXX", 0)]
    [InlineData("XXXXX", 3)]
    [InlineData("-----", 3)]
    [InlineData("XXXXXX", 4)]
    [InlineData("XXXXXX\nXXXXXX", 8)]
    [InlineData("XXXXXX\nXXXXXX\n-----X", 11)]
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
        X-X-X
        XX-X-
        X-X-X
        XX-X-
        X-X-X
        """, 3)]
    [InlineData("""
        --X-X
        -X-X-
        --X-X
        -X-X-
        --X-X
        -----
        """, 4)]
    [InlineData("""
        --X
        ---
        --X
        --X
        X-X
        -X-
        XXX
        XX-
        XX-
        -X-
        XX-
        """, 87)]
    public void CalculateColumnPenalty_ReturnsExpectedPenalty(string input, int expectedValue)
    {
        var m = InputToMatrix(input);
        var (modulePenalty, patternPenalty) = QrSymbolBuilder.CalculateColumnPenalty(m);
        Assert.Equal(expectedValue, modulePenalty + patternPenalty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("XXXXX-----", 0)]
    [InlineData("X-X-X-\nX-X-X-", 0)]
    [InlineData("XXX-X-\nXXX-X-", 6)]
    [InlineData("X---XX-\nX---XX-", 9)]
    [InlineData("""
        XXX
        XXX
        XXX
        """, 12)]
    [InlineData("""
        ----
        ----
        ----
        ----
        """, 27)]
    [InlineData("""
        XXX---XX-X
        XXX---XX--
        XXX---X---
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

            var chunks = Enumerable.Range(0, Math.Max(darkModuleCount, totalModules - darkModuleCount) - (totalModules / 2)).Chunk(5).ToArray();
            var expectedPenalty = chunks.Length == 0 ? 0 : (chunks.Length - 1) * 10;
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
            XXXXXXX-XX---X-X--X-X-XXXXXXX
            X-----X-XXX--X-X--XX--X-----X
            X-XXX-X-X-X-X-XX---XX-X-XXX-X
            X-XXX-X--X-X--X--X-X--X-XXX-X
            X-XXX-X-----XXXXX-XXX-X-XXX-X
            X-----X-XXXXX--X--X---X-----X
            XXXXXXX-X-X-X-X-X-X-X-XXXXXXX
            --------XX-XXXXX-------------
            --XXX-X-X-------X-XXXXXX--XXX
            -----X-XXXXXX-X-X----XXXXXX-X
            X-XX-XX-XXXXXXXXX-X-XXXX-----
            X-X-XX-XXX-XX-X-XXX--XXX-X-X-
            -X-X-XXXX-X--X------XX-X--XXX
            -XX-XX--X---X-X-X--XXXX-XX-XX
            XX-X-XX-XXX-X--XX-X-X-XX-----
            X-XXX---XX--X----XX-X---XX-X-
            -XX-XXX--XX-X--XX----X---XXX-
            X-XXX---X-X-XX-XX---XXXXXX-XX
            X--XX-X--X-X-X-XXX-X--XX-X---
            X-X--X-X-XXXX--XXX-XX-X-X--X-
            X-XX-XX-X-XXXXXXX-XXXXXXX-X--
            --------X---X-X-XXX-X---XX--X
            XXXXXXX---X-XXXX-XXXX-X-X----
            X-----X--XXXXX-XX-XXX---XX---
            X-XXX-X-XX--X---XXX-XXXXXXX-X
            X-XXX-X-XX-XXXX---XXXX-X-XX--
            X-XXX-X-XXX-X-X-XXXXXXXXX-XX-
            X-----X--X----XXXX----X-XX-X-
            XXXXXXX---XXXXXX--X---X-X-X--
            """);
        var expectedPenalty = 641;
        var penalty = QrSymbolBuilder.CalculatePenalty(matrix);
        Assert.Equal(expectedPenalty, penalty);
    }

    private static string ToString(BitMatrix m)
    {
        var sb = new StringBuilder(m.Width * m.Height);
        for (var y = 0; y < m.Height; y++)
        {
            for (var x = 0; x < m.Width; x++)
            {
                sb.Append(m[x, y] ? 'X' : '-');
            }

            if (y + 1 < m.Height)
            {
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private static string ChangesToString(TrackedBitMatrix m)
    {
        var sb = new StringBuilder(m.Width * m.Height);
        for (var y = 0; y < m.Height; y++)
        {
            for (var x = 0; x < m.Width; x++)
            {
                sb.Append(m.IsEmpty(x, y) ? '-' : 'X');
            }

            if (y + 1 < m.Height)
            {
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private static BitBuffer AlternatingCodewordsBuffer(int n)
    {
        var buffer = new BitBuffer(n * 8);
        var writer = new BitWriter(buffer);
        for (var i = 0; i < n; i++)
        {
            var b = (byte)(i % 2 == 0 ? 0 : 255);
            writer.WriteBitsBigEndian(b, 8);
        }
        return buffer;
    }

    private static BitBuffer AllZeroBuffer(int n)
    {
        var buffer = new BitBuffer(n * 8);
        var writer = new BitWriter(buffer);
        for (var i = 0; i < n; i++)
        {
            writer.WriteBitsBigEndian(0, 8);
        }
        return buffer;
    }
}
