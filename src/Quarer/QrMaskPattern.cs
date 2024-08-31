public enum QrMaskPattern : byte
{
    //From ISO/IEC 18004:2015 Third edition Table 10. In the specification, i = y and j = x

    /// <summary>
    /// Mask pattern 0: <c>(y + x) % 2 == 0</c>.
    /// </summary>
    PatternZero_Checkerboard = 0b000,
    /// <summary>
    /// Mask pattern 1: <c>y % 2 == 0</c>.
    /// </summary>
    PatternOne_HorizontalLines = 0b001,
    /// <summary>
    /// Mask pattern 2: <c>x % 3 == 0</c>.
    /// </summary>
    PatternTwo_VerticalLines = 0b010,
    /// <summary>
    /// Mask pattern 3: <c>(y + x) % 3 == 0</c>.
    /// </summary>
    PatternThree_DiagonalLines = 0b011,
    /// <summary>
    /// Mask pattern 4: <c>((y / 2) + (x / 3)) % 2 == 0</c>.
    /// </summary>
    PatternFour_LargeCheckerboard = 0b100,
    /// <summary>
    /// Mask pattern 5: <c>(y * x % 2) + (y * x % 3) == 0</c>.
    /// </summary>
    PatternFive_Fields = 0b101,
    /// <summary>
    /// Mask pattern 6: <c>((y * x % 2) + (y * x % 3)) % 2 == 0</c>.
    /// </summary>
    PatternSix_Diamonds = 0b110,
    /// <summary>
    /// Mask pattern 7: <c>((y + x) % 2 + (y * x % 3)) % 2 == 0</c>.
    /// </summary>
    PatternSeven_Meadow = 0b111,
}
