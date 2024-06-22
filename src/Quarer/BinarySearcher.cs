namespace Quarer;

internal static class BinarySearcher
{
    public static int BinarySearchUpperBound(ReadOnlySpan<int> span, int value)
    {
        if (span.Length == 0)
        {
            return value;
        }

        if (span.Length == 1)
        {
            return span[0] >= value ? 0 : -1;
        }

        var low = 0;
        var high = span.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            if (span[mid] < value)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }

        }

        return low >= span.Length ? -1 : low;
    }
}
