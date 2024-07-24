namespace Quarer;
public enum ModeIndicator
{
    Terminator = 0,
    Numeric = 1,
    Alphanumeric = 2,
    /// <summary>
    /// Structured Append mode is used to split data across several separate QR codes.
    /// </summary>
    StructuredAppend = 3,
    Byte = 4,
    /// <summary>
    /// 8.3.8 - FNC1 mode is used for messages containing data formatted either in accordance with the UCC/EAN Application Identifiers standard or in accordance with a specific industry standard previously agreed with AIM International
    /// </summary>
    Fnc1FirstPosition = 5,
    /// <summary>
    /// Exteneded Channel Interpretation
    /// </summary>
    Eci = 7,
    Kanji = 8,
    Fnc1SecondPosition = 9,
}
