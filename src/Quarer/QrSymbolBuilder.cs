using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Quarer;

public static class QrSymbolBuilder
{
    public static (ByteMatrix Matrix, MaskPattern SelectedMaskPattern) BuildSymbol(BitBuffer dataCodewords, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, MaskPattern? maskPattern = null)
        => BuildSymbol(new ByteMatrix(version.ModulesPerSide, version.ModulesPerSide), dataCodewords, version, errorCorrectionLevel, maskPattern);

    private static (ByteMatrix Matrix, MaskPattern SelectedMaskPattern) BuildSymbol(ByteMatrix matrix, BitBuffer dataCodewords, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, MaskPattern? maskPattern = null)
    {
        EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.TopLeft);
        EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.TopRight);
        EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.BottomLeft);

        EncodeStaticDarkModule(matrix);

        EncodePositionAdjustmentPatterns(matrix, version);

        EncodeTimingPatterns(matrix);

        EncodeVersionInformation(matrix, version);

        if (maskPattern is not null)
        {
            EncodeFormatInformation(matrix, errorCorrectionLevel, maskPattern.Value);
            EncodeDataBits(matrix, version, dataCodewords);
            ApplyMask(matrix, version, maskPattern.Value);
            return (matrix, maskPattern.Value);
        }

        // otherwise, determine the best mask pattern to use

        var patterns = (ReadOnlySpan<MaskPattern>)[MaskPattern.PatternZero_Checkerboard, MaskPattern.PatternOne_HorizontalLines, MaskPattern.PatternTwo_VerticalLines, MaskPattern.PatternThree_DiagonalLines, MaskPattern.PatternFour_LargeCheckerboard, MaskPattern.PatternFive_Fields, MaskPattern.PatternSix_Diamonds, MaskPattern.PatternSeven_Meadow];

        var lowestPenalty = int.MaxValue;
        var selectedMaskPattern = patterns[0];

        EncodeDataBits(matrix, version, dataCodewords);

        foreach (var pattern in patterns)
        {
            ApplyMask(matrix, version, pattern);
            var penalty = CalculatePenalty(matrix);
            if (penalty < lowestPenalty)
            {
                lowestPenalty = penalty;
                selectedMaskPattern = pattern;
            }
            // undo the mask
            ApplyMask(matrix, version, pattern);
        }

        EncodeFormatInformation(matrix, errorCorrectionLevel, selectedMaskPattern);
        ApplyMask(matrix, version, selectedMaskPattern);
        return (matrix, selectedMaskPattern);
    }

    private readonly ref struct Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    }

    private static int MaxAbsoluteValue(Point p) => Math.Max(Math.Abs(p.X), Math.Abs(p.Y));

    public static void EncodePositionDetectionPattern(ByteMatrix matrix, PositionDetectionPatternLocation location)
    {
        //version 1 code is 21x21
        if (matrix.Width < 21 || matrix.Height < 21)
        {
            throw new InvalidOperationException("Matrix size is too small.");
        }

        if (!Enum.IsDefined(location))
        {
            throw new ArgumentOutOfRangeException(nameof(location));
        }

        var (xStart, yStart) = location switch
        {
            PositionDetectionPatternLocation.TopLeft => (0, 0),
            PositionDetectionPatternLocation.TopRight => (matrix.Width - 7, 0),
            PositionDetectionPatternLocation.BottomLeft => (0, matrix.Height - 7),
            _ => throw new UnreachableException()
        };

        for (var y = 0; y < 7; ++y)
        {
            for (var x = 0; x < 7; ++x)
            {
                // see explanation in EmbedPositionAdjustmentPattern for how this works
                matrix[xStart + x, yStart + y] = (MaxAbsoluteValue(new Point(x, y) - new Point(3, 3)) != 2) ? (byte)1 : (byte)0;
            }
        }

        EncodeSeparatorPattern(matrix, xStart, yStart);
    }

    private static void EncodeSeparatorPattern(ByteMatrix matrix, int xStart, int yStart)
    {
        static void SetIfInside(ByteMatrix matrix, int x, int y)
        {
            if (x >= 0 && x < matrix.Width && y >= 0 && y < matrix.Height)
            {
                matrix[x, y] = 0;
            }
        }

        for (var i = -1; i < 8; ++i)
        {
            SetIfInside(matrix, xStart + i, yStart - 1); // top
            SetIfInside(matrix, xStart + i, yStart + 7); // bottom
            SetIfInside(matrix, xStart - 1, yStart + i); // left
            SetIfInside(matrix, xStart + 7, yStart + i); // right
        }
    }

    public static void EncodeStaticDarkModule(ByteMatrix matrix) => matrix[8, matrix.Height - 8] = 1;

    /// <summary>
    /// Encode position adjustment patterns into the symbol. This step must be done bfore encoding the timing patterns.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="version"></param>
    public static void EncodePositionAdjustmentPatterns(ByteMatrix matrix, QrVersion version)
    {
        // version 1 QrCode does not have any alignment patterns
        if (version.Version <= 1)
        {
            return;
        }

        if (matrix[6, 9] == 1)
        {
            throw new InvalidOperationException("Timing patterns or position adjustment patterns have already been encoded. Ensure adjustment patterns are added before timing patters.");
        }

        var positions = version.AlignmentPatternCenters;

        for (var y = 0; y < positions.Length; y++)
        {
            for (var x = 0; x < positions.Length; x++)
            {
                if (DoesNotOverlapFinderPatterns(matrix, positions[x], positions[y]))
                {
                    EncodePositionAdjustmentPattern(matrix, positions[x], positions[y]);
                }
            }
        }
    }

    private static bool DoesNotOverlapFinderPatterns(ByteMatrix matrix, int x, int y)
        => (y > 7 && y < matrix.Height - 8) || (x > 7 && x < matrix.Width - 8) || (x > matrix.Width - 8 && y > matrix.Height - 8);

    private static void EncodePositionAdjustmentPattern(ByteMatrix matrix, int centreX, int centreY)
    {
        Debug.Assert(matrix.Width >= 21 && matrix.Height >= 21);
        Debug.Assert(centreX >= 2 && centreY >= 2);
        for (var dy = -2; dy <= 2; dy++)
        {
            for (var dx = -2; dx <= 2; dx++)
            {
                // Explanation
                // Counting the rings from the center of the alignment pattern:
                //
                // 0 ┌───┐
                // 1 │   │<-- "ring" 2
                // 3 │ |<-- "ring" 0
                // 2 │   <-- "ring" 1
                // 4 └───
                //
                // we don't want to fill ring 1 here.
                // Taking the max absolute value of the coordinates effectively tells us which ring we are on,
                // with 0 being the center ring.
                // Below, we are saying "fill this coordinate with a dark module (true) if it's not in ring 1".
                matrix[centreX + dx, centreY + dy] = MaxAbsoluteValue(new Point(dx, dy)) != 1 ? (byte)1 : (byte)0;
            }
        }
    }

    /// <summary>
    /// Encode timing patterns into the symbol.
    /// </summary>
    /// <param name="matrix"></param>
    public static void EncodeTimingPatterns(ByteMatrix matrix)
    {
        // skip position detection and separator patterns
        // no special handling of alignment patterns is necessary as the alignments patterns line up
        // with respect to the light and dark module alternation of the timing patterns
        for (var i = 8; i < matrix.Width - 8; i++)
        {
            //var bit = i % 2 == 0;
            var bit = (byte)((i & 0x1) ^ 1);
            matrix[i, 6] = bit;
            matrix[6, i] = bit;
        }
    }

    /// <summary>
    /// The generator polynomial used in calculating the BCH code for format information.
    /// <para>
    /// <c>x^10 + x^8 + x^5 + x^4 + x^2 + x + 1</c>
    /// </para>
    /// </summary>
    public const ushort FormatInformationGeneratorPolynomial = 0b10100110111;
    /// <summary>
    /// Mask used for masking format information encoded in a BCH code.
    /// </summary>
    public const ushort FormatBchCodeMask = 0b101_0100_0001_0010;

    /// <summary>
    /// The generator polynomial used in calculating the BCH code for version information.
    /// <para>
    /// <c>x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1</c>
    /// </para>
    /// </summary>
    public const int VersionInformationGeneratorPolynomial = 0b1111100100101;

    public static void EncodeFormatInformation(ByteMatrix matrix, ErrorCorrectionLevel errorCorrectionLevel, MaskPattern maskPattern)
    {
        var formatInformation = GetFormatInformation(errorCorrectionLevel, (byte)maskPattern);
        var size = matrix.Width;
        Debug.Assert(matrix.Width == matrix.Height);

        matrix[8, 0] = (formatInformation & 0b000_0000_0000_0001) != 0 ? (byte)1 : (byte)0;
        matrix[8, 1] = (formatInformation & 0b000_0000_0000_0010) != 0 ? (byte)1 : (byte)0;
        matrix[8, 2] = (formatInformation & 0b000_0000_0000_0100) != 0 ? (byte)1 : (byte)0;
        matrix[8, 3] = (formatInformation & 0b000_0000_0000_1000) != 0 ? (byte)1 : (byte)0;
        matrix[8, 4] = (formatInformation & 0b000_0000_0001_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, 5] = (formatInformation & 0b000_0000_0010_0000) != 0 ? (byte)1 : (byte)0;
        // skipped - timing pattern
        matrix[8, 7] = (formatInformation & 0b000_0000_0100_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, 8] = (formatInformation & 0b000_0000_1000_0000) != 0 ? (byte)1 : (byte)0;

        matrix[8, size - 7] = (formatInformation & 0b000_0001_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 6] = (formatInformation & 0b000_0010_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 5] = (formatInformation & 0b000_0100_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 4] = (formatInformation & 0b000_1000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 3] = (formatInformation & 0b001_0000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 2] = (formatInformation & 0b010_0000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[8, size - 1] = (formatInformation & 0b100_0000_0000_0000) != 0 ? (byte)1 : (byte)0;

        matrix[size - 1, 8] = (formatInformation & 0b000_0000_0000_0001) != 0 ? (byte)1 : (byte)0;
        matrix[size - 2, 8] = (formatInformation & 0b000_0000_0000_0010) != 0 ? (byte)1 : (byte)0;
        matrix[size - 3, 8] = (formatInformation & 0b000_0000_0000_0100) != 0 ? (byte)1 : (byte)0;
        matrix[size - 4, 8] = (formatInformation & 0b000_0000_0000_1000) != 0 ? (byte)1 : (byte)0;
        matrix[size - 5, 8] = (formatInformation & 0b000_0000_0001_0000) != 0 ? (byte)1 : (byte)0;
        matrix[size - 6, 8] = (formatInformation & 0b000_0000_0010_0000) != 0 ? (byte)1 : (byte)0;
        matrix[size - 7, 8] = (formatInformation & 0b000_0000_0100_0000) != 0 ? (byte)1 : (byte)0;
        matrix[size - 8, 8] = (formatInformation & 0b000_0000_1000_0000) != 0 ? (byte)1 : (byte)0;

        matrix[7, 8] = (formatInformation & 0b000_0001_0000_0000) != 0 ? (byte)1 : (byte)0;
        // skipped - timing pattern
        matrix[5, 8] = (formatInformation & 0b000_0010_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[4, 8] = (formatInformation & 0b000_0100_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[3, 8] = (formatInformation & 0b000_1000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[2, 8] = (formatInformation & 0b001_0000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[1, 8] = (formatInformation & 0b010_0000_0000_0000) != 0 ? (byte)1 : (byte)0;
        matrix[0, 8] = (formatInformation & 0b100_0000_0000_0000) != 0 ? (byte)1 : (byte)0;
    }

    private static ushort GetFormatInformation(ErrorCorrectionLevel errorCorrectionLevel, byte maskPattern)
    {
        var formatInfo = errorCorrectionLevel switch
        {
            ErrorCorrectionLevel.L => FormatInformationL[maskPattern],
            ErrorCorrectionLevel.M => FormatInformationM[maskPattern],
            ErrorCorrectionLevel.Q => FormatInformationQ[maskPattern],
            ErrorCorrectionLevel.H => FormatInformationH[maskPattern],
            _ => throw new UnreachableException()
        };
        return formatInfo;
    }

    public static void EncodeVersionInformation(ByteMatrix matrix, QrVersion version)
    {
        if (version.Version is < 7)
        {
            return;
        }

        var versionInformation = VersionInformation[version.Version];
        var size = version.ModulesPerSide;

        Debug.Assert(matrix.Width == matrix.Height);
        if (size != matrix.Width)
        {
            throw new InvalidOperationException("Matrix size does not match version size.");
        }

        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                var v = (versionInformation >> ((j * 3) + i)) & 1;
                matrix[j, size - 11 + i] = v != 0 ? (byte)1 : (byte)0;
                matrix[size - 11 + i, j] = v != 0 ? (byte)1 : (byte)0;
            }
        }
    }

    [SkipLocalsInit]
    public static void EncodeDataBits(ByteMatrix matrix, QrVersion version, BitBuffer data)
    {
        if (version.TotalCodewords != data.ByteCount)
        {
            throw new InvalidOperationException("Data byte count does not match total codewords for QR code version.");
        }

        var functionModules = FunctionModules.GetForVersion(version);

#pragma warning disable IDE0057 // Use range operator //https://github.com/dotnet/roslyn/issues/74960
        Span<byte> yRangeValues = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.ModulesPerSide);
        Span<byte> yRangeValuesReverse = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.ModulesPerSide);
#pragma warning restore IDE0057 // Use range operator

        Span<byte> xRangeValues = stackalloc byte[QrVersion.MaxModulesPerSide / 2];
        var xRange = XRange(version, xRangeValues);
        var yRangeTopDown = YRangeTopDown(yRangeValues);
        var yRangeBottomUp = YRangeBottomUp(yRangeValuesReverse);

        var reverse = false;
        var bitIndex = 0;
        foreach (var x in xRange)
        {
            var yRange = reverse ? yRangeTopDown : yRangeBottomUp;

            if (bitIndex == data.Count)
            {
                break;
            }

            foreach (var y in yRange)
            {
                if (bitIndex == data.Count)
                {
                    break;
                }

                if (!functionModules.IsFunctionModule(x, y))
                {
                    var bit = data[bitIndex];
                    matrix[x, y] = bit ? (byte)1 : (byte)0;
                    bitIndex++;
                }

                if (bitIndex == data.Count)
                {
                    break;
                }

                if (!functionModules.IsFunctionModule(x - 1, y))
                {
                    var bit = data[bitIndex];
                    matrix[x - 1, y] = bit ? (byte)1 : (byte)0;
                    bitIndex++;
                }
            }
            reverse = !reverse;
        }

        Debug.Assert(bitIndex == (version.TotalCodewords * 8));
    }

    public static void ApplyMask(ByteMatrix matrix, QrVersion version, MaskPattern mask)
    {
        var functionModules = FunctionModules.GetForVersion(version);
        var patternOffsetFunc = GetPatternVectorOffsetFunc(mask);
        var patternVector = mask switch
        {
            MaskPattern.PatternZero_Checkerboard => CheckerboardPatternVector,
            MaskPattern.PatternOne_HorizontalLines => HorizontalLinesPatternVector,
            MaskPattern.PatternTwo_VerticalLines => VerticalLinesPatternVector,
            MaskPattern.PatternThree_DiagonalLines => VerticalLinesPatternVector, // same pattern [1 0 0] with a different starting offset per row
            MaskPattern.PatternFour_LargeCheckerboard => LargeCheckboardPatternVector,
            MaskPattern.PatternFive_Fields => FieldsPatternVector,
            MaskPattern.PatternSix_Diamonds => DiamondsPatternVector,
            MaskPattern.PatternSeven_Meadow => MeadowPatternVector,
            _ => throw new UnreachableException()
        };

        for (var y = 0; y < matrix.Height; y++)
        {
            var row = ByteMatrixMarshal.GetWritableRow(matrix, y);
            var segments = functionModules.GetMaskableSegments(y);

            foreach (var segmentRange in segments)
            {
                var segment = row[segmentRange];
                var (x, _) = segmentRange.GetOffsetAndLength(matrix.Width);
                ApplyMaskVectorized(segment, patternVector, patternOffsetFunc, x, y, segment);
                ByteMatrixMarshal.UpdateColumnMajorOrder(matrix, y, x, segment);
            }
        }
        return;
    }

    private static Func<int, int, int> GetPatternVectorOffsetFunc(MaskPattern mask)
    {
        return mask switch
        {
            // Trivial pattern - we either want to start at 0 or 1 depending on the x and y values
            MaskPattern.PatternZero_Checkerboard => static (x, y) => (x + y) % 2,
            // We either need all 1s or all 0s, and each run of 1s or 0s in the pattern vector is 64 bytes long
            MaskPattern.PatternOne_HorizontalLines => static (x, y) => (y & 1) == 0 ? 0 : 64,
            // Trivial pattern - we just find the starting point via the x value
            MaskPattern.PatternTwo_VerticalLines => static (x, y) => x % 3,
            MaskPattern.PatternThree_DiagonalLines => static (x, y) => (x + y) % 3,
            // The pattern here repeats every 6 modules in the x direction. Ever 2 rows, the pattern is offset
            // by a full 3x2 rectangle, so for y == 0 or 1, we don't offset, for y == 2 or 3, we offset by 3 to
            // move into a new 3x2 rectangle
            MaskPattern.PatternFour_LargeCheckerboard => static (x, y) => (x + LargeCheckboardYToOffsetLookup[y % 4]) % 6,
            // Each row in the pattern vector corresponds to a row in the pattern, and each row is 72 bytes long
            MaskPattern.PatternFive_Fields => static (x, y) => (x % 6) + FieldsYToOffsetLookup[y % 6],
            MaskPattern.PatternSix_Diamonds => static (x, y) => (x % 6) + DiamondsYToOffsetLookup[y % 6],
            MaskPattern.PatternSeven_Meadow => static (x, y) => (x % 6) + MeadowYToOffsetLookup[y % 6],
            _ => throw new UnreachableException()
        };
    }

    // 64 bytes + 1 extra byte for additional starting offset (starting at 1 instead of 0)
    private static ReadOnlySpan<byte> CheckerboardPatternVector =>
    [
        1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
        1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
        1
    ];

    private static ReadOnlySpan<byte> HorizontalLinesPatternVector =>
    [
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ];

    private static ReadOnlySpan<byte> VerticalLinesPatternVector =>
    [
        1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
        1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
        1, 0, 0,
    ];

    private static ReadOnlySpan<byte> LargeCheckboardPatternVector =>
    [
        1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1,
        1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0,
        0, 0, 1, 1, 1, 0
    ];

    private static ReadOnlySpan<byte> LargeCheckboardYToOffsetLookup => [0, 0, 3, 3];

    // We only need half the pattern here, the last 3 rows are the same as the first 3
    private static ReadOnlySpan<byte> FieldsPatternVector =>
    [
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
        1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
        1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
     // 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
     // 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
     // 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    ];
    private static ReadOnlySpan<byte> FieldsYToOffsetLookup => [0, 72, 72 * 2, 72 * 3, 72 * 2, 72];

    private static ReadOnlySpan<byte> DiamondsPatternVector =>
    [
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0,
        1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0,
        1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
        1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1,
        1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1,
     // 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
     // last row here for readability to complete the full diamond, but its not actually part of the pattern
    ];
    private static ReadOnlySpan<ushort> DiamondsYToOffsetLookup => [0, 72, 72 * 2, 72 * 3, 72 * 4, 72 * 5];

    private static ReadOnlySpan<byte> MeadowPatternVector =>
    [
        1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
        0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1,
        1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1,
        0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1,
        1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0,
        0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0,
    ];
    private static ReadOnlySpan<ushort> MeadowYToOffsetLookup => DiamondsYToOffsetLookup;

    private static void ApplyMaskVectorized(ReadOnlySpan<byte> data, ReadOnlySpan<byte> patternVector, Func<int, int, int> computePatternVectorOffset, int x, int y, Span<byte> destination)
    {
        var remaining = data.Length;
        var currentOffset = x;
        Debug.Assert(patternVector.Length >= Vector<byte>.Count + computePatternVectorOffset(x, y), "pattern vector too small");
        while (remaining >= Vector<byte>.Count)
        {
            currentOffset = computePatternVectorOffset(currentOffset, y);
            var a = Vector.LoadUnsafe(in data[0], 0);
            var b = Vector.LoadUnsafe(in patternVector[0], (nuint)currentOffset);
            Vector.Xor(a, b).CopyTo(destination);
            destination = destination[Vector<byte>.Count..];
            data = data[Vector<byte>.Count..];
            remaining -= Vector<byte>.Count;
            currentOffset = computePatternVectorOffset(currentOffset + Vector<byte>.Count, y);
            Debug.Assert(patternVector.Length >= Vector<byte>.Count + currentOffset, "pattern vector too small");
        }

        for (var i = 0; i < destination.Length; i++)
        {
            destination[i] = (byte)(data[i] ^ patternVector[computePatternVectorOffset(i + currentOffset, y)]);
        }
    }

    private static ReadOnlySpan<byte> XRange(QrVersion version, Span<byte> destination)
    {
        var index = 0;
        for (var i = version.ModulesPerSide - 1; i >= 7; i -= 2)
        {
            destination[index] = (byte)i;
            index++;
        }

        for (var i = 5; i >= 0; i -= 2)
        {
            destination[index] = (byte)i;
            index++;
        }

        return destination[..index];
    }

    private static ReadOnlySpan<byte> YRangeTopDown(Span<byte> destination)
    {
        var yRange = destination;
        var index = (byte)0;
        for (var i = 0; i < yRange.Length; i++)
        {
            yRange[i] = index;
            index++;
        }
        return yRange[..index];
    }

    private static ReadOnlySpan<byte> YRangeBottomUp(Span<byte> destination)
    {
        var yRange = destination;
        var index = (byte)0;
        for (var i = yRange.Length - 1; i >= 0; i--)
        {
            yRange[i] = index;
            index++;
        }
        return yRange[..index];
    }

    public static int CalculatePenalty(ByteMatrix matrix)
    {
        var (rowPenalty, rowPatternPenalty) = CalculateRowPenalty(matrix);
        var (columnPenalty, columnPatternPenalty) = CalculateColumnPenalty(matrix);
        var blocksPenalty = CalculateBlocksPenalty(matrix);
        var ratioPenalty = CalculateRatioPenalty(matrix);

        return rowPenalty + columnPenalty + rowPatternPenalty + columnPatternPenalty + blocksPenalty + ratioPenalty;
    }

    /// <summary>
    /// <c>N - 2</c> penalty for <c>N</c> consecutive dark modules in a line, for <c>N >= 5</c>.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static (int LinePenalty, int PatternPenalty) CalculateRowPenalty(ByteMatrix matrix)
    {
        var linePenaltyTotal = 0;
        var patternPenaltyTotal = 0;
        for (var row = 0; row < matrix.Height; row++)
        {
            var (linePenalty, patternPenalty) = CalculateLineAndPatternPenaltyNonVectorized(matrix.GetRow(row));
            linePenaltyTotal += linePenalty;
            patternPenaltyTotal += patternPenalty;
        }

        return (linePenaltyTotal, patternPenaltyTotal);
    }

    /// <summary>
    /// <c>N - 2</c> penalty for <c>N</c> consecutive dark modules in a column, for <c>N >= 5</c>.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static (int ColumnPenalty, int PatternPenalty) CalculateColumnPenalty(ByteMatrix matrix)
    {
        var linePenaltyTotal = 0;
        var patternPenaltyTotal = 0;
        for (var column = 0; column < matrix.Width; column++)
        {
            var (linePenalty, patternPenalty) = CalculateLineAndPatternPenaltyNonVectorized(matrix.GetColumn(column));
            linePenaltyTotal += linePenalty;
            patternPenaltyTotal += patternPenalty;
        }

        return (linePenaltyTotal, patternPenaltyTotal);
    }

    private static (int LinePenalty, int PatternPenalty) CalculateLineAndPatternPenaltyNonVectorized(ReadOnlySpan<byte> values)
    {
        var patternPenaltyBuffer = new PatternPenaltyBuffer();

        var linePenalty = 0;
        var patternPenalty = 0;

        var currentLineModuleCount = 0;
        var currentModuleState = 0;
        for (var i = 0; i < values.Length; i++)
        {
            var currentModule = values[i];

            if (patternPenaltyBuffer.ContainsPenaltyPattern())
            {
                patternPenalty += 40;
            }

            if (currentModule == currentModuleState)
            {
                patternPenaltyBuffer.Push(currentModule);
                currentLineModuleCount++;
            }
            else
            {
                if (currentLineModuleCount >= 5)
                {
                    linePenalty += currentLineModuleCount - 2;
                }

                currentLineModuleCount = 1;
                currentModuleState = currentModule;
                patternPenaltyBuffer.Push(currentModule);
            }
        }

        if (currentLineModuleCount >= 5)
        {
            linePenalty += currentLineModuleCount - 2;
        }

        if (patternPenaltyBuffer.ContainsPenaltyPattern())
        {
            patternPenalty += 40;
        }

        return (linePenalty, patternPenalty);
    }

    private ref struct PatternPenaltyBuffer()
    {
        private ushort _pattern = ushort.MaxValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(byte value) => _pattern = (ushort)(((_pattern << 1) | value) & 0b0111_1111_1111_1111);

        private const ushort LeadingPattern = 0b00000_00001011101;
        private const ushort TrailingPattern = 0b00000_10111010000;
        private const ushort DupePattern = 0b0_000010111010000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsPenaltyPattern()
        {
            var dupePattern = _pattern;
            var leadingOrTrailing = _pattern & 0b111_1111_1111;
            //avoid double counting pattern if it has 0000 on both sides
            return leadingOrTrailing is LeadingPattern or TrailingPattern && dupePattern is not DupePattern;
        }
    }

    /// <summary>
    /// Calculates the penalty for 2x2 blocks of dark or light modules in a symbol.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static int CalculateBlocksPenalty(ByteMatrix matrix)
    {
        var blocksPenalty = 0;
        for (var y = 0; y < matrix.Height - 1; y++)
        {
            var x = 0;
            var row1 = matrix[x, y];
            var row2 = matrix[x, y + 1];

            byte pattern = 0;
            pattern |= (byte)(row1 << 3);
            pattern |= (byte)(row2 << 2);

            x++;
            for (; x < matrix.Width; x++)
            {
                pattern = (byte)(pattern >> 2);
                pattern |= (byte)((matrix[x, y]) << 3);
                pattern |= (byte)((matrix[x, y + 1]) << 2);

                if (pattern is 0b0000 or 0b1111)
                {
                    blocksPenalty += 3;
                }
            }
        }
        return blocksPenalty;
    }

    private static ReadOnlySpan<byte> RatioPointsLookup =>
    [
         0,  0,  0,  0,  0,  0, 10, 10, 10, 10, 10, 20, 20, 20, 20, 20, // 0 - 15
        30, 30, 30, 30, 30, 40, 40, 40, 40, 40, 50, 50, 50, 50, 50, // 16 - 30
        60, 60, 60, 60, 60, 70, 70, 70, 70, 70, 80, 80, 80, 80, 80, // 31 - 45
        90, 90, 90, 90, 90, // 46 - 50 (inclusive)
    ];
    public static int CalculateRatioPenalty(ByteMatrix matrix)
    {
        var darkModules = 0;
        for (var i = 0; i < matrix.Height; i++)
        {
            var row = matrix.GetRow(i);
            darkModules += row.Count((byte)1);
        }

        var totalModules = matrix.Width * matrix.Height;

        var ratio = int.Abs((darkModules * 100 / totalModules) - 50);
        return RatioPointsLookup[ratio];
    }

    private static ReadOnlySpan<ushort> FormatInformationL =>
    [
        0b111_0111_1100_0100,
        0b111_0010_1111_0011,
        0b111_1101_1010_1010,
        0b111_1000_1001_1101,
        0b110_0110_0010_1111,
        0b110_0011_0001_1000,
        0b110_1100_0100_0001,
        0b110_1001_0111_0110
    ];

    private static ReadOnlySpan<ushort> FormatInformationM =>
    [
        0b101_0100_0001_0010,
        0b101_0001_0010_0101,
        0b101_1110_0111_1100,
        0b101_1011_0100_1011,
        0b100_0101_1111_1001,
        0b100_0000_1100_1110,
        0b100_1111_1001_0111,
        0b100_1010_1010_0000
    ];

    private static ReadOnlySpan<ushort> FormatInformationQ =>
    [
        0b011_0101_0101_1111,
        0b011_0000_0110_1000,
        0b011_1111_0011_0001,
        0b011_1010_0000_0110,
        0b010_0100_1011_0100,
        0b010_0001_1000_0011,
        0b010_1110_1101_1010,
        0b010_1011_1110_1101
    ];

    private static ReadOnlySpan<ushort> FormatInformationH =>
    [
        0b001_0110_1000_1001,
        0b001_0011_1011_1110,
        0b001_1100_1110_0111,
        0b001_1001_1101_0000,
        0b000_0111_0110_0010,
        0b000_0010_0101_0101,
        0b000_1101_0000_1100,
        0b000_1000_0011_1011
    ];

    private static ReadOnlySpan<int> VersionInformation => [
        0, 0, 0, 0, 0, 0, 0, // first 7 versions do not have version information.
        0x07C94, 0x085BC, 0x09A99, 0x0A4D3, 0x0BBF6,
        0x0C762, 0x0D847, 0x0E60D, 0x0F928, 0x10B78,
        0x1145D, 0x12A17, 0x13532, 0x149A6, 0x15683,
        0x168C9, 0x177EC, 0x18EC4, 0x191E1, 0x1AFAB,
        0x1B08E, 0x1CC1A, 0x1D33F, 0x1ED75, 0x1F250,
        0x209D5, 0x216F0, 0x228BA, 0x2379F, 0x24B0B,
        0x2542E, 0x26A64, 0x27541, 0x28C69
    ];

}

public enum PositionDetectionPatternLocation
{
    TopLeft = 1,
    TopRight,
    BottomLeft,
}
