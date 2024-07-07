namespace Quarer;

public static class ModeIndicatorExtensions
{
    public static int GetBitStreamLength(this ModeIndicator mode, ReadOnlySpan<char> data)
    {
#pragma warning disable IDE0072 // Add missing cases
        return mode switch
        {
            ModeIndicator.Numeric => NumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Alphanumeric => AlphanumericEncoder.GetBitStreamLength(data),
            ModeIndicator.Byte => data.Length * 2 * 8,
            ModeIndicator.Kanji => KanjiEncoder.GetBitStreamLength(data),
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
