namespace Quarer;

/// <summary>
/// Extensions for the <see cref="ModeIndicator"/> enum.
/// </summary>
public static class ModeIndicatorExtensions
{
    /// <summary>
    /// Gets the length in bits of the bit stream when encoding the specified data in the given mode.
    /// </summary>
    public static int GetBitStreamLength(this ModeIndicator mode, ReadOnlySpan<byte> data)
    {
#pragma warning disable IDE0072 // Add missing cases
        return mode switch
        {
            ModeIndicator.Numeric => NumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Alphanumeric => AlphanumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Byte => data.Length * 8,
            ModeIndicator.Kanji => KanjiEncoder.GetBitStreamLength(data),
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
    }

    /// <summary>
    /// Gets the length in data characters of the specified data when encoded in the given mode.
    /// </summary>
    /// <remarks>
    /// This is mostly different for Kanji mode, where each character consists of two bytes.
    /// </remarks>
    public static int GetDataCharacterLength(this ModeIndicator mode, ReadOnlySpan<byte> data)
    {
        return mode switch
        {
            ModeIndicator.Numeric => data.Length,
            ModeIndicator.Alphanumeric => data.Length,
            ModeIndicator.Byte => data.Length,
            ModeIndicator.Kanji => data.Length >> 1,
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
