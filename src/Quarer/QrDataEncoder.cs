using System.Buffers;
using System.Collections.Immutable;

namespace Quarer;

public static class QrDataEncoder
{
    public static readonly SearchValues<char> AlphanumericCharacters = SearchValues.Create(AlphanumericEncoder.Characters);
    public static readonly SearchValues<char> NumericCharacters = SearchValues.Create("0123456789");

    public static QrDataEncoding AnalyzeSimple(ReadOnlySpan<char> data, ErrorCorrectionLevel requestedErrorCorrectionLevel)
    {
        //For now, just use a single mode for the full set of data.
        var mode = DeriveMode(data);
        var dataLength = GetBitStreamLength(mode, data);
        if (!QrCapacityLookup.TryGetVersionForDataCapacity(dataLength, mode, requestedErrorCorrectionLevel, out var version))
        {
            return QrDataEncoding.Invalid(QrAnalysisResult.DataTooLarge);
        }

        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var segment = DataSegment.Create(characterCount, mode, dataLength, new Range(0, data.Length));
        var segments = ImmutableArray.Create(segment);
        return new QrDataEncoding(version, segments);
    }

    public static ModeIndicator DeriveMode(ReadOnlySpan<char> data)
    {
        return data.ContainsAnyExcept(AlphanumericCharacters)
            ? KanjiEncoder.ContainsAnyExceptKanji(data) ? ModeIndicator.Byte : ModeIndicator.Kanji
            : data.ContainsAnyExcept(NumericCharacters) ? ModeIndicator.Alphanumeric : ModeIndicator.Numeric;
    }

    public static int GetBitStreamLength(this ModeIndicator mode, ReadOnlySpan<char> data)
    {
#pragma warning disable IDE0072 // Add missing cases
        return mode switch
        {
            ModeIndicator.Numeric => NumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Alphanumeric => AlphanumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Byte => data.Length * 2 * 8,
            ModeIndicator.Kanji => KanjiEncoder.GetBitStreamLength(data),
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}

public enum QrAnalysisResult
{
    Success = 1,
    DataTooLarge
}
