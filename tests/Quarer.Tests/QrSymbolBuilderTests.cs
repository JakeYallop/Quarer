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
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, QrMaskPattern.PatternZero_Checkerboard);

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
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.M, QrMaskPattern.PatternZero_Checkerboard);
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
        QrSymbolBuilder.EncodeFormatInformation(m, ErrorCorrectionLevel.H, QrMaskPattern.PatternZero_Checkerboard);
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
}
