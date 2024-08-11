using System.Text;
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

static void PrintRow(string property, int[] table, bool createOnSingleLine = false)
{
    Console.Write($"private static ReadOnlySpan<byte> {property} =>");
    Console.Write(createOnSingleLine ? " [" : $"{Environment.NewLine}[{Environment.NewLine}");

    for (var i = 0; i < table.Length; i++)
    {
        Console.Write(table[i]);
        Console.Write(i < table.Length - 1 ? ", " : "");
        if ((i + 1) % 16 is 0 && !createOnSingleLine)
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
