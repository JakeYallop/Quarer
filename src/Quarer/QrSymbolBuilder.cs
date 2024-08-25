using System.Diagnostics;

namespace Quarer;

//TODO: Consider creating a bunch of interfaces so each method is called in the correct order (we would want
// to create an EncodePositionDetectionPatterns method in such case so 3 separate calls are not needed)
public static class QrCodeSymbolBuilder
{
    public static TrackedBitMatrix BuildSymbol(QrVersion version, BitWriter dataCodewords)
        => BuildSymbol(version, dataCodewords, new TrackedBitMatrix(version.ModulesPerSide, version.ModulesPerSide));

    public static TrackedBitMatrix BuildSymbol(QrVersion version, BitWriter dataCodewords, TrackedBitMatrix matrix)
    {
        QrFunctionPatternsEncoder.EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.TopLeft);
        QrFunctionPatternsEncoder.EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.TopRight);
        QrFunctionPatternsEncoder.EncodePositionDetectionPattern(matrix, PositionDetectionPatternLocation.BottomLeft);

        QrFunctionPatternsEncoder.EncodeStaticDarkModule(matrix);

        QrFunctionPatternsEncoder.EncodePositionAdjustmentPatterns(matrix, version);

        QrFunctionPatternsEncoder.EncodeTimingPatterns(matrix);

        //TODO: type info, version info, data
        return matrix;
    }
}

public enum PositionDetectionPatternLocation
{
    TopLeft = 1,
    TopRight,
    BottomLeft,
}

public static class QrFunctionPatternsEncoder
{
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
}

public static class QrEncodingRegionsEncoder
{
}
