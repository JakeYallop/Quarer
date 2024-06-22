using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit.Sdk;

namespace Quarer.Tests;

internal static class AssertExtensions
{
    private const int StackAllocCharThreshold = 256;

    public static void BitsEqual(ref DefaultInterpolatedStringHandler expected, IEnumerable<bool> actualBits) => BitsEqual(expected.ToStringAndClear(), actualBits);
    public static unsafe void BitsEqual(scoped ReadOnlySpan<char> expected, IEnumerable<bool> actualBits)
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
            //TODO: Does this perform a copy?
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
            var s1 = string.Join(',', expectedBits);
            //TODO: Get an array reference from the span somehow?
            var s2 = string.Join(',', actualBitsBuffer.ToArray());
            throw EqualException.ForMismatchedValues(s1, s2);
        }
    }
}
