namespace routes.Test;

/// <summary>
/// Unit tests for SpanHelperBinaryOptimized covering:
/// - FindFirst: Binary search for first element satisfying ascending predicate
/// - FindLast: Binary search for last element satisfying descending predicate
/// - Edge cases: empty arrays, single elements, boundary conditions
/// - Various array sizes to cover all code paths (1-5+ elements)
/// </summary>
public class SpanHelperBinaryOptimizedTest
{
    #region FindFirst Tests

    [Fact]
    public void FindFirst_EmptyArray_ReturnsArrayIsEmpty()
    {
        ReadOnlySpan<int> empty = [];

        var result = SpanHelperBinaryOptimized.FindFirst(empty, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ArrayIsEmpty, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_SingleElement_Satisfies_ReturnsElementFound()
    {
        ReadOnlySpan<int> array = [10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_SingleElement_DoesNotSatisfy_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [3];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_TwoElements_FirstSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_TwoElements_SecondSatisfies_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [3, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindFirst_TwoElements_NeitherSatisfies_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [1, 3];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_ThreeElements_FirstSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_ThreeElements_SecondSatisfies_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [3, 5, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindFirst_ThreeElements_ThirdSatisfies_ReturnsIndexTwo()
    {
        ReadOnlySpan<int> array = [1, 3, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindFirst_ThreeElements_NeitherSatisfies_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [1, 2, 3];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_FourElements_FindsCorrectIndex()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 3, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index); // 4 is the first element >= 3
    }

    [Fact]
    public void FindFirst_FiveElements_FindsCorrectIndex()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index); // 6 is the first element >= 5
    }

    [Fact]
    public void FindFirst_DocumentedExample_GreaterThanOrEqual5_ReturnsIndex3()
    {
        // From the XML doc: FindFirst([0,2,4,6,8], x => x >= 5) should return index of number 6, it's 3
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindFirst_DocumentedExample_WithDuplicates_ReturnsFirstIndex()
    {
        // From the XML doc: FindFirst([0,5,5,5,8], x => x >= 5) should return index of first number 5, it's 1
        ReadOnlySpan<int> array = [0, 5, 5, 5, 8];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindFirst_LargeArray_FindsFirstSatisfying()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i * 2; // 0, 2, 4, 6, ..., 198
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(25, index); // 25 * 2 = 50
    }

    [Fact]
    public void FindFirst_LargeArray_NoneMatch_ReturnsAllElementsNotSatisfies()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i; // 0, 1, 2, ..., 99
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 100, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
    }

    [Fact]
    public void FindFirst_LargeArray_AllMatch_ReturnsIndexZero()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i + 100; // 100, 101, ..., 199
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_NullPredicate_ThrowsArgumentNullException()
    {
        int[] array = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() =>
            SpanHelperBinaryOptimized.FindFirst(array, null!, out _));
    }

    [Fact]
    public void FindFirst_SixElements_FindsCorrectIndex()
    {
        // Testing default case (> 5 elements) with exact match at various positions
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 7, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index); // 8 is the first element >= 7
    }

    [Fact]
    public void FindFirst_SevenElements_FindsAtMiddle()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5, 6, 7];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 4, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindFirst_TenElements_FindsLastElement()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 100];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(9, index);
    }

    #endregion

    #region FindLast Tests

    [Fact]
    public void FindLast_EmptyArray_ReturnsArrayIsEmpty()
    {
        ReadOnlySpan<int> empty = [];

        var result = SpanHelperBinaryOptimized.FindLast(empty, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ArrayIsEmpty, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_SingleElement_Satisfies_ReturnsElementFound()
    {
        ReadOnlySpan<int> array = [3];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_SingleElement_DoesNotSatisfy_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_TwoElements_LastSatisfies_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [3, 4];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLast_TwoElements_OnlyFirstSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [3, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_TwoElements_NeitherSatisfies_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [10, 20];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_ThreeElements_AllSatisfy_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [1, 2, 3];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_ThreeElements_TwoSatisfy_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [1, 3, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLast_ThreeElements_OneSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [3, 10, 20];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_ThreeElements_NeitherSatisfies_ReturnsAllElementsNotSatisfies()
    {
        ReadOnlySpan<int> array = [10, 20, 30];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_FourElements_FindsCorrectIndex()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index); // 4 is the last element <= 5
    }

    [Fact]
    public void FindLast_FiveElements_FindsCorrectIndex()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index); // 4 is the last element <= 5
    }

    [Fact]
    public void FindLast_DocumentedExample_LessThanOrEqual5_ReturnsIndex2()
    {
        // From the XML doc: FindFirst([0,2,4,6,8], x => x <= 5) should return index of number 4, it's 2
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_DocumentedExample_WithDuplicates_ReturnsLastIndex()
    {
        // From the XML doc: FindFirst([0,5,5,5,8], x => x <= 5) should return index of last number 5, it's 3
        ReadOnlySpan<int> array = [0, 5, 5, 5, 8];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindLast_LargeArray_FindsLastSatisfying()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i * 2; // 0, 2, 4, 6, ..., 198
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(25, index); // 25 * 2 = 50
    }

    [Fact]
    public void FindLast_LargeArray_NoneMatch_ReturnsAllElementsNotSatisfies()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i + 100; // 100, 101, 102, ..., 199
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 50, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
    }

    [Fact]
    public void FindLast_LargeArray_AllMatch_ReturnsLastIndex()
    {
        int[] data = new int[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = i; // 0, 1, 2, ..., 99
        }
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 200, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(99, index);
    }

    [Fact]
    public void FindLast_NullPredicate_ThrowsArgumentNullException()
    {
        int[] array = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() =>
            SpanHelperBinaryOptimized.FindLast(array, null!, out _));
    }

    [Fact]
    public void FindLast_SixElements_FindsCorrectIndex()
    {
        // Testing default case (> 5 elements)
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 7, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index); // 6 is the last element <= 7
    }

    [Fact]
    public void FindLast_SevenElements_FindsAtMiddle()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5, 6, 7];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 4, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindLast_TenElements_FindsFirstElement()
    {
        ReadOnlySpan<int> array = [1, 100, 200, 300, 400, 500, 600, 700, 800, 900];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    #endregion

    #region FindFirst - Binary Search Path Coverage

    [Fact]
    public void FindFirst_FourElements_FirstSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_FourElements_SecondSatisfies_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [1, 5, 10, 15];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindFirst_FourElements_ThirdSatisfies_ReturnsIndexTwo()
    {
        ReadOnlySpan<int> array = [1, 2, 5, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindFirst_FourElements_FourthSatisfies_ReturnsIndexThree()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 5];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindFirst_FiveElements_FirstSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_FiveElements_SecondSatisfies_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [1, 5, 10, 15, 20];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindFirst_FiveElements_ThirdSatisfies_ReturnsIndexTwo()
    {
        ReadOnlySpan<int> array = [1, 2, 5, 10, 15];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindFirst_FiveElements_FourthSatisfies_ReturnsIndexThree()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 5, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindFirst_FiveElements_FifthSatisfies_ReturnsIndexFour()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    #endregion

    #region FindLast - Binary Search Path Coverage

    [Fact]
    public void FindLast_FourElements_AllSatisfy_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 10, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindLast_FourElements_ThreeSatisfy_ReturnsIndexTwo()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_FourElements_TwoSatisfy_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [1, 2, 10, 20];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLast_FourElements_OneSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [1, 10, 20, 30];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_FiveElements_AllSatisfy_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 10, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindLast_FiveElements_FourSatisfy_ReturnsIndexThree()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindLast_FiveElements_ThreeSatisfy_ReturnsIndexTwo()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 10, 20];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_FiveElements_TwoSatisfy_ReturnsIndexOne()
    {
        ReadOnlySpan<int> array = [1, 2, 10, 20, 30];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLast_FiveElements_OneSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [1, 10, 20, 30, 40];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    #endregion

    #region Edge Cases and Special Scenarios

    [Fact]
    public void FindFirst_AllElementsSatisfy_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [100, 200, 300, 400, 500];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_OnlyLastElementSatisfies_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 100];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindLast_AllElementsSatisfy_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [1, 2, 3, 4, 5];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 100, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindLast_OnlyFirstElementSatisfies_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [1, 100, 200, 300, 400];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 50, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_ExactMatch_ReturnsCorrectIndex()
    {
        ReadOnlySpan<int> array = [10, 20, 30, 40, 50];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 30, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_ExactMatch_ReturnsCorrectIndex()
    {
        ReadOnlySpan<int> array = [10, 20, 30, 40, 50];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 30, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindFirst_WithNegativeNumbers_WorksCorrectly()
    {
        ReadOnlySpan<int> array = [-10, -5, 0, 5, 10];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 0, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindLast_WithNegativeNumbers_WorksCorrectly()
    {
        ReadOnlySpan<int> array = [-10, -5, 0, 5, 10];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 0, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindFirst_WithCustomType_WorksCorrectly()
    {
        ReadOnlySpan<string> array = ["apple", "banana", "cherry", "date", "elderberry"];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => string.Compare(x, "c", StringComparison.Ordinal) >= 0, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index); // "cherry" is first >= "c"
    }

    [Fact]
    public void FindLast_WithCustomType_WorksCorrectly()
    {
        ReadOnlySpan<string> array = ["apple", "banana", "cherry", "date", "elderberry"];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => string.Compare(x, "d", StringComparison.Ordinal) <= 0, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(2, index); // "cherry" is last <= "d"
    }

    #endregion

    #region Larger Array Recursive Path Tests

    [Fact]
    public void FindFirst_20Elements_FindsInLeftHalf()
    {
        int[] data = Enumerable.Range(0, 20).Select(i => i * 10).ToArray(); // 0, 10, 20, ..., 190
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 45, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(5, index); // 50 is first >= 45
    }

    [Fact]
    public void FindFirst_20Elements_FindsInRightHalf()
    {
        int[] data = Enumerable.Range(0, 20).Select(i => i * 10).ToArray(); // 0, 10, 20, ..., 190
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 145, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(15, index); // 150 is first >= 145
    }

    [Fact]
    public void FindLast_20Elements_FindsInLeftHalf()
    {
        int[] data = Enumerable.Range(0, 20).Select(i => i * 10).ToArray(); // 0, 10, 20, ..., 190
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 45, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index); // 40 is last <= 45
    }

    [Fact]
    public void FindLast_20Elements_FindsInRightHalf()
    {
        int[] data = Enumerable.Range(0, 20).Select(i => i * 10).ToArray(); // 0, 10, 20, ..., 190
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 145, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(14, index); // 140 is last <= 145
    }

    [Fact]
    public void FindFirst_1000Elements_FindsCorrectly()
    {
        int[] data = Enumerable.Range(0, 1000).ToArray();
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 500, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(500, index);
    }

    [Fact]
    public void FindLast_1000Elements_FindsCorrectly()
    {
        int[] data = Enumerable.Range(0, 1000).ToArray();
        ReadOnlySpan<int> array = data;

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 500, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(500, index);
    }

    #endregion

    #region Boundary Condition Tests

    [Fact]
    public void FindFirst_BoundaryAtFirstElement_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_BoundaryJustBeforeFirstElement_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 4, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindFirst_BoundaryAtLastElement_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 25, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindFirst_BoundaryJustAfterLastElement_ReturnsNotSatisfies()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindFirst(array, x => x >= 26, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
    }

    [Fact]
    public void FindLast_BoundaryAtLastElement_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 25, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindLast_BoundaryJustAfterLastElement_ReturnsLastIndex()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 26, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(4, index);
    }

    [Fact]
    public void FindLast_BoundaryAtFirstElement_ReturnsIndexZero()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 5, out var index);

        Assert.Equal(SearchResult.ElementFound, result);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindLast_BoundaryJustBeforeFirstElement_ReturnsNotSatisfies()
    {
        ReadOnlySpan<int> array = [5, 10, 15, 20, 25];

        var result = SpanHelperBinaryOptimized.FindLast(array, x => x <= 4, out var index);

        Assert.Equal(SearchResult.AllElementsNotSatisfiesCondition, result);
    }

    #endregion

    #region FindFirstAndFindLast Tests

    [Fact]
    public void FindFirstAndFindLast_EmptyArray_ReturnsArrayIsEmpty()
    {
        ReadOnlySpan<int> empty = [];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(empty, x => x >= 5, x => x <= 10, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ArrayIsEmpty, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(0, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_NoAscSatisfaction_ReturnsOutOfBoundsAtRight()
    {
        ReadOnlySpan<int> array = [0, 2, 4];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 10, x => x <= 15, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.OutOfBoundsAtRight, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(0, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_AscSatisfied_NoDescSatisfaction_ReturnsOutOfBoundsAtLeft()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        try
        {
            SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 5, x => x <= 3, out var firstIndex, out var lastIndex);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [Fact]
    public void FindFirstAndFindLast_SingleElement_BothSatisfy_ReturnsElementFound()
    {
        ReadOnlySpan<int> array = [10];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 5, x => x <= 15, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(0, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_TwoElements_BothSatisfy_ReturnsIndices()
    {
        ReadOnlySpan<int> array = [5, 10];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 0, x => x <= 15, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(1, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_ThreeElements_PartialSatisfy_ReturnsCorrectIndices()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 2, x => x <= 6, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(1, firstIndex); // 2
        Assert.Equal(3, lastIndex); // 6 (relative to subspan starting at 1: [2,4,6,8] -> last <=6 is 6 at index 2, so absolute 1+2=3)
    }

    [Fact]
    public void FindFirstAndFindLast_DocumentedExample_RangeMinus10To5_ReturnsIndices0To2()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= -10, x => x <= 5, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(2, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_DocumentedExample_Range5To10_ReturnsIndices3To4()
    {
        ReadOnlySpan<int> array = [0, 2, 4, 6, 8];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 5, x => x <= 10, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(3, firstIndex); // 6
        Assert.Equal(4, lastIndex); // 8
    }

    [Fact]
    public void FindFirstAndFindLast_WithDuplicates_ReturnsCorrectRange()
    {
        ReadOnlySpan<int> array = [0, 5, 5, 5, 8];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 5, x => x <= 5, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(1, firstIndex); // first 5
        Assert.Equal(3, lastIndex); // last 5
    }

    [Fact]
    public void FindFirstAndFindLast_AllElementsSatisfy_ReturnsFullRange()
    {
        ReadOnlySpan<int> array = [0, 2, 4];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= -1, x => x <= 10, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(0, firstIndex);
        Assert.Equal(2, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_FirstAndLastSameIndex_ReturnsSameIndex()
    {
        ReadOnlySpan<int> array = [0, 5, 10];

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 5, x => x <= 5, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(1, firstIndex);
        Assert.Equal(1, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_LargeArray_FindsCorrectIndices()
    {
        var array = Enumerable.Range(0, 100).ToArray().AsSpan();

        var result = SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 20, x => x <= 50, out var firstIndex, out var lastIndex);

        Assert.Equal(SearchResult2.ElementFound, result);
        Assert.Equal(20, firstIndex);
        Assert.Equal(50, lastIndex);
    }

    [Fact]
    public void FindFirstAndFindLast_NullAscPredicate_ThrowsArgumentNullException()
    {
        ReadOnlySpan<int> array = [0, 2, 4];

        try
        {
            SpanHelperBinaryOptimized.FindFirstAndFindLast(array, null!, x => x <= 5, out _, out _);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [Fact]
    public void FindFirstAndFindLast_NullDescPredicate_ThrowsArgumentNullException()
    {
        ReadOnlySpan<int> array = [0, 2, 4];

        try
        {
            SpanHelperBinaryOptimized.FindFirstAndFindLast(array, x => x >= 0, null!, out _, out _);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    #endregion
}
