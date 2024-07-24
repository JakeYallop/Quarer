using System.Collections.Immutable;

namespace Quarer;

public sealed class QrEncodingInfo
{
    internal QrEncodingInfo(QrVersion version, ImmutableArray<DataSegment> dataSegments)
    {
        Version = version;
        DataSegments = dataSegments;
    }

    public QrVersion Version { get; }
    public ImmutableArray<DataSegment> DataSegments { get; }
}
