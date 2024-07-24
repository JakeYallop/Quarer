using System;

namespace Quarer.Tests;
public sealed class QrVersionLookupTests
{
    [Theory]
    [InlineData(254, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 5)]
    [InlineData(255, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 5)]
    [InlineData(256, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 6)]
    [InlineData(255, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 6)]
    [InlineData(1270, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 38)]
    [InlineData(370, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 18)]
    [InlineData(154, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 7)]
    [InlineData(282, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 14)]

    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Byte, ErrorCorrectionLevel.H, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 1)]
    [InlineData(1, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 1)]

    [InlineData(7089, ModeIndicator.Numeric, ErrorCorrectionLevel.L, 40)]
    [InlineData(5596, ModeIndicator.Numeric, ErrorCorrectionLevel.M, 40)]
    [InlineData(3993, ModeIndicator.Numeric, ErrorCorrectionLevel.Q, 40)]
    [InlineData(3057, ModeIndicator.Numeric, ErrorCorrectionLevel.H, 40)]
    [InlineData(4296, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.L, 40)]
    [InlineData(3391, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.M, 40)]
    [InlineData(2420, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.Q, 40)]
    [InlineData(1852, ModeIndicator.Alphanumeric, ErrorCorrectionLevel.H, 40)]
    [InlineData(2953, ModeIndicator.Byte, ErrorCorrectionLevel.L, 40)]
    [InlineData(2331, ModeIndicator.Byte, ErrorCorrectionLevel.M, 40)]
    [InlineData(1663, ModeIndicator.Byte, ErrorCorrectionLevel.Q, 40)]
    [InlineData(1273, ModeIndicator.Byte, ErrorCorrectionLevel.H, 40)]
    [InlineData(1817, ModeIndicator.Kanji, ErrorCorrectionLevel.L, 40)]
    [InlineData(1435, ModeIndicator.Kanji, ErrorCorrectionLevel.M, 40)]
    [InlineData(1024, ModeIndicator.Kanji, ErrorCorrectionLevel.Q, 40)]
    [InlineData(784, ModeIndicator.Kanji, ErrorCorrectionLevel.H, 40)]
    public void TryGetVersionForCapacity_WithValidCapacity_ReturnsTrue(int characters, ModeIndicator mode, ErrorCorrectionLevel errorCorrectionLevel, int expectedVersion)
    {
        Assert.True(QrVersionLookup.TryGetVersionForDataCapacity(characters, mode, errorCorrectionLevel, out var version));
        Assert.Equal(expectedVersion, version.Version);
        Assert.Equal(errorCorrectionLevel, version.ErrorCorrectionLevel);
    }

    private static readonly int[][][] ModeErrorLevelVersionCapacityLookup =
    [
        // Numeric
        [
            // L
            [41, 77, 127, 187, 255, 322, 370, 461, 552, 652, 772, 883, 1022, 1101, 1250, 1408, 1548, 1725, 1903, 2061, 2232, 2409, 2620, 2812, 3057, 3283, 3517, 3669, 3909, 4158, 4417, 4686, 4965, 5253, 5529, 5836, 6153, 6479, 6743, 7089],
            // M
            [34, 63, 101, 149, 202, 255, 293, 365, 432, 513, 604, 691, 796, 871, 991, 1082, 1212, 1346, 1500, 1600, 1708, 1872, 2059, 2188, 2395, 2544, 2701, 2857, 3035, 3289, 3486, 3693, 3909, 4134, 4343, 4588, 4775, 5039, 5313, 5596],
            // Q
            [27, 48, 77, 111, 144, 178, 207, 259, 312, 364, 427, 489, 580, 621, 703, 775, 876, 948, 1063, 1159, 1224, 1358, 1468, 1588, 1718, 1804, 1933, 2085, 2181, 2358, 2473, 2670, 2805, 2949, 3081, 3244, 3417, 3599, 3791, 3993],
            // H
            [17, 34, 58, 82, 106, 139, 154, 202, 235, 288, 331, 374, 427, 468, 530, 602, 674, 746, 813, 919, 969, 1056, 1108, 1228, 1286, 1425, 1501, 1581, 1677, 1782, 1897, 2022, 2157, 2301, 2361, 2524, 2625, 2735, 2927, 3057]
        ],
        // Alphanumeric
        [
            // L
            [25, 47, 77, 114, 154, 195, 224, 279, 335, 395, 468, 535, 619, 667, 758, 854, 938, 1046, 1153, 1249, 1352, 1460, 1588, 1704, 1853, 1990, 2132, 2223, 2369, 2520, 2677, 2840, 3009, 3183, 3351, 3537, 3729, 3927, 4087, 4296],
            // M
            [20, 38, 61, 90, 122, 154, 178, 221, 262, 311, 366, 419, 483, 528, 600, 656, 734, 816, 909, 970, 1035, 1134, 1248, 1326, 1451, 1542, 1637, 1732, 1839, 1994, 2113, 2238, 2369, 2506, 2632, 2780, 2894, 3054, 3220, 3391],
            // Q
            [16, 29, 47, 67, 87, 108, 125, 157, 189, 221, 259, 296, 352, 376, 426, 470, 531, 574, 644, 702, 742, 823, 890, 963, 1041, 1094, 1172, 1263, 1322, 1429, 1499, 1618, 1700, 1787, 1867, 1966, 2071, 2181, 2298, 2420],
            // H
            [10, 20, 35, 50, 64, 84, 93, 122, 143, 174, 200, 227, 259, 283, 321, 365, 408, 452, 493, 557, 587, 640, 672, 744, 779, 864, 910, 958, 1016, 1080, 1150, 1226, 1307, 1394, 1431, 1530, 1591, 1658, 1774, 1852]
        ],
        // Byte
        [
            // L
            [17, 32, 53, 78, 106, 134, 154, 192, 230, 271, 321, 367, 425, 458, 520, 586, 644, 718, 792, 858, 929, 1003, 1091, 1171, 1273, 1367, 1465, 1528, 1628, 1732, 1840, 1952, 2068, 2188, 2303, 2431, 2563, 2699, 2809, 2953],
            // M
            [14, 26, 42, 62, 84, 106, 122, 152, 180, 213, 251, 287, 331, 362, 412, 450, 504, 560, 624, 666, 711, 779, 857, 911, 997, 1059, 1125, 1190, 1264, 1370, 1452, 1538, 1628, 1722, 1809, 1911, 1989, 2099, 2213, 2331],
            // Q
            [11, 20, 32, 46, 60, 74, 86, 108, 130, 151, 177, 203, 241, 258, 292, 322, 364, 394, 442, 482, 509, 565, 611, 661, 715, 751, 805, 868, 908, 982, 1030, 1112, 1168, 1228, 1283, 1351, 1423, 1499, 1579, 1663],
            // H
            [7, 14, 24, 34, 44, 58, 64, 84, 98, 119, 137, 155, 177, 194, 220, 250, 280, 310, 338, 382, 403, 439, 461, 511, 535, 593, 625, 658, 698, 742, 790, 842, 898, 958, 983, 1051, 1093, 1139, 1219, 1273]
        ],
        // Kanji
        [
            // L
            [10, 20, 32, 48, 65, 82, 95, 118, 141, 167, 198, 226, 262, 282, 320, 361, 397, 442, 488, 528, 572, 618, 672, 721, 784, 842, 902, 940, 1002, 1066, 1132, 1201, 1273, 1347, 1417, 1496, 1577, 1661, 1729, 1817],
            // M
            [8, 16, 26, 38, 52, 65, 75, 93, 111, 131, 155, 177, 204, 223, 254, 277, 310, 345, 384, 410, 438, 480, 528, 561, 614, 652, 692, 732, 778, 843, 894, 947, 1002, 1060, 1113, 1176, 1224, 1292, 1362, 1435],
            // Q
            [7, 12, 20, 28, 37, 45, 53, 66, 80, 93, 109, 125, 149, 159, 180, 198, 224, 243, 272, 297, 314, 348, 376, 407, 440, 462, 496, 534, 559, 604, 634, 684, 719, 756, 790, 832, 876, 923, 972, 1024],
            // H
            [4, 8, 15, 21, 27, 36, 39, 52, 60, 74, 85, 96, 109, 120, 136, 154, 173, 191, 208, 235, 248, 270, 284, 315, 330, 365, 385, 405, 430, 457, 486, 518, 553, 590, 605, 647, 673, 701, 750, 784]
        ]
    ];

    [Fact]
    public void TryGetVersionForCapacity_WithValidCapacity_AllCombinations()
    {
        for (var mode = 0; mode < ModeErrorLevelVersionCapacityLookup.Length; mode++)
        {
            for (var errorLevel = 0; errorLevel < ModeErrorLevelVersionCapacityLookup[mode].Length; errorLevel++)
            {
                for (var version = 0; version < ModeErrorLevelVersionCapacityLookup[mode][errorLevel].Length; version++)
                {
                    var inputDataCharacters = ModeErrorLevelVersionCapacityLookup[mode][errorLevel][version];
                    var expectedMode = MapMode(mode);
                    var expectedErrorCorrectionLevel = (ErrorCorrectionLevel)(errorLevel + 1);
                    var expectedVersion = version + 1;
                    AssertVersion(inputDataCharacters, expectedMode, expectedErrorCorrectionLevel, expectedVersion);
                    AssertVersion(inputDataCharacters - 1, expectedMode, expectedErrorCorrectionLevel, expectedVersion);
                    if (expectedVersion != 40)
                    {
                        AssertVersion(inputDataCharacters + 1, expectedMode, expectedErrorCorrectionLevel, expectedVersion + 1);
                    }
                }
            }
        }

        static void AssertVersion(int inputDataCharacters, ModeIndicator expectedMode, ErrorCorrectionLevel expectedErrorCorrectionLevel, int expectedVersion)
        {
            var assertionMessage = $"Failed for inputDataCharacters: {inputDataCharacters}, expectedMode: {expectedMode}, expectedErrorCorrectionLevel: {expectedErrorCorrectionLevel}, expectedVersion: {expectedVersion}";
            Assert.True(QrVersionLookup.TryGetVersionForDataCapacity(inputDataCharacters, expectedMode, expectedErrorCorrectionLevel, out var qrVersion), assertionMessage);
            Assert.Equal(expectedVersion, qrVersion.Version);
            Assert.Equal(expectedErrorCorrectionLevel, qrVersion.ErrorCorrectionLevel);
        }

        static ModeIndicator MapMode(int index)
        {
            return index switch
            {
                0 => ModeIndicator.Numeric,
                1 => ModeIndicator.Alphanumeric,
                2 => ModeIndicator.Byte,
                3 => ModeIndicator.Kanji,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
    }

    [Fact]
    public void TryGetVersionForCapacity_DataTooLarge_ReturnsFalse()
        => Assert.False(QrVersionLookup.TryGetVersionForDataCapacity(7089 + 1, ModeIndicator.Numeric, ErrorCorrectionLevel.L, out _));

    [Theory]
    [InlineData((int)ErrorCorrectionLevel.L - 1)]
    [InlineData((int)ErrorCorrectionLevel.H + 1)]
    public void TryGetVersionForCapacity_InvalidErrorCorrectionLevel_ThrowsArgumentOutOfRangeException(int errorCorrectionLevel)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrVersionLookup.TryGetVersionForDataCapacity(1, ModeIndicator.Numeric, (ErrorCorrectionLevel)errorCorrectionLevel, out _));

    [Theory]
    [InlineData(ModeIndicator.Terminator)]
    [InlineData(ModeIndicator.Eci)]
    [InlineData(ModeIndicator.Fnc1FirstPosition)]
    [InlineData(ModeIndicator.Fnc1SecondPosition)]
    [InlineData(ModeIndicator.StructuredAppend)]
    public void TryGetVersionForCapacity_InvalidMode_ThrowsNotSupportedException(ModeIndicator mode)
        => Assert.Throws<NotSupportedException>(() => QrVersionLookup.TryGetVersionForDataCapacity(1, mode, ErrorCorrectionLevel.L, out _));

    [Theory]
    [InlineData(1, ErrorCorrectionLevel.L)]
    [InlineData(7, ErrorCorrectionLevel.M)]
    [InlineData(23, ErrorCorrectionLevel.Q)]
    [InlineData(40, ErrorCorrectionLevel.H)]
    public void GetVersion_ReturnsVersion(int version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var qrVersion = QrVersionLookup.GetVersion(version, errorCorrectionLevel);
        Assert.Equal(version, qrVersion.Version);
        Assert.Equal(errorCorrectionLevel, qrVersion.ErrorCorrectionLevel);
    }

    [Theory]
    [InlineData(ErrorCorrectionLevel.L - 1)]
    [InlineData(ErrorCorrectionLevel.H + 1)]
    public void GetVersion_InvalidErrorCorrectionLevel_ThrowsNotSupportedException(ErrorCorrectionLevel errorCorrectionLevel)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrVersionLookup.GetVersion(1, errorCorrectionLevel));

    [Theory]
    [InlineData(QrVersion.MinVersion - 1, ErrorCorrectionLevel.L)]
    [InlineData(QrVersion.MinVersion - 1, ErrorCorrectionLevel.H)]
    [InlineData(QrVersion.MaxVersion + 1, ErrorCorrectionLevel.L)]
    [InlineData(QrVersion.MaxVersion + 1, ErrorCorrectionLevel.Q)]
    public void GetVersion_InvalidVersion_ThrowsNotSupportedException(int version, ErrorCorrectionLevel errorCorrectionLevel)
        => Assert.Throws<ArgumentOutOfRangeException>(() => QrVersionLookup.GetVersion(version, errorCorrectionLevel));
}
