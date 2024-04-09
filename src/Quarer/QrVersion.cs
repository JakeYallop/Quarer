using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Quarer;

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

    public override string ToString() => Version.ToString(CultureInfo.InvariantCulture);

}
