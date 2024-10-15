using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Quarer;
public static partial class QrSymbolBuilder
{
    [SkipLocalsInit]
    public static void EncodeDataBits(ByteMatrix matrix, QrVersion version, BitBuffer data)
    {
        if (version.TotalCodewords != data.ByteCount)
        {
            throw new InvalidOperationException("Data byte count does not match total codewords for QR code version.");
        }

        var functionModules = FunctionModules.GetForVersion(version);

#pragma warning disable IDE0057 // Use range operator //https://github.com/dotnet/roslyn/issues/74960
        Span<byte> yRangeValues = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.Width);
        Span<byte> yRangeValuesReverse = stackalloc byte[QrVersion.MaxModulesPerSide].Slice(0, version.Width);
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

    private static ReadOnlySpan<byte> XRange(QrVersion version, Span<byte> destination)
    {
        var index = 0;
        for (var i = version.Width - 1; i >= 7; i -= 2)
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

}
