using Quarer;
using Quarer.Numerics;

var (expTable, logTable) = GenerateLogsAndExponents(256, 0x11D);

Console.WriteLine("ExpTable:");
PrintRow("ExpTable", expTable);

Console.WriteLine();
Console.WriteLine("LogTable:");
PrintRow("LogTable", logTable);

var requiredSymbols = new HashSet<int>();
for (var i = QrVersion.MinVersion; i <= QrVersion.MaxVersion; i++)
{
    requiredSymbols.Add(QrVersionLookup.GetVersion(i, ErrorCorrectionLevel.L).ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock);
    requiredSymbols.Add(QrVersionLookup.GetVersion(i, ErrorCorrectionLevel.M).ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock);
    requiredSymbols.Add(QrVersionLookup.GetVersion(i, ErrorCorrectionLevel.Q).ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock);
    requiredSymbols.Add(QrVersionLookup.GetVersion(i, ErrorCorrectionLevel.H).ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock);
}

Console.WriteLine();
Console.WriteLine("Generator Polynomials:");

for (var i = 7; i < 31; i++)
{
    if (requiredSymbols.Contains(i))
    {
        var generator = BuildGeneratorPolynomial(i);
        PrintRow($"G{i}", generator, createOnSingleLine: true);
    }
}

Console.WriteLine();
Console.WriteLine("Pre-calculated BCH codes lookup for format information:");

foreach (var errorCorrectionLevel in (ReadOnlySpan<ErrorCorrectionLevel>)[ErrorCorrectionLevel.L, ErrorCorrectionLevel.M, ErrorCorrectionLevel.Q, ErrorCorrectionLevel.H])
{
    var table = new int[8];
    foreach (var mask in (ReadOnlySpan<QrMaskPattern>)[
        QrMaskPattern.PatternZero_Checkerboard,
        QrMaskPattern.PatternOne_HorizontalLines,
        QrMaskPattern.PatternTwo_VerticalLines,
        QrMaskPattern.PatternThree_DiagonalLines,
        QrMaskPattern.PatternFour_LargeCheckerboard,
        QrMaskPattern.PatternFive_Fields,
        QrMaskPattern.PatternSix_Diamonds,
        QrMaskPattern.PatternSeven_Meadow
        ])
    {

        var bits = ((byte)errorCorrectionLevel << 3) | ((byte)mask);
        var bchCode = (bits << 10) | CalculateBchCode((byte)bits, QrSymbolBuilder.FormatInformationGeneratorPolynomial);
        var maskedBchCode = bchCode ^ QrSymbolBuilder.FormatBchCodeMask;
        table[(int)mask] = maskedBchCode;
    }

    PrintRow($"FormatInformation{errorCorrectionLevel}", table, createOnSingleLine: false, type: "ushort", lineLength: 1, format: static x => BinaryUnderscoreSeparatedString(x, length: 15));
}

static string BinaryUnderscoreSeparatedString(int x, int length)
{
    var b = x.ToString($"B{length}");
    var split = b.Reverse().Chunk(4);
    return $"0b{string.Join("_", split.Select(x => new string(x))).Reverse().ToArray().AsSpan()}";
}
static void PrintRow(string property, int[] table, bool createOnSingleLine = false, string type = "byte", int lineLength = 16, Func<int, string>? format = null)
{
    format ??= static x => x.ToString();
    Console.Write($"private static ReadOnlySpan<{type}> {property} =>");
    Console.Write(createOnSingleLine ? " [" : $"{Environment.NewLine}[{Environment.NewLine}");

    for (var i = 0; i < table.Length; i++)
    {
        Console.Write(format(table[i]));
        Console.Write(i < table.Length - 1 ? ", " : "");
        if ((i + 1) % lineLength is 0 && !createOnSingleLine)
        {
            Console.WriteLine();
        }
    }
    Console.WriteLine("];");
}

static (int[], int[]) GenerateLogsAndExponents(int order, int primitive)
{
    var expTable = new int[order * 2];
    var logTable = new int[order];

    var x = 1;
    for (var i = 0; i < order - 1; i++)
    {
        expTable[i] = x;
        logTable[x] = i;
        x <<= 1;
        if ((x & 0x100) is 256)
        {
            x ^= primitive;
        }
    }

    for (var i = 0; i < order + 1; i++)
    {
        expTable[i + 255] = expTable[i];
    }

    return (expTable, logTable);
}

static int[] BuildGeneratorPolynomial(int numberOfSymbols)
{
    ReadOnlySpan<byte> generator = [1];
    for (var i = 0; i < numberOfSymbols; i++)
    {
        var destination = new byte[40];
        var written = BinaryFiniteField.Multiply(generator, [1, BinaryFiniteField.Pow(2, (byte)i)], destination);
        generator = destination.AsSpan()[..written];
    }

    return generator.ToArray().Select(x => (int)x).ToArray();
}

/// <summary>
/// Calculate the BCH (Bose-Chaudhuri-Hocquenghem) code for a value.
/// </summary>
/// <remarks>
/// Mostly copied from:
/// <see href="https://github.com/zxing-cpp/zxing-cpp/blob/master/core/src/qrcode/QRMatrixUtil.cpp#L143"/>
/// </remarks>
/// <returns></returns>
static int CalculateBchCode(byte bits, int polynomial)
{
    // Calculate BCH (Bose-Chaudhuri-Hocquenghem) code for "value" using polynomial "poly". The BCH
    // code is used for encoding type information and version information.
    // Example: Calculation of version information of 7.
    // f(x) is created from 7.
    //   - 7 = 000111 in 6 bits
    //   - f(x) = x^2 + x^1 + x^0
    // g(x) is given by the standard (p. 67)
    //   - g(x) = x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1
    // Multiply f(x) by x^(18 - 6)
    //   - f'(x) = f(x) * x^(18 - 6)
    //   - f'(x) = x^14 + x^13 + x^12
    // Calculate the remainder of f'(x) / g(x)
    //         x^2
    //         __________________________________________________
    //   g(x) )x^14 + x^13 + x^12
    //         x^14 + x^13 + x^12 + x^11 + x^10 + x^7 + x^4 + x^2
    //         --------------------------------------------------
    //                              x^11 + x^10 + x^7 + x^4 + x^2
    //
    // The remainder is x^11 + x^10 + x^7 + x^4 + x^2
    // Encode it in binary: 110010010100
    // The return value is 0xc94 (1100 1001 0100)
    //
    // Since all coefficients in the polynomials are 1 or 0, we can do the calculation by bit
    // operations. We don't care if cofficients are positive or negative.

    // this gives us the degree of the generator polynomial
    var degreeOfGeneratorPolynomial = MostSignificantBitSetIndex(polynomial);
    // pad input value so its bit length = degree of polynomial + bit length of bits (we may not be using the full
    // 8 bits depending on what we are encoding, QR codes use 5 or 6 for format and version information respectively)
    // this is effectively the same as multiplying by x^(degree of polynomial - bit length)
    var value = bits << (degreeOfGeneratorPolynomial - 1);

    // while remainder has a degree >= polynomial
    while (MostSignificantBitSetIndex(value) >= degreeOfGeneratorPolynomial)
    {
        var shift = MostSignificantBitSetIndex(value) - degreeOfGeneratorPolynomial;
        var divisor = polynomial << shift;
        value ^= divisor;
    }

    return value;
}

static int MostSignificantBitSetIndex(int value) => 32 - int.LeadingZeroCount(value);
