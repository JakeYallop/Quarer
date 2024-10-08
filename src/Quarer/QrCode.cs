using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quarer;

public sealed class QrCode : IEquatable<QrCode>
{
    private const string QrCodeDataTooLargeMessage = "Failed to create QR code, provided data is too large to fit within the QR code capacity using the available encoding capabilities.";

    internal QrCode(QrVersion version, ByteMatrix data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        Version = version;
        Data = data;
        ErrorCorrectionLevel = errorCorrectionLevel;
    }

    public ByteMatrix Data { get; }
    public QrVersion Version { get; }
    public ErrorCorrectionLevel ErrorCorrectionLevel { get; }
    public int Width => Version.ModulesPerSide;
    public int Height => Version.ModulesPerSide;

    private static bool IsValidLatin1(ReadOnlySpan<char> data) => !Latin1Validator.ContainsNonLatin1Characters(data);
    private static ReadOnlySpan<byte> Transcode(ReadOnlySpan<char> data, out EciCode eciCode)
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

    public static QrCode Create(ReadOnlySpan<char> data)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return Create(transcodedData, eciCode);
    }
    public static QrCode Create(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return Create(transcodedData, errorCorrectionLevel, eciCode);
    }
    public static QrCode Create(ReadOnlySpan<char> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return Create(transcodedData, version, errorCorrectionLevel, eciCode);
    }

    public static QrCode Create(ReadOnlySpan<byte> data) => Create(data, EciCode.Empty);
    public static QrCode Create(ReadOnlySpan<byte> data, EciCode eciCode)
    {
        var result = TryCreate(data, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }
    public static QrCode Create(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel) => Create(data, errorCorrectionLevel, EciCode.Empty);
    public static QrCode Create(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var result = TryCreate(data, errorCorrectionLevel, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }
    public static QrCode Create(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel) => Create(data, version, errorCorrectionLevel, EciCode.Empty);
    public static QrCode Create(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, EciCode eciCode)
    {
        var result = TryCreate(data, version, errorCorrectionLevel, eciCode);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return TryCreate(transcodedData, eciCode);
    }
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return TryCreate(transcodedData, errorCorrectionLevel, eciCode);
    }
    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var transcodedData = Transcode(data, out var eciCode);
        return TryCreate(transcodedData, version, errorCorrectionLevel, eciCode);
    }

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data) => TryCreate(data, EciCode.Empty);
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

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel) => TryCreate(data, errorCorrectionLevel, EciCode.Empty);
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

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<byte> data, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
        => TryCreate(data, version, errorCorrectionLevel, EciCode.Empty);
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

    public static bool operator ==(QrCode? left, QrCode? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(QrCode? left, QrCode? right) => !(left == right);

    public bool Equals([NotNullWhen(true)] QrCode? other) => other is not null && Version == other.Version && ErrorCorrectionLevel == other.ErrorCorrectionLevel && Data == other.Data;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrCode other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Version, ErrorCorrectionLevel, Data);

}
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

    public QrCode? Value { get; }
    public QrCreationResult Reason { get; }
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Success => Reason == QrCreationResult.Success;
}

public enum QrCreationResult
{
    Success = 0,
    /// <summary>
    /// Based on a simple analysis of the data, the data is too large to fit into a QR Code.
    /// </summary>
    DataTooLargeSimple = 1
}

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
