using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;
internal static class QrVersionLookup
{
    //TODO: Needs to account for ECI + others
    public static bool TryGetVersionForDataCapacity(int requestedCapacityDataCharacters, ModeIndicator mode, ErrorCorrectionLevel errorCorrectionLevel, [NotNullWhen(true)] out QrVersion? version)
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

        var index = BinarySearchUpperBoundForVersion(requestedCapacityDataCharacters, errorCorrectionLevel, mode);

        if (index is -1)
        {
            return false;
        }

        version = QrVersion.GetVersion((byte)(index + 1));
        return true;
    }

    //public static int BinarySearchUpperBound<T, T1Arg, T2Arg, TValue>(ReadOnlySpan<T> span, TValue value, T1Arg arg, T2Arg arg2, Func<T, T1Arg, T2Arg, TValue> selector) where TValue : IComparable<TValue>
    //{
    //    if (span.Length == 0)
    //    {
    //        return -1;
    //    }

    //    if (span.Length == 1)
    //    {
    //        return selector(span[0], arg, arg2).CompareTo(value) is 0 or 1 ? 0 : -1;
    //    }

    //    var low = 0;
    //    var high = span.Length - 1;
    //    while (low <= high)
    //    {
    //        var mid = low + ((high - low) >> 1);
    //        if (selector(span[mid], arg, arg2).CompareTo(value) == -1)
    //        {
    //            low = mid + 1;
    //        }
    //        else
    //        {
    //            high = mid - 1;
    //        }

    //    }

    //    return low >= span.Length ? -1 : low;
    //}

    internal static int BinarySearchUpperBoundForVersion(int requestedCapacityDataCharacters, ErrorCorrectionLevel level, ModeIndicator mode)
    {
        var low = 0;
        var high = QrVersion.MaxVersion - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var approxCapacity = CalculateApproximateCharacterCapacityWithinRange(QrVersion.GetVersion((byte)(mid + 1)), level, mode);
            if (approxCapacity.CompareTo(requestedCapacityDataCharacters) == -1)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }

        }

        return low >= QrVersion.MaxVersion ? -1 : low;
    }

    //TODO: Needs to account for ECI + others
    //TODO: Tests for this
    internal static bool VersionCanFitData(QrVersion version, ReadOnlySpan<char> data, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode)
    {
        const int ModeIndicatorBits = 4;
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var capacity = (version.GetDataCodewordsCapacity(errorCorrectionLevel) * 8) - ModeIndicatorBits - characterCount;
        return capacity >= mode.GetBitStreamLength(data);
    }

    internal static QrVersion.QrErrorCorrectionBlocks GetErrorCorrectionBlocks(int version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        if (version is < QrVersion.MinVersion or > QrVersion.MaxVersion)
        {
            ThrowOutOfRangeException(QrVersion.MinVersion, QrVersion.MaxVersion, "Version");
        }
        EnsureValidErrorCorrectionLevel(errorCorrectionLevel);

        return GetErrorCorrectionBlocks(errorCorrectionLevel)[version - 1];
    }

    private static void EnsureValidErrorCorrectionLevel(ErrorCorrectionLevel errorCorrectionLevel)
    {
        if ((int)errorCorrectionLevel is < 0 or > 3)
        {
            ThrowOutOfRangeException(0, 3, "Error correction level");
        }
    }

    private static ImmutableArray<QrVersion.QrErrorCorrectionBlocks> GetErrorCorrectionBlocks(ErrorCorrectionLevel level)
    {
        return level switch
        {
            ErrorCorrectionLevel.L => QrVersionsLookupL,
            ErrorCorrectionLevel.M => QrVersionsLookupM,
            ErrorCorrectionLevel.Q => QrVersionsLookupQ,
            ErrorCorrectionLevel.H => QrVersionsLookupH,
            _ => throw new UnreachableException()
        };
    }

    private static int CalculateApproximateCharacterCapacityWithinRange(QrVersion version, ErrorCorrectionLevel errorCorrectionLevel, ModeIndicator mode)
    {
#pragma warning disable IDE0072 // Add missing cases
        const int ModeIndicatorBits = 4;
        var characterCount = CharacterCount.GetCharacterCountBitCount(version, mode);
        var capacity = (version.GetDataCodewordsCapacity(errorCorrectionLevel) * 8) - ModeIndicatorBits - characterCount;

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

    [DoesNotReturn]
    private static void ThrowOutOfRangeException(int min, int max, string description)
        => throw new ArgumentOutOfRangeException($"{description} must be between {min} and {max}.", innerException: null);

    // individual error blocks must be in ascending order of data codewords per block
    // as this is key for the interleaving process, where smaller data blocks are intrerleaved first

    private static readonly ImmutableArray<QrVersion.QrErrorCorrectionBlocks> QrVersionsLookupL =
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

    private static readonly ImmutableArray<QrVersion.QrErrorCorrectionBlocks> QrVersionsLookupM =
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

    private static readonly ImmutableArray<QrVersion.QrErrorCorrectionBlocks> QrVersionsLookupQ =
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

    private static readonly ImmutableArray<QrVersion.QrErrorCorrectionBlocks> QrVersionsLookupH =
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
