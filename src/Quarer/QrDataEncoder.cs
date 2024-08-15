﻿using System.Buffers;
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

    public static IEnumerable<byte> EncodeDataBitStream(QrEncodingInfo qrDataEncoding, ReadOnlySpan<char> data)
    {
        var version = qrDataEncoding.Version;
        var bitBuffer = new BitBuffer(qrDataEncoding.Version.DataCodewordsCapacity);

        foreach (var segment in qrDataEncoding.DataSegments)
        {
#pragma warning disable IDE0010 // Add missing cases
            switch (segment.Mode)
            {
                case ModeIndicator.Numeric:
                    NumericEncoder.Encode(bitBuffer, data[segment.Range]);
                    break;
                case ModeIndicator.Alphanumeric:
                    AlphanumericEncoder.Encode(bitBuffer, data[segment.Range]);
                    break;
                case ModeIndicator.Byte:
                    //TODO: Add byte encoder to allow making this more efficient (e.g vectorization) and allow testability
                    //e.g read 4 bytes at a time and write 32 bits
                    foreach (var c in data[segment.Range])
                    {
                        bitBuffer.WriteBits(c, 8);
                    }
                    break;
                case ModeIndicator.Kanji:
                    KanjiEncoder.Encode(bitBuffer, data[segment.Range]);
                    break;
                default:
                    throw new UnreachableException($"Mode '{segment.Mode}' not expected.");
            }
#pragma warning restore IDE0010 // Add missing cases
        }

        QrTerminatorBlock.WriteTerminator(bitBuffer, version);

        var codewords = bitBuffer.ByteCount;
        while (codewords <= version.DataCodewordsCapacity - 4)
        {
            bitBuffer.WriteBits(PadPattern32Bits, 32);
            codewords += 4;
        }

        var alternate = false;
        while (codewords < version.DataCodewordsCapacity)
        {
            bitBuffer.WriteBits(alternate ? PadPattern8_1 : PadPattern8_2, 8);
            codewords++;
        }

        return bitBuffer.GetByteStream();
    }

    private const byte PadPattern8_1 = 0b1110_1100;
    private const byte PadPattern8_2 = 0b0001_0001;
    private const uint PadPattern32Bits = unchecked((uint)((PadPattern8_1 << 24) | (PadPattern8_2 << 16) | (PadPattern8_1 << 8) | PadPattern8_2));

    public static unsafe BitBuffer EncodeAndInterleaveErrorCorrectionBlocks(QrVersion version, BitBuffer dataCodewordsBitBuffer)
    {
        if (dataCodewordsBitBuffer.ByteCount != version.DataCodewordsCapacity)
        {
            throw new ArgumentException("Size of input data and size of version do not match.", nameof(dataCodewordsBitBuffer));
        }

        var errorCorrectionCodewordsPerBlock = version.ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock;
        var errorCorrectionBlocks = version.ErrorCorrectionBlocks;
        var maxDataCodewordsInBlocks = errorCorrectionBlocks.MaxDataCodewordsInBlock;
        var resultBitBuffer = new BitBuffer(version.TotalCodewords >> 2);

        // max number of codewords in a data block is 123, so ensure buffer size is greater than or equal to 123
        Span<byte> dataCodewordsDestination = stackalloc byte[128];

        var codewordsSeen = 0;
        for (var i = 0; i < maxDataCodewordsInBlocks; i++)
        {
            foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
            {
                if (i < b.DataCodewordsPerBlock)
                {
                    //TODO: Add indexer to BitBuffer?
                    var written = dataCodewordsBitBuffer.GetBytes(codewordsSeen + i, 1, dataCodewordsDestination);
                    Debug.Assert(written == 1);
                    //var dataCodewords = dataCodewordsDestination[..written];
                    resultBitBuffer.WriteBits(dataCodewordsDestination[0], 8);
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
            Span<byte> buffer = errorBlocks[blockIndex - 1];
            errorCodewords.CopyTo(buffer);
            blockIndex++;
            codewordsSeen = b.DataCodewordsPerBlock;
        }

        for (var i = 0; i < errorCorrectionCodewordsPerBlock; i++)
        {
            var errorBlockIndex = 0;
            foreach (var b in errorCorrectionBlocks.EnumerateIndividualBlocks())
            {
                ReadOnlySpan<byte> bytes = errorBlocks[errorBlockIndex];
                resultBitBuffer.WriteBits(bytes[i], 8);
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
