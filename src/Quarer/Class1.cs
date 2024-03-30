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

public readonly struct QrVersion : IEquatable<QrVersion>
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

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);

    public static bool operator ==(QrVersion left, QrVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(QrVersion left, QrVersion right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to <paramref name="other"/>; otherwise, <see langword="false" />.</returns>
    public bool Equals(QrVersion other) => Version == other.Version;

    public override int GetHashCode() => Version.GetHashCode();
}
