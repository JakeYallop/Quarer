using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

/// <summary>
/// A reprensentation of a QR (Quick Response) Code.
/// </summary>
public sealed class QrCode : IEquatable<QrCode>
{
    private const string QrCodeDataTooLargeMessage = "Failed to create QR code, provided data is too large to fit within the QR code capacity using the available encoding capabilities.";

    internal QrCode(QrVersion version, ByteMatrix data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        Version = version;
        Data = data;
        ErrorCorrectionLevel = errorCorrectionLevel;
    }

    /// <summary>
    /// The data matrix for this QR Code.
    /// </summary>
    public ByteMatrix Data { get; }
    /// <summary>
    /// The version of this QR Code.
    /// </summary>
    public QrVersion Version { get; }
    /// <summary>
    /// The error correction level of this QR Code.
    /// </summary>
    public ErrorCorrectionLevel ErrorCorrectionLevel { get; }
    /// <summary>
    /// The width of this QR Code.
    /// </summary>
    public int Width => Version.Width;
    /// <summary>
    /// The height of this QR Code.
    /// </summary>
    public int Height => Version.Height;

    /// <summary>
    /// Creates a QR Code from the provided string. The string is converted to Latin-1 if possible or UTF-8 otherwise
    /// before encoding.
    /// For specific binary data, prefer using the <see cref="Create(ReadOnlySpan{byte})"/> overloads.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<char> data)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return Create(transcodedData, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided string with the specified error correction level. The string is
    /// converted to Latin-1 if possible or UTF-8 otherwise before encoding.
    /// For specific binary data, prefer using the <see cref="Create(ReadOnlySpan{byte}, ErrorCorrectionLevel)"/> overloads.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return Create(transcodedData, errorCorrectionLevel, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided string with the specified version and error correction level. The string is
    /// converted to Latin-1 if possible or UTF-8 otherwise before encoding.
    /// For specific binary data, prefer using the <see cref="Create(ReadOnlySpan{byte}, QrVersion, ErrorCorrectionLevel)"/> overloads.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<char> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return Create(transcodedData, version, errorCorrectionLevel, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided data.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data) => Create(data, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data, with the format or encoding of the data specified using an ECI code.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, EciCode eciCode)
    {
        var result = TryCreate(data, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified error correction level.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel) => Create(data, errorCorrectionLevel, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data with the specified error correction level and with the format o
    /// encoding of the data specified using an ECI code.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var result = TryCreate(data, errorCorrectionLevel, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified version and error correction level.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel) => Create(data, version, errorCorrectionLevel, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data with the specified version, error correction level, with the format or
    /// encoding of the data specified using an ECI code.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var result = TryCreate(data, version, errorCorrectionLevel, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }

    /// <summary>
    /// Creates a QR Code from the provided data. The string is
    /// converted to Latin-1 if possible or UTF-8 otherwise before encoding. If the QR code cannot be created, for
    /// example because the the data is too large to fit into the QR Code, this method will return a result with a
    /// <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return TryCreate(transcodedData, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified error correction level. The string is converted to
    /// Latin-1 if possible or UTF-8 otherwise before encoding. If the QR code cannot be created, for example because
    /// the the data is too large to fit into the QR Code, this method will return a result with a
    /// <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return TryCreate(transcodedData, errorCorrectionLevel, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified version and error correction level. The string is
    /// converted to Latin-1 if possible or UTF-8 otherwise before encoding. If the QR code cannot
    /// be created, for example because the the data is too large to fit into the QR Code, this method will return a
    /// result with a <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for
    /// the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return TryCreate(transcodedData, version, errorCorrectionLevel, eciCode);
    }

    /// <inheritdoc cref="QrCode.TryCreate(ReadOnlySpan{char})" />
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data) => TryCreate(data, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data, with the format or encoding of the data specified using an ECI code.
    /// If the QR code cannot be created, for example because the the data is too large to fit into the QR Code, this
    /// method will return a result with a <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/>
    /// and the reason for the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, EciCode eciCode)
    {
        QrCodeCreationResult result = null!;
        // try default error correct level (M), then try successively lower levels as these have more space
        // we exit out early in try create if we do not have space, so this should not attempt to encode the entire QR code
        foreach (var ec in (ReadOnlySpan<ErrorCorrectionLevel>)[ErrorCorrectionLevel.M, ErrorCorrectionLevel.L])
        {
            result = TryCreate(data, ec, eciCode);
            if (result.Success)
            {
                break;
            }
        }
        return result;
    }

    /// <inheritdoc cref="QrCode.TryCreate(ReadOnlySpan{char}, ErrorCorrectionLevel)" />
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel) => TryCreate(data, errorCorrectionLevel, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data with the specified error correction level, with the format or encoding
    /// of the data specified using an ECI code. If the QR code cannot be created, for example because the the data is too
    /// large to fit into the QR Code, this method will return a result with a <see cref="QrCodeCreationResult.Success"/>
    /// value of <see langword="false"/> and the reason for the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var analysisResult = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel, eciCode);
        return analysisResult.Reason switch
        {
            AnalysisResult.DataTooLarge => new(QrCreationResult.DataTooLargeSimple),
            AnalysisResult.Success => CreateQrCode(data, analysisResult.Value!),
            _ => throw new InvalidOperationException("Unexpected analysis result.")
        };
    }

    /// <inheritdoc cref="QrCode.TryCreate(ReadOnlySpan{char}, QrVersion, ErrorCorrectionLevel)" />
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
        => TryCreate(data, version, errorCorrectionLevel, EciCode.Empty);

    /// <summary>
    /// Creates a QR Code from the provided data with the specified version and error correction level, with the
    /// format or encoding of the data specified using an ECI code. If the QR code cannot be created, for example
    /// because the the data is too large to fit into the QR Code, this method will return a result with a
    /// <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for the failure.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="version"></param>
    /// <param name="errorCorrectionLevel"></param>
    /// <param name="eciCode"></param>
    /// <returns></returns>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var mode = QrDataEncoder.DeriveMode(data);
        if (!QrVersion.VersionCanFitData(version, data, errorCorrectionLevel, mode, eciCode))
        {
            return new(QrCreationResult.DataTooLargeSimple);
        }

        var encoding = QrDataEncoder.CreateSimpleDataEncoding(data, version, errorCorrectionLevel, mode, eciCode);
        return CreateQrCode(data, encoding);
    }

    private static QrCodeCreationResult CreateQrCode(ReadOnlySpan<byte> data, QrEncodingInfo encodingInfo)
    {
        var dataCodewords = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        var withErrorCodewords = QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(dataCodewords, encodingInfo.Version, encodingInfo.ErrorCorrectionLevel);
        var (matrix, _) = QrSymbolBuilder.BuildSymbol(withErrorCodewords, encodingInfo.Version, encodingInfo.ErrorCorrectionLevel);
        var qrCode = new QrCode(encodingInfo.Version, matrix, encodingInfo.ErrorCorrectionLevel);
        return new(qrCode);
    }

    /// <summary>
    /// Returns a value indicating if two <see cref="QrCode"/> instances are equal.
    /// </summary>
    public static bool operator ==(QrCode? left, QrCode? right) => left is null ? right is null : left.Equals(right);
    /// <summary>
    /// Returns a value indicating if two <see cref="QrCode"/> instances are not equal.
    /// </summary>
    public static bool operator !=(QrCode? left, QrCode? right) => !(left == right);
    /// <inheritdoc />
    public bool Equals([NotNullWhen(true)] QrCode? other) => other is not null && Version == other.Version && ErrorCorrectionLevel == other.ErrorCorrectionLevel && Data == other.Data;
    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrCode other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Version, ErrorCorrectionLevel, Data);

    private static bool IsValidLatin1(ReadOnlySpan<char> data) => !Latin1Validator.ContainsNonLatin1Characters(data);
    private static ReadOnlySpan<byte> TranscodeToLatin1OrUtf8(ReadOnlySpan<char> data, out EciCode eciCode)
    {
        ReadOnlySpan<byte> transcodedData;
        if (!IsValidLatin1(data))
        {
            var buffer = new byte[Encoding.UTF8.GetByteCount(data)];
            var written = Encoding.UTF8.GetBytes(data, buffer);
            Debug.Assert(written == buffer.Length);
            transcodedData = buffer;
            eciCode = new EciCode(26);
        }
        else
        {
            var buffer = new byte[Encoding.Latin1.GetByteCount(data)];
            var written = Encoding.Latin1.GetBytes(data, buffer);
            Debug.Assert(written == buffer.Length);
            transcodedData = buffer;
            eciCode = EciCode.Empty;
            // latin1 encoding (ISO-8859-1) is the encoding for QR codes, we can store a tiny bit more data in latin1 vs utf8
        }

        return transcodedData;
    }

}

/// <summary>
/// The result a QR Code creation operation.
/// </summary>
public sealed class QrCodeCreationResult
{
    internal QrCodeCreationResult(QrCreationResult result)
    {
        Reason = result;
    }
    internal QrCodeCreationResult(QrCode value) : this(QrCreationResult.Success)
    {
        Value = value;
    }

    /// <summary>
    /// The created QR Code, if the operation was successful.
    /// </summary>
    public QrCode? Value { get; }

    /// <summary>
    /// The reason the operation failed, or <see cref="QrCreationResult.Success"/> if the operation was successful.
    /// </summary>
    public QrCreationResult Reason { get; }

    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Success => Reason == QrCreationResult.Success;
}

/// <summary>
/// The result reason of a QR Code creation operation.
/// </summary>
public enum QrCreationResult
{
    /// <summary>
    /// The QR Code creation operation was successful.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Based on a simple analysis of the data, the data is too large to fit into a QR Code.
    /// </summary>
    DataTooLargeSimple = 1
}

/// <summary>
/// Thrown when an error occurs while creating a QR Code.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class QrCodeException : Exception
{
    internal QrCodeException(string message) : base(message)
    {
    }

    internal QrCodeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
