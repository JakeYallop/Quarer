using System.Diagnostics;

namespace Quarer;

internal static class AlphanumericEncoder
{
    public const string Characters = "0123456789AABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

    public static ReadOnlySpan<short> Character2Map =>
    [
        36 ,	// Space - 32
        -1 ,	// 33
        -1 ,	// 34
        -1 ,	// 35
        37 ,	// $ - 36
        38 ,	// % - 37
        -1 ,	// 38
        -1 ,	// 39
        -1 ,	// 40
        -1 ,	// 41
        39 ,	// * - 42
        40 ,	// + - 43
        -1 ,    // 44
        41 ,	// - - 45
        42 ,	// . - 46
        43 ,	// / - 47
        0  ,	// 0 - 48
        1  ,	// 1 - 49
        2  ,	// 2 - 50
        3  ,	// 3 - 51
        4  ,	// 4 - 52
        5  ,	// 5 - 53
        6  ,	// 6 - 54
        7  ,	// 7 - 55
        8  ,	// 8 - 56
        9  ,	// 9 - 57
        44 ,	// : - 58
        -1 ,	// 59
        -1 ,	// 60
        -1 ,	// 61
        -1 ,	// 62
        -1 ,	// 63
        -1 ,	// 64
        10 ,	// A - 65
        11 ,	// B
        12 ,	// C
        13 ,	// D
        14 ,	// E
        15 ,	// F
        16 ,	// G
        17 ,	// H
        18 ,	// I
        19 ,	// J
        20 ,	// K
        21 ,	// L
        22 ,	// M
        23 ,	// N
        24 ,	// O
        25 ,	// P
        26 ,	// Q
        27 ,	// R
        28 ,	// S
        29 ,	// T
        30 ,	// U
        31 ,	// V
        32 ,	// W
        33 ,	// X
        34 ,	// Y
        35 ,	// Z - 90
    ];

    public static ReadOnlySpan<short> Character1Map =>
    [
        1620,   // Space - 32
        -1  ,   // 33
        -1  ,   // 34
        -1  ,   // 35
        1665,   // $ - 36
        1710,   // % - 37
        -1  ,   // 38
        -1  ,   // 39
        -1  ,   // 40
        -1  ,   // 41
        1755,   // * - 42
        1800,   // + - 43
        -1  ,   // 44
        1845,   // - - 45
        1890,   // . - 46
        1935,   // / - 47
        0   ,   // 0 - 48
        45  ,   // 1 - 49
        90  ,   // 2 - 50
        135 ,   // 3 - 51
        180 ,   // 4 - 52
        225 ,   // 5 - 53
        270 ,   // 6 - 54
        315 ,   // 7 - 55
        360 ,   // 8 - 56
        405 ,   // 9 - 57
        1980,   // : - 58
        -1  ,	// 59
        -1  ,	// 60
        -1  ,	// 61
        -1  ,	// 62
        -1  ,	// 63
        -1  ,	// 64
        450 ,   // A - 65
        495 ,   // B
        540 ,   // C
        585 ,   // D
        630 ,   // E
        675 ,   // F
        720 ,   // G
        765 ,   // H
        810 ,   // I
        855 ,   // J
        900 ,   // K
        945 ,   // L
        990 ,   // M
        1035,   // N
        1080,   // O
        1125,   // P
        1170,   // Q
        1215,   // R
        1260,   // S
        1305,   // T
        1350,   // U
        1395,   // V
        1440,   // W
        1485,   // X
        1530,   // Y
        1575,   // Z - 90
    ];

    public static void Encode(BitWriter writer, scoped ReadOnlySpan<char> data)
    {
        var position = 0;
        for (; position + 2 <= data.Length; position += 2)
        {
            var pair = data[position..(position + 2)];
            var i1 = pair[0] - ' ';
            var i2 = pair[1] - ' ';
            Debug.Assert(Character1Map[i1] != -1);
            Debug.Assert(Character2Map[i2] != -1);
            writer.WriteBitsBigEndian(Character1Map[i1] + Character2Map[i2], 11);
        }

        if (data.Length != position)
        {
            Debug.Assert(data.Length == position + 1, "Expected only 1 character remaining after encoding all other pairs.");
            var remainingCharacter = data[position] - ' ';

            Debug.Assert(Character2Map[remainingCharacter] != -1);
            writer.WriteBitsBigEndian(Character2Map[remainingCharacter], 6);
        }
    }

    /// <summary>
    /// Gets the length of an alphanumeric bitstream created from the provided data, excluding the mode and character count indicator bits.
    /// </summary>
    /// <returns></returns>
    public static int GetBitStreamLength(scoped ReadOnlySpan<char> alphanumericData)
        => (11 * (alphanumericData.Length / 2)) + (6 * (alphanumericData.Length % 2));
}
