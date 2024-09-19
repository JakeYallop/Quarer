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
            //TODO: This is right for char, but feels wierd, maybe we need a ROS<byte> overload?
            ModeIndicator.Byte => data.Length * 2 * 8,
            ModeIndicator.Kanji => KanjiEncoder.GetBitStreamLength(data),
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
    }

    public static int GetDataCharacterLength(this ModeIndicator mode, ReadOnlySpan<char> data)
    {
        return mode switch
        {
            ModeIndicator.Numeric => data.Length,
            ModeIndicator.Alphanumeric => data.Length,
            //TODO: This is right for char, but feels wierd, maybe we need a ROS<byte> overload?
            ModeIndicator.Byte => data.Length * 2,
            ModeIndicator.Kanji => data.Length,
            _ => throw new NotSupportedException($"Mode must be one of {ModeIndicator.Numeric}, {ModeIndicator.Alphanumeric}, {ModeIndicator.Byte} or {ModeIndicator.Kanji}.")
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
