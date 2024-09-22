using System.Text;

namespace Quarer.Tests;
internal static class MatrixTestUtilities
{
    public static string MatrixToString(BitMatrix m)
    {
        var sb = new StringBuilder(m.Width * m.Height);
        for (var y = 0; y < m.Height; y++)
        {
            for (var x = 0; x < m.Width; x++)
            {
                sb.Append(m[x, y] ? "X" : '-');
                if (x + 1 < m.Width)
                {
                    sb.Append(' ');
                }
            }

            if (y + 1 < m.Height)
            {
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    public static BitBuffer AlternatingCodewordsBuffer(int n)
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

    public static BitBuffer AllZeroBuffer(int n)
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
