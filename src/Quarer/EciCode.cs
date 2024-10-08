using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Quarer;

public readonly record struct EciCode
{
    public EciCode(byte? value)
    {
        if (value > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "ECI code must be less than 127. Multibyte ECI codes are not supported.");
        }

        Value = value;
    }

    public byte? Value { get; }

    public static EciCode Empty => new(null);

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsEmpty() => Value.GetValueOrDefault() == 0;

    /// <summary>
    /// Returns the amount this ECI code will contribute to the overall length of a data segment.
    /// </summary>
    /// <returns></returns>
    public int GetDataSegmentLength() => Value is not null ? 12 : 0; //ECI mode indicator (4) + ECI code (8)

    public override string ToString()
    {
        var value = Value.GetValueOrDefault();
        return value is 0 ? "" : value.ToString(CultureInfo.InvariantCulture);
    }
}
