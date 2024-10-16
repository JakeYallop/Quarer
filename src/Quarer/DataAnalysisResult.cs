using System.Diagnostics.CodeAnalysis;

namespace Quarer;

/// <summary>
/// Represents the result of analyzing data for encoding into a QR code.
/// </summary>
public sealed class DataAnalysisResult
{
    private DataAnalysisResult(QrEncodingInfo encoding) : this(encoding, AnalysisResult.Success)
    {
    }

    private DataAnalysisResult(QrEncodingInfo? encoding, AnalysisResult result)
    {
        Value = encoding;
        Reason = result;
    }
    /// <summary>
    /// The encoding information for the data, including the mode and minimum version for the QR code.
    /// </summary>
    public QrEncodingInfo? Value { get; }

    /// <summary>
    /// The reason the data could not be encoded, or <see cref="AnalysisResult.Success" /> if the data can be encoded
    /// successfully.
    /// </summary>
    public AnalysisResult Reason { get; }

    /// <summary>
    /// Returns <see langword="true"/> if the data can be encoded successfully, or <see langword="false"/> otherwise.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Success => Reason is AnalysisResult.Success;

    internal static DataAnalysisResult Invalid(AnalysisResult result)
        => result == AnalysisResult.Success
            ? throw new ArgumentException("Cannot create an invalid result from a success.", nameof(result))
            : new(null, result);

    internal static DataAnalysisResult Successful(QrEncodingInfo encoding)
        => new(encoding);
}

