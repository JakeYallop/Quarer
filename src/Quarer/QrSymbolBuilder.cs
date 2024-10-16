using System.Diagnostics;

namespace Quarer;

/// <summary>
/// Methods for creating a QR Code symbol, including masking, penaltly calculationsm and data and function module placement.
/// </summary>
public static partial class QrSymbolBuilder
{
    /// <summary>
    /// Create a QR Code symbol from the given data codewords, version, error correction level, and mask pattern.
    /// If a mask pattern is not provided, the mask pattern than minimises the penalty score for a complete symbol will
    /// be calculated and used.
    /// </summary>
    public static (ByteMatrix Matrix, MaskPattern SelectedMaskPattern) BuildSymbol(BitBuffer dataCodewords, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, MaskPattern? maskPattern = null)
        => BuildSymbol(new ByteMatrix(version.Width, version.Height), dataCodewords, version, errorCorrectionLevel, maskPattern);

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
            EncodeDataCodewords(matrix, version, dataCodewords);
            ApplyMask(matrix, version, maskPattern.Value);
            return (matrix, maskPattern.Value);
        }

        // otherwise, determine the best mask pattern to use

        var patterns = (ReadOnlySpan<MaskPattern>)[MaskPattern.PatternZero_Checkerboard, MaskPattern.PatternOne_HorizontalLines, MaskPattern.PatternTwo_VerticalLines, MaskPattern.PatternThree_DiagonalLines, MaskPattern.PatternFour_LargeCheckerboard, MaskPattern.PatternFive_Fields, MaskPattern.PatternSix_Diamonds, MaskPattern.PatternSeven_Meadow];

        var lowestPenalty = int.MaxValue;
        var selectedMaskPattern = patterns[0];

        EncodeDataCodewords(matrix, version, dataCodewords);

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

    /// <summary>
    /// Embed position detection patterns onto the symbol, at the given location.
    /// </summary>
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

    /// <summary>
    /// Encode the static dark module into the symbol.
    /// </summary>
    public static void EncodeStaticDarkModule(ByteMatrix matrix) => matrix[8, matrix.Height - 8] = 1;

    /// <summary>
    /// Encode position adjustment patterns into the symbol.
    /// </summary>
    public static void EncodePositionAdjustmentPatterns(ByteMatrix matrix, QrVersion version)
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

    /// <summary>
    /// Encode format information into the symbol.
    /// </summary>
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

    /// <summary>
    /// Encode version information into the symbol. Version 6 symbols and below do not include version information.
    /// </summary>
    public static void EncodeVersionInformation(ByteMatrix matrix, QrVersion version)
    {
        if (version.Version is < 7)
        {
            return;
        }

        var versionInformation = VersionInformation[version.Version];
        var size = version.Width;

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
