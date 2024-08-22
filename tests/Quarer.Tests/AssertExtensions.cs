using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit.Sdk;

namespace Quarer.Tests;

internal static class AssertExtensions
{
    private const int StackAllocCharThreshold = 256;

    public static void BitsEqual(ref DefaultInterpolatedStringHandler expected, IEnumerable<bool> actualBits, bool divideIntoBytes = false) => BitsEqual(expected.ToStringAndClear(), actualBits, divideIntoBytes);
    public static unsafe void BitsEqual(scoped ReadOnlySpan<char> expected, IEnumerable<bool> actualBits, bool divideIntoBytes = false)
    {
        scoped ReadOnlySpan<char> actualBitsBuffer;
        if (actualBits.TryGetNonEnumeratedCount(out var count))
        {
            Span<char> stackallocBuffer = count <= StackAllocCharThreshold
                ? stackalloc char[count]
                : new char[count];

            var i = 0;
            foreach (var item in actualBits)
            {
                stackallocBuffer[i++] = item ? '1' : '0';
            }
            actualBitsBuffer = stackallocBuffer;
        }
        else
        {
            var l = new List<char>(30);
            l.AddRange(actualBits.Select(x => x ? '1' : '0'));
            l.TrimExcess();
            actualBitsBuffer = CollectionsMarshal.AsSpan(l);
        }

        var initialCount = expected.ContainsAny(" \r\n") ? 30 : expected.Length;
        var expectedBits = new List<char>(initialCount);
        foreach (var line in expected.EnumerateLines())
        {
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c is not ('1' or '0' or ' '))
                {
                    throw new InvalidOperationException($"Unexpected character '{c}' ({(int)c})");
                }

                if (c == ' ')
                {
                    continue;
                }

                expectedBits.Add(c);
            }
        }

        if (!CollectionsMarshal.AsSpan(expectedBits).SequenceEqual(actualBitsBuffer))
        {
            if (divideIntoBytes)
            {
                var s1 = string.Join('|', expectedBits.Chunk(8).Select(chunk => string.Join(',', chunk)));
                var s2 = string.Join('|', actualBitsBuffer.ToArray().Chunk(8).Select(chunk => string.Join(',', chunk)));
                throw EqualException.ForMismatchedValues(s1, s2);
            }
            else
            {
                var s1 = string.Join(',', expectedBits);
                var s2 = string.Join(',', actualBitsBuffer.ToArray());
                throw EqualException.ForMismatchedValues(s1, s2);
            }
        }
    }
}
