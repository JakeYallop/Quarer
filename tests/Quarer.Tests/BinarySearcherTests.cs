namespace Quarer.Tests;

public class BinarySearcherTests
{
    [Fact]
    public void ReturnsExpectedResult()
    {
        for (var i = 3; i <= 100; i += 2)
        {
            var span = Enumerable.Range(0, i).Select(x => x * 2).ToArray().AsSpan();
            for (var index = 0; index < span.Length; index++)
            {
                Assert.Equal(index, BinarySearcher.BinarySearchUpperBound(span, span[index]));

                var expectedNextIndexForValuePlusOne = index + 1 < span.Length ? index + 1 : -1;
                Assert.Equal(expectedNextIndexForValuePlusOne, BinarySearcher.BinarySearchUpperBound(span, span[index] + 1));
            }
        }
    }

    public static TheoryData<int, int[], int> Data()
    {
        return new()
        {
            { 1, [1], 0 },
            { 1, [1, 2], 0 },
            { 1, [0, 1, 2], 1 },
            { 2, [0, 1, 2, 3], 2 },
            { 0, [1], 0 },
            { 0, [1, 2], 0 },
            { 2, [1], -1 },
            { 3, [1, 2], -1 },
            { 4, [1, 2, 3], -1 },
            { 5, [1, 2, 3, 4], -1 },
        };
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReturnsExpectedResult2(int value, int[] array, int expected)
        => Assert.Equal(expected, BinarySearcher.BinarySearchUpperBound(array, value));
}
