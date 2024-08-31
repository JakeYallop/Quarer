using System.Buffers;
using System.Diagnostics;

namespace Quarer;

//TODO: Consider creating a bunch of interfaces so each method is called in the correct order (we would want
// to create an EncodePositionDetectionPatterns method in such case so 3 separate calls are not needed)
public static class QrCodeSymbolBuilder
{
    public static TrackedBitMatrix BuildSymbol(QrVersion version, BitWriter dataCodewords, QrMaskPattern? maskPattern = null)
        => BuildSymbol(new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide), version, dataCodewords, maskPattern);

    private static TrackedBitMatrix BuildSymbol(TrackedBitMatrix matrix, QrVersion version, BitWriter dataCodewords, QrMaskPattern? maskPattern = null)
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
            EncodeFormatInformation(matrix, version.ErrorCorrectionLevel, maskPattern.Value);
            EncodeDataBits(matrix, version, dataCodewords.Buffer, maskPattern.Value);
            return matrix;
        }

        // otherwise, determine the best mask pattern to use

        var patterns = (ReadOnlySpan<QrMaskPattern>)[QrMaskPattern.PatternZero_Checkerboard, QrMaskPattern.PatternOne_HorizontalLines, QrMaskPattern.PatternTwo_VerticalLines, QrMaskPattern.PatternThree_DiagonalLines, QrMaskPattern.PatternFour_LargeCheckerboard, QrMaskPattern.PatternFive_Fields, QrMaskPattern.PatternSix_Diamonds, QrMaskPattern.PatternSeven_Meadow];

        var highestPenalty = int.MaxValue;
        var resultMatrix = matrix;
        foreach (var pattern in patterns)
        {
            var copiedMatrix = matrix.Clone();
            // important to encode format information before data bits so that non-empty modules are correctly set
            EncodeFormatInformation(copiedMatrix, version.ErrorCorrectionLevel, pattern);
            EncodeDataBits(copiedMatrix, version, dataCodewords.Buffer, pattern);

            var penalty = EvaluateSymbol(matrix);
            if (penalty < highestPenalty)
            {
                highestPenalty = penalty;
                resultMatrix = copiedMatrix;
            }
        }

        return resultMatrix;
    }

    private readonly ref struct Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    }

    private static int MaxAbsoluteValue(Point p) => Math.Max(Math.Abs(p.X), Math.Abs(p.Y));

    public static void EncodePositionDetectionPattern(TrackedBitMatrix matrix, PositionDetectionPatternLocation location)
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
                matrix[xStart + x, yStart + y] = MaxAbsoluteValue(new Point(x, y) - new Point(3, 3)) != 2;
            }
        }

        EncodeSeparatorPattern(matrix, xStart, yStart);
    }

    private static void EncodeSeparatorPattern(TrackedBitMatrix matrix, int xStart, int yStart)
    {
        static void SetIfInside(TrackedBitMatrix matrix, int x, int y)
        {
            if (x >= 0 && x < matrix.Width && y >= 0 && y < matrix.Height)
            {
                matrix[x, y] = false;
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

    public static void EncodeStaticDarkModule(TrackedBitMatrix matrix) => matrix[matrix.Width - 8, 8] = true;

    public static void EncodePositionAdjustmentPatterns(TrackedBitMatrix matrix, QrVersion version)
    {
        // version 1 QrCode does not have any alignment patterns
        if (version.Version <= 1)
        {
            return;
        }

        var positions = version.AlignmentPatternCenters;

        for (var y = 0; y < positions.Length; y++)
        {
            for (var x = 0; x < positions.Length; x++)
            {
                if (DoesNotOverlapFinderPatterns(matrix, x, y))
                {
                    EncodePositionAdjustmentPattern(matrix, positions[x], positions[y]);
                }
            }
        }
    }

    private static bool DoesNotOverlapFinderPatterns(TrackedBitMatrix matrix, int x, int y)
        // we expect nothing else other than the finder patterns to be written to the matrix at this point
        // and the finder patterns overlap the center coordinates of the alignment patterns
        => matrix.IsEmpty(x, y);

    private static void EncodePositionAdjustmentPattern(TrackedBitMatrix matrix, int centreX, int centreY)
    {
        Debug.Assert(matrix.Width >= 21 && matrix.Height >= 21);
        Debug.Assert(centreX >= 2 && centreY >= 2);
        for (var dy = -2; dy <= 2; dy++)
        {
            for (var dx = -2; dx <= 2; dx++)
            {
                // Explanaitio
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
                matrix[centreX + dx, centreY + dy] = MaxAbsoluteValue(new Point(dx, dy) - new Point(2, 2)) != 1;
            }
        }
    }

    public static void EncodeTimingPatterns(TrackedBitMatrix matrix)
    {
        // skip position detection and separator patterns
        // no special handling of alignment patterns is necessary as the alignments patterns line up
        // with respect to the light and dark module alternation of the timing patterns
        for (var i = 8; i < matrix.Width - 8; i++)
        {
            var bit = i % 2 == 0;
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

    public static void EncodeFormatInformation(TrackedBitMatrix matrix, ErrorCorrectionLevel errorCorrectionLevel, QrMaskPattern maskPattern)
    {
        var formatInformation = GetFormatInformation(errorCorrectionLevel, (byte)maskPattern);
        var size = matrix.Width;
        Debug.Assert(matrix.Width == matrix.Height);

        matrix[8, 0] = (formatInformation & 0b000_0000_0000_0001) != 0;
        matrix[8, 1] = (formatInformation & 0b000_0000_0000_0010) != 0;
        matrix[8, 2] = (formatInformation & 0b000_0000_0000_0100) != 0;
        matrix[8, 3] = (formatInformation & 0b000_0000_0000_1000) != 0;
        matrix[8, 4] = (formatInformation & 0b000_0000_0001_0000) != 0;
        matrix[8, 5] = (formatInformation & 0b000_0000_0010_0000) != 0;
        // skipped - timing pattern
        matrix[8, 7] = (formatInformation & 0b000_0000_0100_0000) != 0;
        matrix[8, 8] = (formatInformation & 0b000_0000_1000_0000) != 0;

        matrix[8, size - 7] = (formatInformation & 0b000_0001_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b000_0010_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b000_0100_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b000_1000_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b001_0000_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b010_0000_0000_0000) != 0;
        matrix[8, size - 7] = (formatInformation & 0b100_0000_0000_0000) != 0;

        matrix[size - 1, 8] = (formatInformation & 0b000_0000_0000_0001) != 0;
        matrix[size - 2, 8] = (formatInformation & 0b000_0000_0000_0010) != 0;
        matrix[size - 3, 8] = (formatInformation & 0b000_0000_0000_0100) != 0;
        matrix[size - 4, 8] = (formatInformation & 0b000_0000_0000_1000) != 0;
        matrix[size - 5, 8] = (formatInformation & 0b000_0000_0001_0000) != 0;
        matrix[size - 6, 8] = (formatInformation & 0b000_0000_0010_0000) != 0;
        matrix[size - 7, 8] = (formatInformation & 0b000_0000_0100_0000) != 0;
        matrix[size - 8, 8] = (formatInformation & 0b000_0000_1000_0000) != 0;

        matrix[7, 8] = (formatInformation & 0b000_0001_0000_0000) != 0;
        // skipped - timing pattern
        matrix[5, 8] = (formatInformation & 0b000_0010_0000_0000) != 0;
        matrix[4, 8] = (formatInformation & 0b000_0100_0000_0000) != 0;
        matrix[3, 8] = (formatInformation & 0b000_1000_0000_0000) != 0;
        matrix[2, 8] = (formatInformation & 0b001_0000_0000_0000) != 0;
        matrix[1, 8] = (formatInformation & 0b010_0000_0000_0000) != 0;
        matrix[0, 8] = (formatInformation & 0b100_0000_0000_0000) != 0;

        //TODO: Write version of this that uses loops and compare perf vs code size
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

    public static void EncodeVersionInformation(TrackedBitMatrix matrix, QrVersion version)
    {
        if (version.Version is <= 7)
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
                matrix[j, size - 11 + i] = v != 0;
                matrix[size - 11 + i, j] = v != 0;
            }
        }
    }

    public static void EncodeDataBits(TrackedBitMatrix matrix, QrVersion version, BitBuffer data, QrMaskPattern maskPattern)
    {
        if (version.TotalCodewords != data.ByteCount)
        {
            throw new InvalidOperationException("Data byte count does not match total codewords for QR code version.");
        }

#pragma warning disable IDE0057 // Use range operator //https://github.com/dotnet/roslyn/issues/74960
        Span<byte> yRangeValues = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.ModulesPerSide);
        Span<byte> yRangeValuesReverse = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.ModulesPerSide);
#pragma warning restore IDE0057 // Use range operator

        Span<byte> xRangeValues = stackalloc byte[QrVersion.MaxModulesPerSide / 2];
        var xValuesWritten = XRange(version, xRangeValues);
        var xRange = (ReadOnlySpan<byte>)xRangeValues[..xValuesWritten];
        var yRangeTopDown = YRangeTopDown(yRangeValues);
        var yRangeBottomUp = YRangeBottomUp(yRangeValuesReverse);

        var reverse = false;
        var bitIndex = 0;
        foreach (var x in xRange)
        {
            var yRange = reverse ? yRangeTopDown : yRangeBottomUp;

            foreach (var y in yRange)
            {
                if (matrix.IsEmpty(x, y))
                {
                    var bit = data[(bitIndex / 8) + (7 - (bitIndex % 8))];
                    matrix[x, y] = GetMaskedBit(bit, maskPattern, x, y);
                }
                bitIndex++;

                if (matrix.IsEmpty(x - 1, y))
                {
                    var bit = data[(bitIndex / 8) + (7 - (bitIndex % 8))];
                    matrix[x, y - 1] = GetMaskedBit(bit, maskPattern, x, y);
                }
                bitIndex++;
            }
            reverse = !reverse;
        }

        Debug.Assert(bitIndex == version.TotalCodewords * 8);
    }

    public static bool GetMaskedBit(bool bit, QrMaskPattern mask, int x, int y)
    {
        var maskBit = mask switch
        {
            QrMaskPattern.PatternZero_Checkerboard => (x + y) % 2 == 0,
            QrMaskPattern.PatternOne_HorizontalLines => y % 2 == 0,
            QrMaskPattern.PatternTwo_VerticalLines => x % 3 == 0,
            QrMaskPattern.PatternThree_DiagonalLines => (x + y) % 3 == 0,
            QrMaskPattern.PatternFour_LargeCheckerboard => ((y / 2) + (x / 3)) % 2 == 0,
            QrMaskPattern.PatternFive_Fields => (y * x % 2) + (y * x % 3) == 0,
            QrMaskPattern.PatternSix_Diamonds => ((x * y % 2) + (x * y % 3)) % 2 == 0,
            QrMaskPattern.PatternSeven_Meadow => (((x + y) % 2) + (x * y % 3)) % 2 == 0,
            _ => throw new ArgumentOutOfRangeException(nameof(mask))
        };
        return maskBit != bit;
    }

    private static int XRange(QrVersion version, Span<byte> destination)
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

        return index + 1;
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
        return yRange;
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
        return yRange;
    }

    public static int EvaluateSymbol(TrackedBitMatrix matrix)
    {
        var (rowPenalty, rowPatternPenalty) = CalculateRowPenalty(matrix);
        var (columnPenalty, columnPatternPenalty) = CalculateColumnPenalty(matrix);

        //TODO: 2x2 blocks of dark modules, proportion of dark modules

        return rowPenalty + columnPenalty + rowPatternPenalty + columnPatternPenalty;
    }

    /// <summary>
    /// <c>N - 2</c> penalty for <c>N</c> consecutive dark modules in a line, for <c>N >= 5</c>.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static (int LinePenalty, int PatternPenalty) CalculateRowPenalty(TrackedBitMatrix matrix)
    {
        var linePenaltyTotal = 0;
        var patternPenaltyTotal = 0;
        for (var i = 0; i < matrix.Height; i++)
        {
            var (linePenalty, patternPenalty) = CalculateLineAndPatternPenalty(matrix.GetRow(i));
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
    public static (int ColumnPenalty, int PatternPenalty) CalculateColumnPenalty(TrackedBitMatrix matrix)
    {
        var linePenaltyTotal = 0;
        var patternPenaltyTotal = 0;
        for (var i = 0; i < matrix.Height; i++)
        {
            var (linePenalty, patternPenalty) = CalculateLineAndPatternPenalty(matrix.GetColumn(i));
            linePenaltyTotal += linePenalty;
            patternPenaltyTotal += patternPenalty;
        }

        return (linePenaltyTotal, patternPenaltyTotal);
    }

    private static ReadOnlySpan<byte> FinderPatternTrailing => [0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1];
    private static ReadOnlySpan<byte> FinderPatternLeading => [1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0];
    private static ReadOnlySpan<byte> FinderPatternAll => [0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0];
    private static readonly SearchValues<byte> LinePattern = SearchValues.Create([1, 1, 1, 1, 1]);

    //TODO: Evaluate the constant overhead of this vectorization - it could outweigh the benefits of the vectorization
    private static (int LinePenalty, int PatternPenalty) CalculateLineAndPatternPenalty(BitBuffer line)
    {
        var values = GetValues(line, stackalloc byte[QrVersion.MaxModulesPerSide]);
        AssertConvertedBytes(values);

        var currentIndex = 0;
        var currentLineModuleCount = 0;
        var linePenalty = 0;

        var patternCount = values.Count(FinderPatternLeading);
        patternCount += values.Count(FinderPatternTrailing);
        patternCount -= values.Count(FinderPatternAll);

        currentIndex = values.IndexOfAny(LinePattern);
        while (currentIndex != -1 && currentIndex < values.Length)
        {
            currentLineModuleCount = 5;
            currentIndex += currentLineModuleCount + 1;
            while (currentIndex < values.Length && values[currentIndex] == 1)
            {
                currentLineModuleCount++;
                currentLineModuleCount += 1;
                currentIndex += currentLineModuleCount + 1;
            }

            linePenalty += currentLineModuleCount - 2;
            if (currentIndex < values.Length)
            {
                currentIndex = values[(currentIndex + 1)..].IndexOfAny(LinePattern);
            }
        }

        return (linePenalty, patternCount * 40);

        static ReadOnlySpan<byte> GetValues(BitBuffer line, Span<byte> destination)
        {
            var index = 0;
            foreach (var b in line.AsBitEnumerable())
            {
                // we cannot just bit cast here as the internal representation of a bool is not guaranteed
                destination[index] = b ? (byte)1 : (byte)0;
                index++;
            }
            return destination[..(index + 1)];
        }

        [Conditional("DEBUG")]
        static void AssertConvertedBytes(ReadOnlySpan<byte> bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                Debug.Assert(bytes[i] < 2, $"Found value greater than 1 at index {i}. Expected all values to be either 1 or 0. [{string.Join(", ", bytes.Slice(Math.Min(0, i), Math.Max(bytes.Length, i + 5)).ToArray())}");
            }
        }
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
