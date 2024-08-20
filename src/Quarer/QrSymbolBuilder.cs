using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarer;
public static class QrCodeSymbolBuilder
{
    public static BitMatrix BuildSymbol(QrVersion version, BitWriter codewords)
    {

        return null!;
    }
}

public static class QrFunctionPatternsEncoder
{
    //ported from https://github.com/zxing-cpp/zxing-cpp/blob/d979b765a14423f6d7747827b72193a0da9f3030/core/src/qrcode/QRMatrixUtil.cpp

    private readonly ref struct Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    }

    private static int MaxAbsoluteValue(Point p) => Math.Max(Math.Abs(p.X), Math.Abs(p.Y));

    public static void EmbedPositionDetectionPattern(TrackedBitMatrix matrix, int xStart, int yStart)
    {
        for (var y = 0; y < 7; ++y)
        {
            for (var x = 0; x < 7; ++x)
            {
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

}

public static class QrEncodingRegionsEncoder
{
}
