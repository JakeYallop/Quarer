﻿using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed class QrCode : IEquatable<QrCode>
{
    private const string QrCodeDataTooLargeMessage = "Failed to create QR code, provided data is too large to fit within the QR code capacity using the available encoding capabilities.";

    internal QrCode(QrVersion version, MaskPattern maskPattern, BitMatrix data)
    {
        Version = version;
        MaskPattern = maskPattern;
        Data = data;
    }

    public QrVersion Version { get; }
    public MaskPattern MaskPattern { get; }
    public BitMatrix Data { get; }
    public ErrorCorrectionLevel ErrorCorrectionLevel => Version.ErrorCorrectionLevel;
    public int Width => Version.ModulesPerSide;
    public int Height => Version.ModulesPerSide;

    public static bool operator ==(QrCode? left, QrCode? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(QrCode? left, QrCode? right) => !(left == right);

    public bool Equals([NotNullWhen(true)] QrCode? other) => other is not null && Version == other.Version && MaskPattern == other.MaskPattern && Data == other.Data;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrCode other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Version, MaskPattern, Data);

    public static QrCode Create(ReadOnlySpan<char> data)
    {
        // try default error correct level (M), then try successively lower levels as these have more space
        // we exit out early in try create if we do not have space, so this should not attempt to encode the entire QR code
        foreach (var ec in (ReadOnlySpan<ErrorCorrectionLevel>)[ErrorCorrectionLevel.M, ErrorCorrectionLevel.L])
        {
            var result = TryCreate(data, ec);
            if (result.Success)
            {
                return result.Value;
            }
        }

        throw new QrCodeException(QrCodeDataTooLargeMessage);

    }

    public static QrCode Create(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var result = TryCreate(data, errorCorrectionLevel);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }

    public static QrCode Create(ReadOnlySpan<char> data, QrVersion version)
    {
        var result = TryCreate(data, version);
        return result.Success ? result.Value : throw new QrCodeException(QrCodeDataTooLargeMessage);
    }


    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data)
    {
        QrCodeCreationResult result = null!;
        // try default error correct level (M), then try successively lower levels as these have more space
        // we exit out early in try create if we do not have space, so this should not attempt to encode the entire QR code
        foreach (var ec in (ReadOnlySpan<ErrorCorrectionLevel>)[ErrorCorrectionLevel.M, ErrorCorrectionLevel.L])
        {
            result = TryCreate(data, ec);
            if (result.Success)
            {
                break;
            }
        }
        return result;
    }

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var analysisResult = QrDataEncoder.AnalyzeSimple(data, errorCorrectionLevel);
        return analysisResult.AnalysisResult switch
        {
            AnalysisResult.DataTooLarge => new(QrCreationResult.DataTooLargeSimple),
            AnalysisResult.Success => CreateQrCode(data, analysisResult.Result!),
            _ => throw new InvalidOperationException("Unexpected analysis result.")
        };
    }

    public static QrCodeCreationResult TryCreate(ReadOnlySpan<char> data, QrVersion version)
    {
        var mode = QrDataEncoder.DeriveMode(data);
        if (!QrVersionLookup.VersionCanFitData(version, data, mode))
        {
            return new(QrCreationResult.DataTooLargeSimple);
        }

        var encoding = QrDataEncoder.CreateSimpleDataEncoding(data, version, mode);
        return CreateQrCode(data, encoding);
    }

    private static QrCodeCreationResult CreateQrCode(ReadOnlySpan<char> data, QrEncodingInfo encodingInfo)
    {
        var dataCodewords = QrDataEncoder.EncodeDataBits(encodingInfo, data);
        var withErrorCodewords = QrDataEncoder.EncodeAndInterleaveErrorCorrectionBlocks(encodingInfo.Version, dataCodewords);
        var (matrix, maskPattern) = QrSymbolBuilder.BuildSymbol(encodingInfo.Version, withErrorCodewords);
        var qrCode = new QrCode(encodingInfo.Version, maskPattern, matrix);
        return new(qrCode);
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
}

public enum QrCreationResult
{
    Success = 0,
    /// <summary>
    /// Based on a simple analysis of the data, the data is too large to fit into a QR Code.
    /// </summary>
    DataTooLargeSimple = 1
}

public sealed class QrCodeException : Exception
{
    internal QrCodeException(string message) : base(message)
    {
    }

    internal QrCodeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
