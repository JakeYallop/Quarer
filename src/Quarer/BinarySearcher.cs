namespace Quarer;

internal static class BinarySearcher
{
    // Span overload exists to allow `Span<T>` values to correctly infer type arguments
    public static int BinarySearchUpperBound<T, TValue>(Span<T> span, TValue value, Func<T, TValue> selector) where TValue : IComparable<TValue>
        => BinarySearchUpperBound((ReadOnlySpan<T>)span, value, selector);
    public static int BinarySearchUpperBound<T, TValue>(ReadOnlySpan<T> span, TValue value, Func<T, TValue> selector) where TValue : IComparable<TValue>
    {
        if (span.Length == 0)
        {
            return -1;
        }

        if (span.Length == 1)
        {
            return selector(span[0]).CompareTo(value) is 0 or 1 ? 0 : -1;
        }

        var low = 0;
        var high = span.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            if (selector(span[mid]).CompareTo(value) == -1)
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
