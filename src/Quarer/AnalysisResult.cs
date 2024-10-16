namespace Quarer;

/// <summary>
/// Represents the result of analyzing data for encoding into a QR code.
/// </summary>
public enum AnalysisResult
{
    /// <summary>
    /// The data can be encoded successfully.
    /// </summary>
    Success = 0,
    /// <summary>
    /// The data is too large to fit within a QR code.
    /// </summary>
    DataTooLarge = 1,
}

