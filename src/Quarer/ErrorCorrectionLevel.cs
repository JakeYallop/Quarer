namespace Quarer;
/// <summary>
/// The error correction level of a QR Code.
/// </summary>
public enum ErrorCorrectionLevel
{
    /// <summary>
    /// Level L: 7% of codewords can be restored.
    /// </summary>
    L = 0b01,
    /// <summary>
    /// Level M: 15% of codewords can be restored.
    /// </summary>
    M = 0b00,
    /// <summary>
    /// Level Q: 25% of codewords can be restored.
    /// </summary>
    Q = 0b11,
    /// <summary>
    /// Level H: 30% of codewords can be restored.
    /// </summary>
    H = 0b10
}
