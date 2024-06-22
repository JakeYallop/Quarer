using System.Collections.Immutable;

namespace Quarer;

public sealed class QrDataEncoding
{
    internal QrDataEncoding(QrVersion version, ImmutableArray<DataSegment> dataSegments) : this(version, dataSegments, QrAnalysisResult.Success)
    {
    }

    internal QrDataEncoding(QrVersion version, ImmutableArray<DataSegment> dataSegments, QrAnalysisResult result)
    {
        Version = version;
        DataSegments = dataSegments;
        Result = result;
    }

    internal static QrDataEncoding Invalid(QrAnalysisResult result)
    {
        return result == QrAnalysisResult.Success
            ? throw new ArgumentException("An invalid result cannot be a Success result.", nameof(result))
            : new(default, default, result);
    }

    public QrVersion Version { get; }
    public ImmutableArray<DataSegment> DataSegments { get; }
    public QrAnalysisResult Result { get; }

}
