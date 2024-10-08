namespace Quarer;

public static class ModeIndicatorExtensions
{
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
