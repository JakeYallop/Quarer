namespace Quarer;

internal static class BinarySearcher
{
    // Span overload exists to allow `Span<T>` values to correctly infer type arguments
    public static int BinarySearchUpperBound<T, T1Arg, T2Arg, TValue>(Span<T> span, TValue value, T1Arg arg, T2Arg arg2, Func<T, T1Arg, T2Arg, TValue> selector) where TValue : IComparable<TValue>
        => BinarySearchUpperBound((ReadOnlySpan<T>)span, value, arg, arg2, selector);
    public static int BinarySearchUpperBound<T, T1Arg, T2Arg, TValue>(ReadOnlySpan<T> span, TValue value, T1Arg arg, T2Arg arg2, Func<T, T1Arg, T2Arg, TValue> selector) where TValue : IComparable<TValue>
    {
        if (span.Length == 0)
        {
            return -1;
        }

        if (span.Length == 1)
        {
            return selector(span[0], arg, arg2).CompareTo(value) is 0 or 1 ? 0 : -1;
        }

        var low = 0;
        var high = span.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            if (selector(span[mid], arg, arg2).CompareTo(value) == -1)
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
