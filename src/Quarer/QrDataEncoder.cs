using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Quarer.Numerics;

namespace Quarer;

public static class QrDataEncoder
{
    public static readonly SearchValues<char> AlphanumericCharacters = SearchValues.Create(AlphanumericEncoder.Characters);
    public static readonly SearchValues<char> NumericCharacters = SearchValues.Create("0123456789");

    public static DataAnalysisResult AnalyzeSimple(ReadOnlySpan<char> data, ErrorCorrectionLevel requestedErrorCorrectionLevel)
    {
        //For now, just use a single mode for the full set of data.
        var mode = DeriveMode(data);
        var dataLength = mode.GetDataCharacterLength(data);
        if (!QrVersion.TryGetVersionForDataCapacity(dataLength, mode, requestedErrorCorrectionLevel, out var version))
        {
            return DataAnalysisResult.Invalid(AnalysisResult.DataTooLarge);
        }

        var bitstreamLength = mode.GetBitStreamLength(data);
        var encoding = CreateDataSegment(data, bitstreamLength, version, requestedErrorCorrectionLevel, mode);
        return DataAnalysisResult.Successful(encoding);
    }

    internal static QrEncodingInfo CreateSimpleDataEncoding(ReadOnlySpan<char> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode)
    {
        Debug.Assert(QrVersion.VersionCanFitData(version, data, errorCorrectionLevel, mode), "Expected version to be large enough to contain data.");
        var dataLength = mode.GetBitStreamLength(data);
        return CreateDataSegment(data, dataLength, version, errorCorrectionLevel, mode);
    }

    private static QrEncodingInfo CreateDataSegment(ReadOnlySpan<char> data, int bitstreamLength, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode)
    {
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var segment = DataSegment.Create(characterCount, mode, bitstreamLength, new Range(0, data.Length));
        var segments = ImmutableArray.Create(segment);
        var encoding = new QrEncodingInfo(version, errorCorrectionLevel, segments);
        return encoding;
    }

    public static ModeIndicator DeriveMode(ReadOnlySpan<char> data)
    {
        return data.ContainsAnyExcept(AlphanumericCharacters)
            ? KanjiEncoder.ContainsAnyExceptKanji(data) ? ModeIndicator.Byte : ModeIndicator.Kanji
            : data.ContainsAnyExcept(NumericCharacters) ? ModeIndicator.Alphanumeric : ModeIndicator.Numeric;
    }

    /// <summary>
    /// Encodes the data codeword stream, including mode indicators, character counts, and a terminator symbol if necessary.
    /// </summary>
    /// <param name="qrDataEncoding"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static BitBuffer EncodeDataBits(QrEncodingInfo qrDataEncoding, ReadOnlySpan<char> data)
    {
        var version = qrDataEncoding.Version;
        var dataCodewordsCapacity = qrDataEncoding.Version.GetDataCodewordsCapacity(qrDataEncoding.ErrorCorrectionLevel);
        var bitWriter = new BitWriter(new(dataCodewordsCapacity * 8));

        foreach (var segment in qrDataEncoding.DataSegments)
        {
            var dataSlice = data[segment.Range];
#pragma warning disable IDE0010 // Add missing cases
            switch (segment.Mode)
            {
                case ModeIndicator.Numeric:
                    WriteHeader(bitWriter, version, ModeIndicator.Numeric, dataSlice);
                    NumericEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                case ModeIndicator.Alphanumeric:

                    WriteHeader(bitWriter, version, ModeIndicator.Alphanumeric, dataSlice);
                    AlphanumericEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                case ModeIndicator.Byte:
                    //TODO: Add byte encoder to allow making this more efficient (e.g vectorization) and allow testability
                    //e.g read 4 bytes at a time and write 32 bits
                    WriteHeader(bitWriter, version, ModeIndicator.Byte, dataSlice);
                    foreach (var c in data[segment.Range])
                    {
                        bitWriter.WriteBitsBigEndian(c, 8);
                    }
                    break;
                case ModeIndicator.Kanji:
                    WriteHeader(bitWriter, version, ModeIndicator.Byte, dataSlice);
                    KanjiEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                default:
                    throw new UnreachableException($"Mode '{segment.Mode}' not expected.");
            }
#pragma warning restore IDE0010 // Add missing cases
        }

        QrTerminatorBlock.WriteTerminator(bitWriter, version, qrDataEncoding.ErrorCorrectionLevel);

        PadBitsInFinalCodeword(bitWriter);

        var codewords = bitWriter.BytesWritten;
        while (codewords <= dataCodewordsCapacity - 4)
        {
            bitWriter.WriteBitsBigEndian(PadPattern32Bits, 32);
            codewords += 4;
        }

        var alternate = true;
        while (codewords < dataCodewordsCapacity)
        {
            bitWriter.WriteBitsBigEndian(alternate ? PadPattern8_1 : PadPattern8_2, 8);
            alternate = !alternate;
            codewords++;
        }

        return bitWriter.Buffer;

        static void WriteHeader(BitWriter writer, QrVersion version, ModeIndicator mode, ReadOnlySpan<char> dataSlice)
        {
            QrHeaderBlock.WriteHeader(writer, version, mode, mode.GetDataCharacterLength(dataSlice));
        }

        static void PadBitsInFinalCodeword(BitWriter writer)
        {
            var remainingBitsInFinalCodewordAfterTerminator = writer.BitsWritten & 7;
            if (remainingBitsInFinalCodewordAfterTerminator != 0)
            {
                writer.WriteBitsBigEndian(0, 8 - remainingBitsInFinalCodewordAfterTerminator);
            }
        }
    }

    public const byte PadPattern8_1 = 0b1110_1100;
    public const byte PadPattern8_2 = 0b0001_0001;
    public const uint PadPattern32Bits = unchecked((uint)((PadPattern8_1 << 24) | (PadPattern8_2 << 16) | (PadPattern8_1 << 8) | PadPattern8_2));

    public static unsafe BitBuffer EncodeAndInterleaveErrorCorrectionBlocks(BitBuffer dataCodewordsBitBuffer, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        if (dataCodewordsBitBuffer.ByteCount != version.GetDataCodewordsCapacity(errorCorrectionLevel))
        {
            throw new ArgumentException("Size of input data and size of version do not match.", nameof(dataCodewordsBitBuffer));
        }

        var errorCorrectionBlocks = version.GetErrorCorrectionBlocks(errorCorrectionLevel);
        var errorCorrectionCodewordsPerBlock = errorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock;
        var maxDataCodewordsInBlocks = errorCorrectionBlocks.MaxDataCodewordsInBlock;
        var resultBitBuffer = new BitWriter(new(version.TotalCodewords * 8));

        var codewordsSeen = 0;
        for (var i = 0; i < maxDataCodewordsInBlocks; i++)
        {
            foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
            {
                if (i < b.DataCodewordsPerBlock)
                {
                    var dataCodewords = BitBufferMarshal.GetBytes(dataCodewordsBitBuffer, codewordsSeen + i, 1);
                    resultBitBuffer.WriteBitsBigEndian(dataCodewords[0], 8);
                }
                codewordsSeen += b.DataCodewordsPerBlock;
            }
            codewordsSeen = 0;
        }

        //TODO: Evaluate SkipLocalsInit here or for entire project for performance
        // max number of blocks for a symbol is 81, max number of codewords per block is 30
        // ~4kb of stack space
        Span<ByteBuffer30> errorBlocks = stackalloc ByteBuffer30[81];
        var generator = BinaryFiniteField.GetGeneratorPolynomial(errorCorrectionCodewordsPerBlock);

        codewordsSeen = 0;
        var blockIndex = 1;
        Span<byte> divisionDestination = stackalloc byte[256];
        foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
        {
            var dataCodewords = BitBufferMarshal.GetBytes(dataCodewordsBitBuffer, codewordsSeen, b.DataCodewordsPerBlock);
            var (written, separator) = BinaryFiniteField.Divide(dataCodewords, generator, divisionDestination);
            var divisionResult = divisionDestination[..written];
            var errorCodewords = divisionResult[separator..];
            Span<byte> writer = errorBlocks[blockIndex - 1];
            errorCodewords.CopyTo(writer);
            blockIndex++;
            codewordsSeen += b.DataCodewordsPerBlock;
            divisionResult.Clear();
        }

        for (var i = 0; i < errorCorrectionCodewordsPerBlock; i++)
        {
            var errorBlockIndex = 0;
            foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
            {
                ReadOnlySpan<byte> bytes = errorBlocks[errorBlockIndex];
                resultBitBuffer.WriteBitsBigEndian(bytes[i], 8);
                errorBlockIndex++;
            }
            errorBlockIndex = 0;
        }

        return resultBitBuffer.Buffer;
    }

    [InlineArray(30)]
    private struct ByteBuffer30
    {
        public byte _0;
    }
}

public enum AnalysisResult
{
    Success = 1,
    DataTooLarge
}

public sealed class DataAnalysisResult
{
    private DataAnalysisResult(QrEncodingInfo encoding) : this(encoding, AnalysisResult.Success)
    {
    }

    private DataAnalysisResult(QrEncodingInfo? encoding, AnalysisResult result)
    {
        Value = encoding;
        Reason = result;
    }
    public QrEncodingInfo? Value { get; }
    public AnalysisResult Reason { get; }
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Success => Reason is AnalysisResult.Success;

    public static DataAnalysisResult Invalid(AnalysisResult result)
        => result == AnalysisResult.Success
            ? throw new ArgumentException("Cannot create an invalid result from a success.", nameof(result))
            : new(null, result);

    public static DataAnalysisResult Successful(QrEncodingInfo encoding)
        => new(encoding);
}

