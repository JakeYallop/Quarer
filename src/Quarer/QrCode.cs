using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace Quarer;

#pragma warning disable
public class QrCode
{
    private const int StackAllocByteThreshold = 512;

    private QrCode()
    {
    }

    public static void Generate(ReadOnlySpan<char> data, ModeIndicator mode, QrVersion qrVersion)
    {
        if (mode != ModeIndicator.Numeric)
        {
            throw new NotSupportedException("Non-numeric modes are currently not supported.");
        }

        //TODO: Use array pool here?
        Span<byte> utf8Data = data.Length <= StackAllocByteThreshold ? stackalloc byte[StackAllocByteThreshold].Slice(0, data.Length) : new byte[data.Length];
        var status = Utf8.FromUtf16(data, utf8Data, out _, out var bytesWritten, replaceInvalidSequences: true);
        if (status != OperationStatus.Done)
        {
            throw new InvalidOperationException($"Conversion to UTF-8 failed. Reason: '{status}'");
        }
        EncodeNumeric(qrVersion, mode, utf8Data);
    }

    private static object EncodeNumeric(QrVersion version, ModeIndicator mode, Span<byte> utf8Data)
    {
        //convert from ASCII values (48 - 57), to digit values (0 - 9)
        foreach (ref var b in utf8Data)
        {
            Debug.Assert(b is >= 48 and <= 57, "Invalid digit in input data.");
            b = (byte)(b - '0');
        }

        var writer = new BitWriter();
        var header = QrHeaderBlock.Create(version, mode, utf8Data.Length);
        header.WriteHeader(writer);
        ReadOnlySpan<byte> roData = utf8Data;
        return writer.Buffer.AsBitEnumerable().ToArray();
    }
}
