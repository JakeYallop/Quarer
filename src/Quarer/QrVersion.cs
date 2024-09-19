using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed partial class QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    public const byte MinVersion = 1;
    public const byte MaxVersion = 40;
    public const int MaxModulesPerSide = 17 + (4 * MaxVersion);

    internal QrVersion(byte version, ushort totalCodewords, byte remainderBits)
    {
        Version = version;
        AlignmentPatternCenters = AlignmentPatternCentersLookup[version - 1];
        RemainderBits = remainderBits;
        TotalCodewords = totalCodewords;
    }

    public static QrVersion GetVersion(byte version)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(version, 40);

        return QrVersionsLookup[version - 1];
    }

    public byte Version { get; }
    public ImmutableArray<byte> AlignmentPatternCenters { get; }
    public ushort TotalCodewords { get; }
    public byte RemainderBits { get; }
    public byte ModulesPerSide => (byte)(17 + (byte)(4 * Version));

    public QrErrorCorrectionBlocks GetErrorCorrectionBlocks(ErrorCorrectionLevel errorCorrectionLevel)
        => QrVersionLookup.GetErrorCorrectionBlocks(Version, errorCorrectionLevel);

    public ushort GetDataCodewordsCapacity(ErrorCorrectionLevel errorCorrectionLevel)
    {
        //TODO: hardcode this on startup instead
        var total = 0;
        foreach (var item in GetErrorCorrectionBlocks(errorCorrectionLevel).Blocks)
        {
            total += item.DataCodewordsPerBlock * item.Count;
        }
        return (ushort)total;
    }

    public static bool operator ==(QrVersion? left, QrVersion? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(QrVersion? left, QrVersion? right) => !(left == right);

    /// <inheritdoc />
    public int CompareTo(QrVersion? other) => other is null ? 1 : Version.CompareTo(other.Version);

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);
    /// <inheritdoc />
    public bool Equals([NotNullWhen(true)] QrVersion? other) => other is not null && Version == other.Version;
    public override int GetHashCode() => Version.GetHashCode();

    private static readonly ImmutableArray<ImmutableArray<byte>> AlignmentPatternCentersLookup =
    [
        [], // version 1
        [6, 18], // version 2
        [6, 22], // version 3
        [6, 26], // version 4
        [6, 30], // version 5
        [6, 34], // version 6
        [6, 22, 38], // version 7
        [6, 24, 42], // version 8
        [6, 26, 46], // version 9
        [6, 28, 50], // version 10
        [6, 30, 54], // version 11
        [6, 32, 58], // version 12
        [6, 34, 62], // version 13
        [6, 26, 46, 66], // version 14
        [6, 26, 48, 70], // version 15
        [6, 26, 50, 74], // version 16
        [6, 30, 54, 78], // version 17
        [6, 30, 56, 82], // version 18
        [6, 30, 58, 86], // version 19
        [6, 34, 62, 90], // version 20
        [6, 28, 50, 72, 94], // version 21
        [6, 26, 50, 74, 98], // version 22
        [6, 30, 54, 78, 102], // version 23
        [6, 28, 54, 80, 106], // version 24
        [6, 32, 58, 84, 110], // version 25
        [6, 30, 58, 86, 114], // version 26
        [6, 34, 62, 90, 118], // version 27
        [6, 26, 50, 74, 98, 122], // version 28
        [6, 30, 54, 78, 102, 126], // version 29
        [6, 26, 52, 78, 104, 130], // version 30
        [6, 30, 56, 82, 108, 134], // version 31
        [6, 34, 60, 86, 112, 138], // version 32
        [6, 30, 58, 86, 114, 142], // version 33
        [6, 34, 62, 90, 118, 146], // version 34
        [6, 30, 54, 78, 102, 126, 150], // version 35
        [6, 24, 50, 76, 102, 128, 154], // version 36
        [6, 28, 54, 80, 106, 132, 158], // version 37
        [6, 32, 58, 84, 110, 136, 162], // version 38
        [6, 26, 54, 82, 110, 138, 166], // version 39
        [6, 30, 58, 86, 114, 142, 170], // version 40
    ];

    private static readonly ImmutableArray<QrVersion> QrVersionsLookup =
    [
        new(1, 26, 0),
        new(2, 44 , 7),
        new(3, 70, 7),
        new(4, 100, 7),
        new(5, 134, 7),
        new(6, 172, 7),
        new(7, 196, 0),
        new(8, 242, 0),
        new(9, 292, 0),
        new(10, 346, 0),
        new(11, 404, 0),
        new(12, 466, 0),
        new(13, 532, 0),
        new(14, 581, 3),
        new(15, 655, 3),
        new(16, 733, 3),
        new(17, 815, 3),
        new(18, 901, 3),
        new(19, 991, 3),
        new(20, 1085, 3),
        new(21, 1156, 4),
        new(22, 1258, 4),
        new(23, 1364, 4),
        new(24, 1474, 4),
        new(25, 1588, 4),
        new(26, 1706, 4),
        new(27, 1828, 4),
        new(28, 1921, 3),
        new(29, 2051, 3),
        new(30, 2185, 3),
        new(31, 2323, 3),
        new(32, 2465, 3),
        new(33, 2611, 3),
        new(34, 2761, 3),
        new(35, 2876, 0),
        new(36, 3034, 0),
        new(37, 3196, 0),
        new(38, 3362, 0),
        new(39, 3532, 0),
        new(40, 3706, 0),
    ];
}
