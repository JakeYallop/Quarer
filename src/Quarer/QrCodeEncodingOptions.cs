namespace Quarer;

/// <summary>
/// Options affecting the encoding of a QR code.
/// </summary>
public class QrCodeEncodingOptions
{
    internal static readonly QrCodeEncodingOptions Empty = new();

    /// <summary>
    /// Creates a new instance of the <see cref="QrCodeEncodingOptions"/> class.
    /// </summary>
    public QrCodeEncodingOptions()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="QrCodeEncodingOptions"/> class with the specified error correction level.
    /// </summary>
    public QrCodeEncodingOptions(ErrorCorrectionLevel errorCorrectionLevel)
    {
        ErrorCorrectionLevel = errorCorrectionLevel;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="QrCodeEncodingOptions"/> class with the specified version and error correction level.
    /// </summary>
    public QrCodeEncodingOptions(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        Version = version;
        ErrorCorrectionLevel = errorCorrectionLevel;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="QrCodeEncodingOptions"/> class with the specified version, error correction level, and ECI code.
    /// </summary>
    public QrCodeEncodingOptions(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        Version = version;
        ErrorCorrectionLevel = errorCorrectionLevel;
        EciCode = eciCode;
    }

    /// <summary>
    /// The Extended Channel Interpretation (ECI) code to use. Defaults to <see cref="EciCode.Empty"/>.
    /// </summary>
    public EciCode EciCode { get; init; }
    /// <summary>
    /// The version of the QR code to encode. If left <see langword="null"/>, the version will be automatically
    /// determined based on the content to encode.
    /// </summary>
    public QrVersion? Version { get; init; }
    /// <summary>
    /// The error correction level to use. if left <see langword="null"/>, the error correction level will be
    /// automatically determined based on the content to encode.
    /// </summary>
    public ErrorCorrectionLevel? ErrorCorrectionLevel { get; init; }
}
