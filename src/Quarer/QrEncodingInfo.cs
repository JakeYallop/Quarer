using System.Collections.Immutable;

namespace Quarer;

public sealed class QrEncodingInfo
{
    internal QrEncodingInfo(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ImmutableArray<DataSegment> dataSegments)
    {
        Version = version;
        ErrorCorrectionLevel = errorCorrectionLevel;
        DataSegments = dataSegments;
    }

    public QrVersion Version { get; }
    public ErrorCorrectionLevel ErrorCorrectionLevel { get; }
    public ImmutableArray<DataSegment> DataSegments { get; }
}
