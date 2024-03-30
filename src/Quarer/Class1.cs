using System.Diagnostics.CodeAnalysis;

namespace Quarer;

#pragma warning disable CA1027 // Mark enums with FlagsAttribute
public enum QrModeIndicator
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
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
    ECI = 7,
    Kanji = 8,
    Fnc1SecondPosition = 9,
    /// <summary>
    /// Exteneded Channel Interpretation
    /// </summary>
}

public readonly struct QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    public const byte MinVersion = 1;
    public const byte MaxVersion = 40;

    public QrVersion() : this(MinVersion) { }
    public QrVersion(byte version)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(version, 40);

        Version = version;
    }

    public readonly byte Version { get; }

    public static bool operator ==(QrVersion left, QrVersion right) => left.Equals(right);
    public static bool operator !=(QrVersion left, QrVersion right) => !(left == right);
    public static bool operator <(QrVersion left, QrVersion right) => left.CompareTo(right) < 0;
    public static bool operator <=(QrVersion left, QrVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >(QrVersion left, QrVersion right) => left.CompareTo(right) > 0;
    public static bool operator >=(QrVersion left, QrVersion right) => left.CompareTo(right) >= 0;

    public static byte ToByte(QrVersion version) => version.Version;
    public static QrVersion FromByte(byte version) => new(version);
    public int CompareTo(QrVersion other) => Version.CompareTo(other.Version);

    public static implicit operator byte(QrVersion version)
        => version.Version;
    public static explicit operator QrVersion(byte version)
        => new(version);

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to <paramref name="other"/>; otherwise, <see langword="false" />.</returns>
    public bool Equals(QrVersion other) => Version == other.Version;
    public override int GetHashCode() => Version.GetHashCode();

}

internal static class CharacterCountIndicator
{
    internal static ReadOnlySpan<short> CharacterCountNumeric => [10, 12, 14];
    internal static ReadOnlySpan<short> CharacterCountAlphanumeric => [9, 11, 13];
    internal static ReadOnlySpan<short> CharacterCountByte => [8, 16, 16];
    internal static ReadOnlySpan<short> CharacterCountKanji => [8, 10, 12];

    public static short GetCharacterCount(QrModeIndicator mode, QrVersion version)
    {
        var offset = version.Version switch
        {
            >= 1 and <= 9 => 0,
            > 9 and <= 26 => 1,
            >= 27 and <= 40 => 2,
            _ => throw new NotSupportedException($"Invalid QrVersion found. Expected a version from 1 and 40, but found '{version.Version}'.")
        };

#pragma warning disable IDE0072 // Add missing cases
        return mode switch
        {
            QrModeIndicator.Numeric => CharacterCountNumeric[offset],
            QrModeIndicator.Alphanumeric => CharacterCountAlphanumeric[offset],
            QrModeIndicator.Byte => CharacterCountByte[offset],
            QrModeIndicator.Kanji => CharacterCountKanji[offset],
            _ => throw new NotSupportedException($"Unexpected QrModeIndicator '{mode}'. Character counts are only required by {QrModeIndicator.Numeric}, {QrModeIndicator.Alphanumeric}, {QrModeIndicator.Byte}, {QrModeIndicator.Kanji} modes."),
        };
#pragma warning restore IDE0072 // Add missing cases
    }
}
