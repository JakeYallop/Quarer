using System.Runtime.CompilerServices;

namespace Quarer;
public static partial class QrSymbolBuilder
{
    /// <summary>
    /// Calculates the penaly score for a QR code symbol. Lower is better.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Calculates the penalty for the ratio of dark modules vs light modules in a symbol. 50/50 produces no penalty.
    /// </summary>
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
}
