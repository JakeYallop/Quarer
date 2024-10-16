using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Quarer.Numerics;

namespace Quarer;

/// <summary>
/// A class that provides methods for encoding data that can be placed into a QR Code.
/// </summary>
public static class QrDataEncoder
{
    internal static readonly SearchValues<byte> AlphanumericCharacters = SearchValues.Create(AlphanumericEncoder.Characters);
    internal static readonly SearchValues<byte> NumericCharacters = SearchValues.Create(NumericEncoder.Characters);

    /// <summary>
    /// Performs a simple analysis on the provided data to determine the best encoding and smallest possible version
    /// that can fit the provided data using the given error correction level and optional ECI code.
    /// <para>
    /// If the data cannot fit within the QR code, an <see cref="AnalysisResult.DataTooLarge"/> reason will be returned.
    /// </para>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="requestedErrorCorrectionLevel"></param>
    /// <param name="eciCode"></param>
    /// <returns></returns>
    public static DataAnalysisResult AnalyzeSimple(ReadOnlySpan<byte> data, ErrorCorrectionLevel requestedErrorCorrectionLevel, EciCode eciCode)
    {
        // For now, just use a single mode for the full set of data.
        var mode = DeriveMode(data);
        var dataLength = mode.GetDataCharacterLength(data);
        if (!QrVersion.TryGetVersionForDataCapacity(dataLength, requestedErrorCorrectionLevel, mode, eciCode, out var version))
        {
            return DataAnalysisResult.Invalid(AnalysisResult.DataTooLarge);
        }

        var bitstreamLength = mode.GetBitStreamLength(data);
        var encoding = CreateDataSegment(data, bitstreamLength, version, requestedErrorCorrectionLevel, mode, eciCode);
        return DataAnalysisResult.Successful(encoding);
    }

    internal static QrEncodingInfo CreateSimpleDataEncoding(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode, EciCode eciCode)
    {
        Debug.Assert(QrVersion.VersionCanFitData(version, data, errorCorrectionLevel, mode, eciCode), "Expected version to be large enough to contain data.");
        var dataLength = mode.GetBitStreamLength(data);
        return CreateDataSegment(data, dataLength, version, errorCorrectionLevel, mode, eciCode);
    }

    private static QrEncodingInfo CreateDataSegment(ReadOnlySpan<byte> data, int bitstreamLength, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode, EciCode eciCode)
    {
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var segment = DataSegment.Create(characterCount, mode, bitstreamLength, new Range(0, data.Length), eciCode);
        var segments = ImmutableArray.Create(segment);
        var encoding = new QrEncodingInfo(version, errorCorrectionLevel, segments);
        return encoding;
    }

    /// <summary>
    /// Determines the best encoding to use for the provided data. Does not compute the optimal encoding sequence
    /// for a given set of data.
    /// </summary>
    public static ModeIndicator DeriveMode(ReadOnlySpan<byte> data)
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
    public static BitBuffer EncodeDataBits(QrEncodingInfo qrDataEncoding, ReadOnlySpan<byte> data)
    {
        var version = qrDataEncoding.Version;
        var dataCodewordsCapacity = qrDataEncoding.Version.GetDataCodewordsCapacity(qrDataEncoding.ErrorCorrectionLevel);
        var bitWriter = new BitWriter(new(dataCodewordsCapacity * 8));

        foreach (var segment in qrDataEncoding.DataSegments)
        {
            var eciCode = segment.EciCode;
            var dataSlice = data[segment.Range];
            WriteHeader(bitWriter, version, segment.Mode, dataSlice, eciCode);
#pragma warning disable IDE0010 // Add missing cases
            switch (segment.Mode)
            {
                case ModeIndicator.Numeric:
                    NumericEncoder.Encode(bitWriter, dataSlice);
                    break;
                case ModeIndicator.Alphanumeric:

                    AlphanumericEncoder.Encode(bitWriter, dataSlice);
                    break;
                case ModeIndicator.Byte:
                    //TODO: Add byte encoder to allow making this more efficient (e.g vectorization) and allow testability
                    //e.g read 4 bytes at a time and write 32 bits
                    foreach (var c in dataSlice)
                    {
                        bitWriter.WriteBitsBigEndian(c, 8);
                    }
                    break;
                case ModeIndicator.Kanji:
                    KanjiEncoder.Encode(bitWriter, dataSlice);
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

        static void WriteHeader(BitWriter writer, QrVersion version, ModeIndicator mode, ReadOnlySpan<byte> dataSlice, EciCode eciCode)
            => QrHeaderBlock.WriteHeader(writer, version, mode, mode.GetDataCharacterLength(dataSlice), eciCode);

        static void PadBitsInFinalCodeword(BitWriter writer)
        {
            var remainingBitsInFinalCodewordAfterTerminator = writer.BitsWritten & 7;
            if (remainingBitsInFinalCodewordAfterTerminator != 0)
            {
                writer.WriteBitsBigEndian(0, 8 - remainingBitsInFinalCodewordAfterTerminator);
            }
        }
    }

    /// <summary>
    /// The first pattern used to pad the final codewords in a QR code.
    /// </summary>
    public const byte PadPattern8_1 = 0b1110_1100;
    /// <summary>
    /// The second pattern used to pad the final codewords in a QR code.
    /// </summary>
    public const byte PadPattern8_2 = 0b0001_0001;
    /// <summary>
    /// 2 pairs of padding patterns encoded in big-endian format in a 32-bit integer.
    /// </summary>
    public const uint PadPattern32Bits = unchecked((uint)((PadPattern8_1 << 24) | (PadPattern8_2 << 16) | (PadPattern8_1 << 8) | PadPattern8_2));

    /// <summary>
    /// Computes the error correction codewords for the provided data codewords, and interleaves them into a new bit buffer.
    /// </summary>
    [SkipLocalsInit]
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

        // max number of blocks for a symbol is 81, max number of codewords per block is 30
        // ~4kb of stack space
        Span<ByteBuffer30> errorBlocks = stackalloc ByteBuffer30[81];
        var generator = BinaryFiniteField.GetGeneratorPolynomial(errorCorrectionCodewordsPerBlock);

        codewordsSeen = 0;
        var blockIndex = 1;
        Span<byte> divisionDestination = stackalloc byte[256];
        divisionDestination.Clear();
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
