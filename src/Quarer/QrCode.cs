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
        var numericEncoder = new NumericModeEncoder(bitWriter);
        numericEncoder.Encode(numericData);
        return bitWriter.UnsafeGetRawBuffer();
    }
}

internal readonly ref struct NumericModeEncoder(BitWriter writer)
{
    private readonly BitWriter _writer = writer;

    public void Encode(ReadOnlySpan<byte> data)
    {
        var position = 0;
        for (; position + 3 <= data.Length; position += 3)
        {
            ReadOnlySpan<byte> digits = data[position..(position + 3)];
            var v = GetDigitsAsValue(digits);
            _writer.WriteBits(v, 10);
        }

        if (data.Length != position)
        {
            var remaining = data.Length - position;
            ReadOnlySpan<byte> remainingDigits = data[position..(position + remaining)];
            var numberOfBits = remaining switch
            {
                1 => 4,
                2 => 7,
                _ => throw new InvalidOperationException("Expected only 1 or 2 digits as a remainder after encoding all other 10 bit triples.")
            };

            _writer.WriteBits(GetDigitsAsValue(remainingDigits), numberOfBits);
        }
    }

    private static ushort GetDigitsAsValue(ReadOnlySpan<byte> slicedDigits)
    {
        return slicedDigits.Length switch
        {
            3 => (ushort)((slicedDigits[0] * 100) + (slicedDigits[1] * 10) + slicedDigits[2]),
            2 => (ushort)((slicedDigits[0] * 10) + slicedDigits[1]),
            1 => slicedDigits[0],
            _ => throw new InvalidOperationException($"Expected 1, 2 or 3 digits, found '{slicedDigits.Length}' digits instead.")
        };
    }
}
