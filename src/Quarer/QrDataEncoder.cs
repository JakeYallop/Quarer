using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

    public static IEnumerable<byte> EncodeDataBitStream(ReadOnlySpan<char> data, QrEncodingInfo qrDataEncoding)
    {
        var version = qrDataEncoding.Version;
        var bitWriter = new BitWriter(qrDataEncoding.Version.DataCodewordsCapacity);

        foreach (var segment in qrDataEncoding.DataSegments)
        {
#pragma warning disable IDE0010 // Add missing cases
            switch (segment.Mode)
            {
                case ModeIndicator.Numeric:
                    NumericEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                case ModeIndicator.Alphanumeric:
                    AlphanumericEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                case ModeIndicator.Byte:
                    //TODO: Add byte encoder to allow making this more efficient (e.g vectorization) and allow testability
                    //e.g read 4 bytes at a time and write 32 bits
                    foreach (var c in data[segment.Range])
                    {
                        bitWriter.WriteBits(c, 8);
                    }
                    break;
                case ModeIndicator.Kanji:
                    KanjiEncoder.Encode(bitWriter, data[segment.Range]);
                    break;
                default:
                    throw new UnreachableException($"Mode '{segment.Mode}' not expected.");
            }
#pragma warning restore IDE0010 // Add missing cases
        }

        QrTerminatorBlock.WriteTerminator(bitWriter, version);

        var codewords = bitWriter.ByteCount;
        while (codewords <= version.DataCodewordsCapacity - 4)
        {
            bitWriter.WriteBits(PadPattern32Bits, 32);
            codewords += 4;
        }

        var alternate = false;
        while (codewords < version.DataCodewordsCapacity)
        {
            bitWriter.WriteBits(alternate ? PadPattern8_1 : PadPattern8_2, 8);
            codewords++;
        }

        return bitWriter.GetByteStream();
    }

    private const byte PadPattern8_1 = 0b1110_1100;
    private const byte PadPattern8_2 = 0b0001_0001;
    private const uint PadPattern32Bits = unchecked((uint)((PadPattern8_1 << 24) | (PadPattern8_2 << 16) | (PadPattern8_1 << 8) | PadPattern8_2));
}

public enum AnalysisResult
{
    Success = 1,
    DataTooLarge
}
