using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace Quarer;

public class QrCode
{
    private const int StackAllocByteThreshold = 512;

    private static readonly SearchValues<char> AlphanumericValues = SearchValues.Create("0123456789AABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:");

    private QrCode()
    {
    }

    public static void Generate(ReadOnlySpan<char> data, QrModeIndicator mode, QrVersion qrVersion)
    {
        if (mode != QrModeIndicator.Numeric)
        {
            throw new NotSupportedException("Non-numeric modes are currently not supported.");
        }
    }

    private static ReadOnlySpan<byte> EncodeNumeric(ReadOnlySpan<char> data)
    {
        Span<byte> numericData = data.Length <= StackAllocByteThreshold ? stackalloc byte[data.Length] : new byte[data.Length];
        OperationStatus status = Utf8.FromUtf16(data, numericData, out _, out var bytesWritten, replaceInvalidSequences: true);
        if (status != OperationStatus.Done)
        {
            throw new InvalidOperationException($"Conversion to UTF-8 failed. Reason: '{status}'");
        }

        //convert from ASCII values (48 - 57), to digit values (0 - 9)
        foreach (ref var b in numericData)
        {
            Debug.Assert(b is >= 48 and <= 57, "Invalid digit in input data.");
            b = (byte)(b - '0');
        }

        var bitWriter = new BitWriter();

        var encoder = new NumericModeEncoder(numericData);
        encoder.WriteData(bitWriter);

        return bitWriter.UnsafeGetRawBuffer();
    }
}

internal ref struct NumericModeEncoder
{
    private int _position;
    private readonly ReadOnlySpan<byte> _data;

    public NumericModeEncoder(ReadOnlySpan<byte> data)
    {
        if (data.Length % 3 != 0)
        {
            throw new InvalidOperationException("Currently remainder is not handled. Ensure numeric data is an extact multiple of 3.");
        }

        _position = 0;
        _data = data;
    }

    public unsafe void WriteData(BitWriter writer)
    {
        for (; _position < _data.Length; _position += 3)
        {
            ReadOnlySpan<byte> digits = _data.Slice(_position, _position + 3);
            var v = (ushort)((digits[0] * 100) + (digits[1] * 10) + digits[2]);

            writer.WriteBits(v, 10);
        }

        //TODO: handle remainder
    }
}
