using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Quarer;

public readonly struct QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    // TODO: Replace with ImmutableArray; https://github.com/xunit/xunit/issues/2970
    private static readonly QrVersion[] QrVersionsLookup = [
            new QrVersion(1, 26, 0),
            new QrVersion(2, 44, 7),
            new QrVersion(3, 70, 7),
            new QrVersion(4, 100, 7),
            new QrVersion(5, 134, 7),
            new QrVersion(6, 172, 7),
            new QrVersion(7, 196, 0),
            new QrVersion(8, 242, 0),
            new QrVersion(9, 292, 0),
            new QrVersion(10, 346, 0),
            new QrVersion(11, 404, 0),
            new QrVersion(12, 466, 0),
            new QrVersion(13, 532, 0),
            new QrVersion(14, 581, 3),
            new QrVersion(15, 655, 3),
            new QrVersion(16, 733, 3),
            new QrVersion(17, 815, 3),
            new QrVersion(18, 901, 3),
            new QrVersion(19, 991, 3),
            new QrVersion(20, 1085, 3),
            new QrVersion(21, 1156, 4),
            new QrVersion(22, 1258, 4),
            new QrVersion(23, 1364, 4),
            new QrVersion(24, 1474, 4),
            new QrVersion(25, 1588, 4),
            new QrVersion(26, 1706, 4),
            new QrVersion(27, 1828, 4),
            new QrVersion(28, 1921, 3),
            new QrVersion(29, 2051, 3),
            new QrVersion(30, 2185, 3),
            new QrVersion(31, 2323, 3),
            new QrVersion(32, 2465, 3),
            new QrVersion(33, 2611, 3),
            new QrVersion(34, 2761, 3),
            new QrVersion(35, 2876, 0),
            new QrVersion(36, 3034, 0),
            new QrVersion(37, 3196, 0),
            new QrVersion(38, 3362, 0),
            new QrVersion(39, 3532, 0),
            new QrVersion(40, 3706, 0)
    ];

    public const byte MinVersion = 1;
    public const byte MaxVersion = 40;
    public static readonly QrVersion Max = new(40, 3706, 0);
    public static readonly QrVersion Min = new(1, 26, 0);

    public QrVersion() : this(MinVersion, 26, 0) { }

    private QrVersion(byte version, ushort dataCapacityCodewords, byte remainderBits)
    {
        Version = version;
        DataCapactiyCodewords = dataCapacityCodewords;
        RemainderBits = remainderBits;
    }

    public static QrVersion GetVersion(byte version)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(version, 40);

        return QrVersionsLookup[version - 1];
    }

    public readonly byte Version { get; }
    public readonly ushort DataCapactiyCodewords { get; }
    public readonly byte RemainderBits { get; }

    public static bool operator ==(QrVersion left, QrVersion right) => left.Equals(right);
    public static bool operator !=(QrVersion left, QrVersion right) => !(left == right);
    public static bool operator <(QrVersion left, QrVersion right) => left.CompareTo(right) < 0;
    public static bool operator <=(QrVersion left, QrVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >(QrVersion left, QrVersion right) => left.CompareTo(right) > 0;
    public static bool operator >=(QrVersion left, QrVersion right) => left.CompareTo(right) >= 0;

    public static byte ToByte(QrVersion version) => version.Version;
    public static QrVersion FromByte(byte version) => GetVersion(version);
    public readonly int CompareTo(QrVersion other) => Version.CompareTo(other.Version);

    public static implicit operator byte(QrVersion version)
        => version.Version;
    public static explicit operator QrVersion(byte version)
        => GetVersion(version);

    public override readonly string ToString() => Version.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc cref="object.Equals(object?)" />
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to <paramref name="other"/>; otherwise, <see langword="false" />.</returns>
    public bool Equals(QrVersion other) => Version == other.Version;
    public override readonly int GetHashCode() => Version.GetHashCode();
}
