using System.Collections.Immutable;

namespace Quarer;

/// <summary>
/// The encoding information for creating an adequately sized QR Code from some data.
/// </summary>
public sealed class QrEncodingInfo
{
    internal QrEncodingInfo(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ImmutableArray<DataSegment> dataSegments)
    {
        Version = version;
        ErrorCorrectionLevel = errorCorrectionLevel;
        DataSegments = dataSegments;
    }

    /// <summary>
    /// The minimum version that fits the data.
    /// </summary>
    public QrVersion Version { get; }
    /// <summary>
    /// The error correction level to use.
    /// </summary>
    public ErrorCorrectionLevel ErrorCorrectionLevel { get; }
    /// <summary>
    /// The way to encode separate chunks of data into a QR code. If returned from
    /// <see cref="QrDataEncoder.AnalyzeSimple(ReadOnlySpan{byte}, ErrorCorrectionLevel, EciCode)"/>, this will only
    /// have a single segment, indicating that all the data is to be encoded using the same mode.
    /// </summary>
    public ImmutableArray<DataSegment> DataSegments { get; }
}
