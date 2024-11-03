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
    /// Creates a QR Code from the provided string with the specified version and error correction level. The string is
    /// converted to Latin-1 if possible or UTF-8 otherwise before encoding. For binary data, prefer using the
    /// <see cref="Create(ReadOnlySpan{byte}, QrCodeEncodingOptions?)"/> overload.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<char> data, QrCodeEncodingOptions? options = null)
    {
        var result = TryCreate(data, options);
        return ToValue(result);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified options.
    /// </summary>
    public static QrCode Create(ReadOnlySpan<byte> data, QrCodeEncodingOptions? options = null)
    {
        var result = TryCreate(data, options);
        return ToValue(result);
    }

    private static QrCode ToValue(QrCodeCreationResult result)
    {
        return result.Reason switch
        {
            QrCreationResult.Success => result.Value!,
            QrCreationResult.DataTooLargeSimple => throw new QrCodeException(QrCodeDataTooLargeMessage),
            QrCreationResult.EciCodeNotAllowedForCharacterData => throw new QrCodeException("ECI code cannot be set when using the char method overloads. Use the byte method overloads instead if you want to pass an ECI code."),
            _ => throw new QrCodeException(QrCodeDataTooLargeMessage)
        };
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified options. The string is converted to Latin-1 if
    /// possible or UTF-8 otherwise before encoding. For binary data, prefer using the
    /// <see cref="TryCreate(ReadOnlySpan{byte}, QrCodeEncodingOptions?)"/> overload. If the QR code cannot be created,
    /// for example because the the data is too large to fit into the QR Code, this method will return a result with a
    /// <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for the failure.
    /// 
    /// <para>
    /// An <see cref="EciCode"/> cannot be passed to the char method overloads - use the byte method overloads instead
    /// if you want to pass an <see cref="EciCode"/>.
    /// </para>
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, QrCodeEncodingOptions? options = null)
    {
        options ??= QrCodeEncodingOptions.Empty;
        if (options.EciCode != EciCode.Empty)
        {
            return new(QrCreationResult.EciCodeNotAllowedForCharacterData);
        }
        var transcodedData = TranscodeToLatin1OrUtf8(data, out var eciCode);
        return TryCreate(transcodedData, options.Version, options.ErrorCorrectionLevel, eciCode);
    }

    /// <summary>
    /// Creates a QR Code from the provided data with the specified options. If the QR code cannot be created, for
    /// example because the the data is too large to fit into the QR Code, this method will return a result with a
    /// <see cref="QrCodeCreationResult.Success"/> value of <see langword="false"/> and the reason for the failure.
    /// </summary>
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, QrCodeEncodingOptions? options = null)
    {
        options ??= QrCodeEncodingOptions.Empty;
        return TryCreate(data, options.Version, options.ErrorCorrectionLevel, options.EciCode);
    }

    private static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, QrVersion? version, ErrorCorrectionLevel? errorCorrectionLevel, EciCode eciCode)
    {
        ReadOnlySpan<ErrorCorrectionLevel> errorLevels = errorCorrectionLevel is not null ? [errorCorrectionLevel.Value] : [ErrorCorrectionLevel.M, ErrorCorrectionLevel.L];
        if (version is not null)
        {
            foreach (var error in errorLevels)
            {
                var mode = QrDataEncoder.DeriveMode(data);
                if (!QrVersion.VersionCanFitData(version, data, error, mode, eciCode))
                {
                    continue;
                }

                var encoding = QrDataEncoder.CreateSimpleDataEncoding(data, version, error, mode, eciCode);
                return CreateQrCode(data, encoding);
            }
        }
        else
        {
            foreach (var error in errorLevels)
            {
                var analysisResult = QrDataEncoder.AnalyzeSimple(data, error, eciCode);
                if (analysisResult.Reason == AnalysisResult.Success)
                {
                    return CreateQrCode(data, analysisResult.Value!);
                }
            }
        }
        return new(QrCreationResult.DataTooLargeSimple);
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
    DataTooLargeSimple = 1,
    /// <summary>
    /// ECI code cannot be set when using the <see cref="QrCode.TryCreate(ReadOnlySpan{char}, QrCodeEncodingOptions)"/> overload. Use the byte overload instead.
    /// </summary>
    EciCodeNotAllowedForCharacterData = 2,
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
