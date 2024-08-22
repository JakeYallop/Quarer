using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Quarer.Numerics;

namespace Quarer;

public sealed class DataAnalysisResult
{
    private DataAnalysisResult(QrEncodingInfo encoding) : this(encoding, AnalysisResult.Success)
    {
    }

    private DataAnalysisResult(QrEncodingInfo? encoding, AnalysisResult result)
    {
        Result = encoding;
        AnalysisResult = result;
    }

    public QrEncodingInfo? Result { get; }
    public AnalysisResult AnalysisResult { get; }
    [MemberNotNullWhen(true, nameof(Result))]
    public bool Success => AnalysisResult is AnalysisResult.Success;

    public static DataAnalysisResult Invalid(AnalysisResult result)
        => result == AnalysisResult.Success
            ? throw new ArgumentException("Cannot create an invalid result from a success.", nameof(result))
            : new(null, result);

    public static DataAnalysisResult Successful(QrEncodingInfo encoding)
        => new(encoding);
}

public static class QrDataEncoder
{
    public static readonly SearchValues<char> AlphanumericCharacters = SearchValues.Create(AlphanumericEncoder.Characters);
    public static readonly SearchValues<char> NumericCharacters = SearchValues.Create("0123456789");

    public static DataAnalysisResult AnalyzeSimple(ReadOnlySpan<char> data, ErrorCorrectionLevel requestedErrorCorrectionLevel)
    {
        //For now, just use a single mode for the full set of data.
        var mode = DeriveMode(data);
        var dataLength = mode.GetBitStreamLength(data);
        if (!QrVersionLookup.TryGetVersionForDataCapacity(dataLength, mode, requestedErrorCorrectionLevel, out var version))
        {
            return DataAnalysisResult.Invalid(AnalysisResult.DataTooLarge);
        }

        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var segment = DataSegment.Create(characterCount, mode, dataLength, new Range(0, data.Length));
        var segments = ImmutableArray.Create(segment);
        var encoding = new QrEncodingInfo(version, segments);
        return DataAnalysisResult.Successful(encoding);
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
        var bitWriter = new BitWriter(new(qrDataEncoding.Version.DataCodewordsCapacity * 8));

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

        QrTerminatorBlock.WriteTerminator(bitWriter, version);

        PadBitsInFinalCodeword(bitWriter);

        var codewords = bitWriter.BytesWritten;
        while (codewords <= version.DataCodewordsCapacity - 4)
        {
            bitWriter.WriteBitsBigEndian(PadPattern32Bits, 32);
            codewords += 4;
        }

        var alternate = true;
        while (codewords < version.DataCodewordsCapacity)
        {
            bitWriter.WriteBitsBigEndian(alternate ? PadPattern8_1 : PadPattern8_2, 8);
            alternate = !alternate;
            codewords++;
        }

        return bitWriter.Buffer;

        static void WriteHeader(BitWriter writer, QrVersion version, ModeIndicator mode, ReadOnlySpan<char> dataSlice)
        {
            var header = QrHeaderBlock.Create(version, mode, mode.GetDataCharacterLength(dataSlice));
            header.WriteHeader(writer);
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

    public static unsafe BitWriter EncodeAndInterleaveErrorCorrectionBlocks(QrVersion version, BitBuffer dataCodewordsBitBuffer)
    {
        if (dataCodewordsBitBuffer.ByteCount != version.DataCodewordsCapacity)
        {
            throw new ArgumentException("Size of input data and size of version do not match.", nameof(dataCodewordsBitBuffer));
        }

        var errorCorrectionCodewordsPerBlock = version.ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock;
        var errorCorrectionBlocks = version.ErrorCorrectionBlocks;
        var maxDataCodewordsInBlocks = errorCorrectionBlocks.MaxDataCodewordsInBlock;
        var resultBitBuffer = new BitWriter(new(version.TotalCodewords >> 2));

        // max number of codewords in a data block is 123, so ensure writer size is greater than or equal to 123
        Span<byte> dataCodewordsDestination = stackalloc byte[128];

        var codewordsSeen = 0;
        for (var i = 0; i < maxDataCodewordsInBlocks; i++)
        {
            foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
            {
                if (i < b.DataCodewordsPerBlock)
                {
                    var written = dataCodewordsBitBuffer.GetBytes(codewordsSeen + i, 1, dataCodewordsDestination);
                    Debug.Assert(written == 1);
                    //var dataCodewords = dataCodewordsDestination[..written];
                    resultBitBuffer.WriteBitsBigEndian(dataCodewordsDestination[0], 8);
                }
                codewordsSeen += b.DataCodewordsPerBlock;
            }
            codewordsSeen = 0;
        }

        // max number of blocks for a symbol is 81, max number of codewords per block is 30
        Span<ByteBuffer30> errorBlocks = stackalloc ByteBuffer30[81];
        var generator = BinaryFiniteField.GetGeneratorPolynomial(errorCorrectionCodewordsPerBlock);

        codewordsSeen = 0;
        var blockIndex = 1;
        Span<byte> divisionDestination = stackalloc byte[256];
        foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
        {
            var dataCodewordsWritten = dataCodewordsBitBuffer.GetBytes(codewordsSeen, b.DataCodewordsPerBlock, dataCodewordsDestination);
            var dataCodewords = dataCodewordsDestination[..dataCodewordsWritten];

            var (written, separator) = BinaryFiniteField.Divide(dataCodewords, generator, divisionDestination);
            var divisionResult = divisionDestination[..written];
            var errorCodewords = divisionResult[separator..];
            Span<byte> writer = errorBlocks[blockIndex - 1];
            errorCodewords.CopyTo(writer);
            blockIndex++;
            codewordsSeen = b.DataCodewordsPerBlock;
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

        return resultBitBuffer;
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
