using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

/// <summary>
/// Represents a specific version of a QR Code with all associated information including size, alignment patterns and error correction blocks.
/// </summary>
public sealed partial class QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    internal const byte MinVersion = 1;
    internal const byte MaxVersion = 40;
    internal const int MaxModulesPerSide = 17 + (4 * MaxVersion);

    internal QrVersion(byte version, ushort totalCodewords, byte remainderBits)
    {
        Version = version;
        AlignmentPatternCenters = AlignmentPatternCentersLookup[version - 1];
        RemainderBits = remainderBits;
        TotalCodewords = totalCodewords;
        var modulesPerSide = (byte)(17 + (byte)(4 * version));
        Width = modulesPerSide;
        Height = modulesPerSide;
    }

    /// <summary>
    /// Get version information for a QR Code version.
    /// </summary>
    /// <param name="version">The version to use.</param>
    public static QrVersion GetVersion(byte version)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(version, 40);

        return QrVersionsLookup[version - 1];
    }

    /// <summary>
    /// The version of this QR Code.
    /// </summary>
    public byte Version { get; }
    /// <summary>
    /// The placement of alignment patterns for this QR Code version.
    /// </summary>
    public ImmutableArray<byte> AlignmentPatternCenters { get; }
    /// <summary>
    /// The total number of codewords (data + ecc) in this QR Code version.
    /// </summary>
    public ushort TotalCodewords { get; }
    /// <summary>
    /// The number of remainder bits left over after filling the capacity of the symbol.
    /// </summary>
    public byte RemainderBits { get; }
    /// <summary>
    /// The width of this QR Code version.
    /// </summary>
    public byte Width { get; }
    /// <summary>
    /// The height of this QR Code version.
    /// </summary>
    public byte Height { get; }

    /// <summary>
    /// Get the error correction blocks for this QR Code version for the given <see cref="ErrorCorrectionLevel" />.
    /// </summary>
    public QrErrorCorrectionBlocks GetErrorCorrectionBlocks(ErrorCorrectionLevel errorCorrectionLevel)
        => GetErrorCorrectionBlocks(Version, errorCorrectionLevel);

    /// <summary>
    /// Get the number of data codewords present in this QR Code version, at the given <see cref="ErrorCorrectionLevel"/>.
    /// </summary>
    /// <param name="errorCorrectionLevel"></param>
    public int GetDataCodewordsCapacity(ErrorCorrectionLevel errorCorrectionLevel)
    {
        var blocks = GetErrorCorrectionBlocks(errorCorrectionLevel);
        return blocks.DataCodewordsCount;
    }

    /// <summary>
    /// Given the number of characters (for a given mode), along with an error correction level and optionally an ECI encoding, find the smallest version that can fit the data.
    /// </summary>
    /// <param name="requestedCapacityDataCharacters">The number of characters to try to store in the QR Code. These are mode specific - byte mode is 1 per byte, kanji is 1 per 2 bytes etc.</param>
    /// <param name="errorCorrectionLevel">The error correction level to use.</param>
    /// <param name="mode">The mode we are encoding in.</param>
    /// <param name="eciCode">The ECI code to use, may be <see cref="EciCode.Empty"/>.</param>
    /// <param name="version">The QR Code version that can fit the <paramref name="requestedCapacityDataCharacters"/>, null if the data is too large.</param>
    public static bool TryGetVersionForDataCapacity(int requestedCapacityDataCharacters, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode, EciCode eciCode, [NotNullWhen(true)] out QrVersion? version)
    {
        version = default!;
        if ((int)errorCorrectionLevel is < 0 or > 3)
        {
            return false;
        }

        if (mode is not (ModeIndicator.Alphanumeric or ModeIndicator.Numeric or ModeIndicator.Kanji or ModeIndicator.Byte))
        {
            return false;
        }

        var index = BinarySearchUpperBoundForVersionWithCapacity(requestedCapacityDataCharacters, errorCorrectionLevel, mode, eciCode);

        if (index is -1)
        {
            return false;
        }

        version = GetVersion((byte)(index + 1));
        return true;
    }

    /// <summary>
    /// Given a QR version and some data, returns true if the data can fit within the QR Code.
    /// </summary>
    public static bool VersionCanFitData(QrVersion version, ReadOnlySpan<byte> data, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode, EciCode eciCode)
    {
        const int ModeIndicatorBits = 4;
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var capacity = (version.GetDataCodewordsCapacity(errorCorrectionLevel) * 8) - ModeIndicatorBits - characterCount - eciCode.GetDataSegmentLength();

        return capacity >= mode.GetBitStreamLength(data);
    }

    /// <summary>
    /// Returns a value representing whether two <see cref="QrVersion"/> instances are equal.
    /// </summary>
    public static bool operator ==(QrVersion? left, QrVersion? right) => left is null ? right is null : left.Equals(right);
    /// <summary>
    /// Returns a value representing whether two <see cref="QrVersion"/> instances are not equal.
    /// </summary>
    public static bool operator !=(QrVersion? left, QrVersion? right) => !(left == right);

    /// <inheritdoc />
    public int CompareTo(QrVersion? other) => other is null ? 1 : Version.CompareTo(other.Version);

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);
    /// <inheritdoc />
    public bool Equals([NotNullWhen(true)] QrVersion? other) => other is not null && Version == other.Version;
    /// <inheritdoc />
    public override int GetHashCode() => Version.GetHashCode();

    private static int BinarySearchUpperBoundForVersionWithCapacity(int requestedCapacityDataCharacters, ErrorCorrectionLevel level, ModeIndicator mode, EciCode eciCode)
    {
        var low = 0;
        var high = MaxVersion - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var approxCapacity = CalculateApproximateCharacterCapacityWithinRange(GetVersion((byte)(mid + 1)), level, mode, eciCode);
            if (approxCapacity.CompareTo(requestedCapacityDataCharacters) == -1)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }

        }

        return low >= MaxVersion ? -1 : low;
    }

    private static int CalculateApproximateCharacterCapacityWithinRange(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode, EciCode eciCode)
    {
#pragma warning disable IDE0072 // Add missing cases
        const int ModeIndicatorBits = 4;
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var capacity = (version.GetDataCodewordsCapacity(errorCorrectionLevel) * 8) - ModeIndicatorBits - characterCount - eciCode.GetDataSegmentLength();

        Debug.Assert(capacity > 0);
        return mode switch
        {
            // 10 bits per 3 characters, 3 / 10, integer division allows us to ignore the remainder cases (4 or 7 bits left over)
            ModeIndicator.Numeric => (int)(capacity * 0.3),
            // 11 bits per 2 characters, 2.0 / 11.0, integer division allows us to ignore the remainder case (1 character in 6 bits)
            ModeIndicator.Alphanumeric => (int)(capacity * 0.18181818181818182d),
            // 8 bits per character
            ModeIndicator.Byte => capacity >> 3,
            // 13 bits per character, 1.0 / 13.0,
            ModeIndicator.Kanji => (int)(capacity * 0.07692307692307693),
            _ => throw new UnreachableException()
        };
#pragma warning restore IDE0072 // Add missing cases
    }

    private static QrErrorCorrectionBlocks GetErrorCorrectionBlocks(int version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        if (version is < MinVersion or > MaxVersion)
        {
            ThrowOutOfRangeException(MinVersion, MaxVersion, "Version");
        }

        if ((int)errorCorrectionLevel is < 0 or > 3)
        {
            ThrowOutOfRangeException(0, 3, "Error correction level");
        }

        return GetErrorCorrectionBlocks(errorCorrectionLevel, version);
    }

    private static QrErrorCorrectionBlocks GetErrorCorrectionBlocks(ErrorCorrectionLevel level, int version)
    {
        return level switch
        {
            ErrorCorrectionLevel.L => QrVersionsLookupL[version - 1],
            ErrorCorrectionLevel.M => QrVersionsLookupM[version - 1],
            ErrorCorrectionLevel.Q => QrVersionsLookupQ[version - 1],
            ErrorCorrectionLevel.H => QrVersionsLookupH[version - 1],
            _ => throw new UnreachableException()
        };
    }

    [DoesNotReturn]
    private static void ThrowOutOfRangeException(int min, int max, string description)
       => throw new ArgumentOutOfRangeException($"{description} must be between {min} and {max}.", innerException: null);

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

    // individual error blocks must be in ascending order of data codewords per block
    // as this is key for the interleaving process, where smaller data blocks are interleaved first

    private static readonly ImmutableArray<QrErrorCorrectionBlocks> QrVersionsLookupL =
    [
        new(7, [new(1, 19)]),
        new(10, [new(1, 34)]),
        new(15, [new(1, 55)]),
        new(20, [new(1, 80)]),
        new(26, [new(1, 108)]),
        new(18, [new(2, 68)]),
        new(20, [new(2, 78)]),
        new(24, [new(2, 97)]),
        new(30, [new(2, 116)]),
        new(18, [new(2, 68), new(2, 69)]),
        new(20, [new(4, 81)]),
        new(24, [new(2, 92), new(2, 93)]),
        new(26, [new(4, 107)]),
        new(30, [new(3, 115), new(1, 116)]),
        new(22, [new(5, 87), new(1, 88)]),
        new(24, [new(5, 98), new(1, 99)]),
        new(28, [new(1, 107), new(5, 108)]),
        new(30, [new(5, 120), new(1, 121)]),
        new(28, [new(3, 113), new(4, 114)]),
        new(28, [new(3, 107), new(5, 108)]),
        new(28, [new(4, 116), new(4, 117)]),
        new(28, [new(2, 111), new(7, 112)]),
        new(30, [new(4, 121), new(5, 122)]),
        new(30, [new(6, 117), new(4, 118)]),
        new(26, [new(8, 106), new(4, 107)]),
        new(28, [new(10, 114), new(2, 115)]),
        new(30, [new(8, 122), new(4, 123)]),
        new(30, [new(3, 117), new(10, 118)]),
        new(30, [new(7, 116), new(7, 117)]),
        new(30, [new(5, 115), new(10, 116)]),
        new(30, [new(13, 115), new(3, 116)]),
        new(30, [new(17, 115)]),
        new(30, [new(17, 115), new(1, 116)]),
        new(30, [new(13, 115), new(6, 116)]),
        new(30, [new(12, 121), new(7, 122)]),
        new(30, [new(6, 121), new(14, 122)]),
        new(30, [new(17, 122), new(4, 123)]),
        new(30, [new(4, 122), new(18, 123)]),
        new(30, [new(20, 117), new(4, 118)]),
        new(30, [new(19, 118), new(6, 119)]),
    ];

    private static readonly ImmutableArray<QrErrorCorrectionBlocks> QrVersionsLookupM =
    [
        new(10, [new(1, 16)]),
        new(16, [new(1, 28)]),
        new(26, [new(1, 44)]),
        new(18, [new(2, 32)]),
        new(24, [new(2, 43)]),
        new(16, [new(4, 27)]),
        new(18, [new(4, 31)]),
        new(22, [new(2, 38), new(2, 39)]),
        new(22, [new(3, 36), new(2, 37)]),
        new(26, [new(4, 43), new(1, 44)]),
        new(30, [new(1, 50), new(4, 51)]),
        new(22, [new(6, 36), new(2, 37)]),
        new(22, [new(8, 37), new(1, 38)]),
        new(24, [new(4, 40), new(5, 41)]),
        new(24, [new(5, 41), new(5, 42)]),
        new(28, [new(7, 45), new(3, 46)]),
        new(28, [new(10, 46), new(1, 47)]),
        new(26, [new(9, 43), new(4, 44)]),
        new(26, [new(3, 44), new(11, 45)]),
        new(26, [new(3, 41), new(13, 42)]),
        new(26, [new(17, 42)]),
        new(28, [new(17, 46)]),
        new(28, [new(4, 47), new(14, 48)]),
        new(28, [new(6, 45), new(14, 46)]),
        new(28, [new(8, 47), new(13, 48)]),
        new(28, [new(19, 46), new(4, 47)]),
        new(28, [new(22, 45), new(3, 46)]),
        new(28, [new(3, 45), new(23, 46)]),
        new(28, [new(21, 45), new(7, 46)]),
        new(28, [new(19, 47), new(10, 48)]),
        new(28, [new(2, 46), new(29, 47)]),
        new(28, [new(10, 46), new(23, 47)]),
        new(28, [new(14, 46), new(21, 47)]),
        new(28, [new(14, 46), new(23, 47)]),
        new(28, [new(12, 47), new(26, 48)]),
        new(28, [new(6, 47), new(34, 48)]),
        new(28, [new(29, 46), new(14, 47)]),
        new(28, [new(13, 46), new(32, 47)]),
        new(28, [new(40, 47), new(7, 48)]),
        new(28, [new(18, 47), new(31, 48)]),
    ];

    private static readonly ImmutableArray<QrErrorCorrectionBlocks> QrVersionsLookupQ =
    [
        new(13, [new(1, 13)]),
        new(22, [new(1, 22)]),
        new(18, [new(2, 17)]),
        new(26, [new(2, 24)]),
        new(18, [new(2, 15), new(2, 16)]),
        new(24, [new(4, 19)]),
        new(18, [new(2, 14), new(4, 15)]),
        new(22, [new(4, 18), new(2, 19)]),
        new(20, [new(4, 16), new(4, 17)]),
        new(24, [new(6, 19), new(2, 20)]),
        new(28, [new(4, 22), new(4, 23)]),
        new(26, [new(4, 20), new(6, 21)]),
        new(24, [new(8, 20), new(4, 21)]),
        new(20, [new(11, 16), new(5, 17)]),
        new(30, [new(5, 24), new(7, 25)]),
        new(24, [new(15, 19), new(2, 20)]),
        new(28, [new(1, 22), new(15, 23)]),
        new(28, [new(17, 22), new(1, 23)]),
        new(26, [new(17, 21), new(4, 22)]),
        new(30, [new(15, 24), new(5, 25)]),
        new(28, [new(17, 22), new(6, 23)]),
        new(30, [new(7, 24), new(16, 25)]),
        new(30, [new(11, 24), new(14, 25)]),
        new(30, [new(11, 24), new(16, 25)]),
        new(30, [new(7, 24), new(22, 25)]),
        new(28, [new(28, 22), new(6, 23)]),
        new(30, [new(8, 23), new(26, 24)]),
        new(30, [new(4, 24), new(31, 25)]),
        new(30, [new(1, 23), new(37, 24)]),
        new(30, [new(15, 24), new(25, 25)]),
        new(30, [new(42, 24), new(1, 25)]),
        new(30, [new(10, 24), new(35, 25)]),
        new(30, [new(29, 24), new(19, 25)]),
        new(30, [new(44, 24), new(7, 25)]),
        new(30, [new(39, 24), new(14, 25)]),
        new(30, [new(46, 24), new(10, 25)]),
        new(30, [new(49, 24), new(10, 25)]),
        new(30, [new(48, 24), new(14, 25)]),
        new(30, [new(43, 24), new(22, 25)]),
        new(30, [new(34, 24), new(34, 25)]),
    ];

    private static readonly ImmutableArray<QrErrorCorrectionBlocks> QrVersionsLookupH =
    [
        new(17, [new(1, 9)]),
        new(28, [new(1, 16)]),
        new(22, [new(2, 13)]),
        new(16, [new(4, 9)]),
        new(22, [new(2, 11), new(2, 12)]),
        new(28, [new(4, 15)]),
        new(26, [new(4, 13), new(1, 14)]),
        new(26, [new(4, 14), new(2, 15)]),
        new(24, [new(4, 12), new(4, 13)]),
        new(28, [new(6, 15), new(2, 16)]),
        new(24, [new(3, 12), new(8, 13)]),
        new(28, [new(7, 14), new(4, 15)]),
        new(22, [new(12, 11), new(4, 12)]),
        new(24, [new(11, 12), new(5, 13)]),
        new(24, [new(11, 12), new(7, 13)]),
        new(30, [new(3, 15), new(13, 16)]),
        new(28, [new(2, 14), new(17, 15)]),
        new(28, [new(2, 14), new(19, 15)]),
        new(26, [new(9, 13), new(16, 14)]),
        new(28, [new(15, 15), new(10, 16)]),
        new(30, [new(19, 16), new(6, 17)]),
        new(24, [new(34, 13)]),
        new(30, [new(16, 15), new(14, 16)]),
        new(30, [new(30, 16), new(2, 17)]),
        new(30, [new(22, 15), new(13, 16)]),
        new(30, [new(33, 16), new(4, 17)]),
        new(30, [new(12, 15), new(28, 16)]),
        new(30, [new(11, 15), new(31, 16)]),
        new(30, [new(19, 15), new(26, 16)]),
        new(30, [new(23, 15), new(25, 16)]),
        new(30, [new(23, 15), new(28, 16)]),
        new(30, [new(19, 15), new(35, 16)]),
        new(30, [new(11, 15), new(46, 16)]),
        new(30, [new(59, 16), new(1, 17)]),
        new(30, [new(22, 15), new(41, 16)]),
        new(30, [new(2, 15), new(64, 16)]),
        new(30, [new(24, 15), new(46, 16)]),
        new(30, [new(42, 15), new(32, 16)]),
        new(30, [new(10, 15), new(67, 16)]),
        new(30, [new(20, 15), new(61, 16)]),
    ];
}
