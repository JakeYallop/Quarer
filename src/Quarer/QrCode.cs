using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace Quarer;

#pragma warning disable
public class QrCode
{
    private const int StackAllocByteThreshold = 512;

    private static readonly SearchValues<char> AlphanumericValues = SearchValues.Create("0123456789AABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:");

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
        //TODO: If we have multi-byte characters in the input, this will not be enough space. However, we should never actually get multibyte characters calling into this method whilst we do not support Kanji mode.
        Span<byte> utf8Data = data.Length <= StackAllocByteThreshold ? stackalloc byte[data.Length] : new byte[data.Length];
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

        var bitWriter = new BitWriter();
        var header = QrHeader.Create(in version, in mode, utf8Data.Length);
        var numericEncoder = new NumericModeEncoder(bitWriter);
        header.WriteHeader(bitWriter);
        numericEncoder.Encode(utf8Data);
        return bitWriter.GetBitStream().ToArray();
    }
}
