namespace Quarer;
/// <summary>
/// Describes information about different parts of the data bit stream within a QR code.
/// </summary>
public enum ModeIndicator
{
    /// <summary>
    /// Used for indicating the end of the data bit stream.
    /// </summary>
    Terminator = 0,
    /// <summary>
    /// Indicates that a segment of data is encoded in numeric mode.
    /// </summary>
    Numeric = 1,
    /// <summary>
    /// Indicates that a segment of data is encoded in alphanumeric mode.
    /// </summary>
    Alphanumeric = 2,
    /// <summary>
    /// Structured Append mode is used to split data across several separate QR Codes.
    /// </summary>
    StructuredAppend = 3,
    /// <summary>
    /// Indicates that a segment of data is encoded in byte mode.
    /// </summary>
    Byte = 4,
    /// <summary>
    /// 8.3.8 - FNC1 mode is used for messages containing data formatted either in accordance with the UCC/EAN Application Identifiers standard or in accordance with a specific industry standard previously agreed with AIM International
    /// </summary>
    Fnc1FirstPosition = 5,
    /// <summary>
    /// Indicates that an Exteneded Channel Interpretation (ECI) code is present and will follow this mode indicator.
    /// </summary>
    Eci = 7,
    /// <summary>
    /// Indicates that a segment of data is encoded in Kanji mode.
    /// </summary>
    Kanji = 8,
    /// <summary>
    /// 8.3.8 - FNC1 mode is used for messages containing data formatted either in accordance with the UCC/EAN Application Identifiers standard or in accordance with a specific industry standard previously agreed with AIM International
    /// </summary>
    Fnc1SecondPosition = 9,
}
