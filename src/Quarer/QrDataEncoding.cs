namespace Quarer;

public sealed class QrDataEncoding
{
    public QrDataEncoding(QrVersion version, (ModeIndicator, Range)[] dataSegments)
    {
        Version = version;
        DataSegments = dataSegments;
    }

    public QrVersion Version { get; set; }
    public (ModeIndicator, Range)[] DataSegments { get; }
}
