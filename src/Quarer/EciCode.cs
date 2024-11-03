using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Quarer;

/// <summary>
/// Represents an Extended Channel Interpretation (ECI) code.
/// </summary>
public readonly record struct EciCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EciCode"/> struct with the specified ECI code.
    /// </summary>
    public EciCode(byte? value)
    {
        if (value > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "ECI code must be less than 127. Multibyte ECI codes are not supported.");
        }

        Value = value;
    }

    /// <summary>
    /// The ECI code value.
    /// </summary>
    public byte? Value { get; }

    /// <summary>
    /// An empty ECI code. When encoding this will be act as if no ECI code has been specified.
    /// </summary>
    public static EciCode Empty => new();

    /// <summary>
    /// Returns <see langword="true"/> if this ECI code is empty.
    /// </summary>
    /// <returns></returns>
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsEmpty() => Value.GetValueOrDefault() == 0;

    /// <summary>
    /// Returns the amount this ECI code will contribute to the overall length of a data segment.
    /// </summary>
    /// <returns></returns>
    public int GetDataSegmentLength() => Value is not null ? 12 : 0; //ECI mode indicator (4) + ECI code (8)

    /// <summary>
    /// Returns a string representation of this ECI code. Returns an empty string if the ECI code is empty.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var value = Value.GetValueOrDefault();
        return value is 0 ? "" : value.ToString(CultureInfo.InvariantCulture);
    }
}
