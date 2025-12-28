namespace routes.Test;

public class Ip4RangeSetStackAllocTest
{
    #region Constructor Tests

    [Fact]
    public void Constructor_EmptyBuffer_CreatesEmptySet()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void Constructor_WithEmptyElements_CreatesEmptySet()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        ReadOnlySpan<Ip4Range> elements = ReadOnlySpan<Ip4Range>.Empty;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void Constructor_WithSingleElement_CreatesSetWithOneRange()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elementsArray = [range];
        ReadOnlySpan<Ip4Range> elements = elementsArray;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(range.FirstAddress, spans[0].FirstAddress);
        Assert.Equal(range.LastAddress, spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithUnsortedElements_SortsAndCreatesSet()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elementsArray = [range1, range2];
        ReadOnlySpan<Ip4Range> elements = elementsArray;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert: should be sorted by FirstAddress
        var spans = set.ToReadOnlySpan();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(30), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), spans[1].LastAddress);
    }

    [Fact]
    public void Constructor_WithOverlappingElements_MergesThem()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(25));
        var range2 = new Ip4Range(new Ip4Address(20), new Ip4Address(30));
        Ip4Range[] elementsArray = [range1, range2];
        ReadOnlySpan<Ip4Range> elements = elementsArray;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert: should merge overlapping ranges
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithAdjacentElements_MergesThem()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
        Ip4Range[] elementsArray = [range1, range2];
        ReadOnlySpan<Ip4Range> elements = elementsArray;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert: should merge adjacent ranges
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithDisjointElements_KeepsThemSeparate()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        Ip4Range[] elementsArray = [range1, range2];
        ReadOnlySpan<Ip4Range> elements = elementsArray;

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert: should keep disjoint ranges separate
        var spans = set.ToReadOnlySpan();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(30), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), spans[1].LastAddress);
    }

    #endregion

    #region Span Accessor Tests

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectReadOnlySpan()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elementsArray = [range];
        ReadOnlySpan<Ip4Range> elements = elementsArray;
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Act
        var span = set.ToReadOnlySpan();

        // Assert

        Assert.Equal(1, span.Length);
        Assert.Equal(range.FirstAddress, span[0].FirstAddress);
        Assert.Equal(range.LastAddress, span[0].LastAddress);
    }

    [Fact]
    public void SpanAccessors_EmptySet_ReturnEmptySpans()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[10];
        var set = new Ip4RangeSetStackAlloc(buffer);

        // Act
        var readonlySpan = set.ToReadOnlySpan();

        // Assert
        Assert.Equal(0, readonlySpan.Length);
    }

    #endregion

    #region Union Tests

    [Fact]
    public void Union_DisjointRanges_KeepsBothRanges()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Debug: check initial state
        var initialSpans1 = set1.ToReadOnlySpan();
        var initialSpans2 = set2.ToReadOnlySpan();
        Assert.Equal(1, initialSpans1.Length);
        Assert.Equal(1, initialSpans2.Length);

        // Act
        set1.Union1(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(2, spans.Length); // This is failing
        if (spans.Length >= 1)
        {
            Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
            Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
        }
        if (spans.Length >= 2)
        {
            Assert.Equal(new Ip4Address(30), spans[1].FirstAddress);
            Assert.Equal(new Ip4Address(40), spans[1].LastAddress);
        }
    }

    [Fact]
    public void Union_AdjacentRanges_MergesThem()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.Union1(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Union_WithEmptySet_DoesNothing()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var set2 = new Ip4RangeSetStackAlloc(buffer2);

        // Act
        set1.Union1(set2);

        // Assert: set1 should remain unchanged
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Union_EmptySetWithNonEmpty_AddsRanges()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        var set1 = new Ip4RangeSetStackAlloc(buffer1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.Union1(set2);

        // Assert: set1 should now contain set2's range
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Union_MultipleRanges_HandlesComplexMerging()
    {
        // Arrange: set1 has [10-20], [40-50], set2 has [15-35], [60-70]
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        var ranges1 = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        };
        var set1 = new Ip4RangeSetStackAlloc(buffer1, ranges1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var ranges2 = new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(60), new Ip4Address(70))
        };
        var set2 = new Ip4RangeSetStackAlloc(buffer2, ranges2);

        // Act
        set1.Union1(set2);

        // Assert: should have [10-35], [40-50], [60-70]
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(3, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(35), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(40), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), spans[1].LastAddress);
        Assert.Equal(new Ip4Address(60), spans[2].FirstAddress);
        Assert.Equal(new Ip4Address(70), spans[2].LastAddress);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_DisjointRanges_NoChange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: set1 should remain unchanged
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Except_CompleteOverlap_RemovesAll()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: set1 should be empty
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void Except_PartialOverlapAtStart_TruncatesRange()
    {
        // Arrange: [10-30] except [5-20] should result in [21-30]
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(5), new Ip4Address(20));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(21), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Except_PartialOverlapAtEnd_TruncatesRange()
    {
        // Arrange: [10-30] except [20-40] should result in [10-19]
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(20), new Ip4Address(40));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), spans[0].LastAddress);
    }

    [Fact]
    public void Except_MiddleOverlap_SplitsIntoTwoRanges()
    {
        // Arrange: [10-30] except [15-20] should result in [10-14] and [21-30]
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(15), new Ip4Address(20));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(21), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[1].LastAddress);
    }

    [Fact]
    public void Except_WithEmptySet_NoChange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var set2 = new Ip4RangeSetStackAlloc(buffer2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: set1 should remain unchanged
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Except_EmptySetExceptingNonEmpty_RemainsEmpty()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var set1 = new Ip4RangeSetStackAlloc(buffer1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: set1 should remain empty
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void Except_MultipleRanges_ComplexExclusions()
    {
        // Arrange: [10-20], [30-40], [50-60] except [15-35], [55-65]
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[50];
        var ranges1 = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        };
        var set1 = new Ip4RangeSetStackAlloc(buffer1, ranges1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var ranges2 = new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(55), new Ip4Address(65))
        };
        var set2 = new Ip4RangeSetStackAlloc(buffer2, ranges2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: should result in [10-14], [36-40], [50-54]
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(3, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(36), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), spans[1].LastAddress);
        Assert.Equal(new Ip4Address(50), spans[2].FirstAddress);
        Assert.Equal(new Ip4Address(54), spans[2].LastAddress);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void Operations_SingleIpRanges_WorkCorrectly()
    {
        // Arrange: /32 ranges (single IP)
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(10));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(10));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act: Union with same single IP
        set1.Union1(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(10), spans[0].LastAddress);
    }

    [Fact]
    public void Operations_AtMinimumIpAddress_WorkCorrectly()
    {
        // Arrange: range starting at 0.0.0.0
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Act & Assert: should handle minimum address
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
    }

    [Fact]
    public void Operations_AtMaximumIpAddress_WorkCorrectly()
    {
        // Arrange: range ending at 255.255.255.255
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(uint.MaxValue - 100), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Act & Assert: should handle maximum address
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    [Fact]
    public void Union_FullIpRange_WorksCorrectly()
    {
        // Arrange: union with entire IP range
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(1000), new Ip4Address(2000));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var range2 = Ip4Range.All; // 0.0.0.0 to 255.255.255.255
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act: union with full range
        set1.Union1(set2);

        // Assert: should result in full range
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    [Fact]
    public void Except_FullIpRange_ResultsInEmptySet()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(1000), new Ip4Address(2000));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var range2 = Ip4Range.All; // 0.0.0.0 to 255.255.255.255
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act: except entire IP range
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert: should be empty
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void LargeRangeOperations_PerformCorrectly()
    {
        // Arrange: large ranges for correctness testing
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(0), new Ip4Address(10_000_000));
        Ip4Range[] elements1 = [range1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var range2 = new Ip4Range(new Ip4Address(5_000_000), new Ip4Address(15_000_000));
        Ip4Range[] elements2 = [range2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act: union with another large range
        set1.Union1(set2);

        // Assert: should merge correctly
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(15_000_000), spans[0].LastAddress);
    }

    [Fact]
    public void MultipleOperations_MaintainsSortedOrder()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[50];

        // Start with empty set
        var set = new Ip4RangeSetStackAlloc(buffer);

        // Act: perform multiple operations
        Span<Ip4Range> tempBuffer1 = stackalloc Ip4Range[10];
        Ip4Range[] elements1 = [new Ip4Range(new Ip4Address(50), new Ip4Address(60))];
        var set1 = new Ip4RangeSetStackAlloc(tempBuffer1, elements1);
        set.Union1(set1);

        Span<Ip4Range> tempBuffer2 = stackalloc Ip4Range[10];
        Ip4Range[] elements2 = [new Ip4Range(new Ip4Address(10), new Ip4Address(20))];
        var set2 = new Ip4RangeSetStackAlloc(tempBuffer2, elements2);
        set.Union1(set2);

        Span<Ip4Range> tempBuffer3 = stackalloc Ip4Range[10];
        Ip4Range[] elements3 = [new Ip4Range(new Ip4Address(30), new Ip4Address(40))];
        var set3 = new Ip4RangeSetStackAlloc(tempBuffer3, elements3);
        set.Union1(set3);

        Span<Ip4Range> tempBuffer4 = stackalloc Ip4Range[20];
        Ip4Range[] elements4 = [new Ip4Range(new Ip4Address(35), new Ip4Address(55))];
        var set4 = new Ip4RangeSetStackAlloc(tempBuffer4, elements4);
        set.ExceptSorted(set4.ToReadOnlySpan());

        // Assert: ranges should be in sorted order
        var spans = set.ToReadOnlySpan();
        for (int i = 0; i < spans.Length - 1; i++)
        {
            Assert.True(spans[i].LastAddress.ToUInt32() + 1 <= spans[i + 1].FirstAddress.ToUInt32(),
                $"Ranges not properly sorted or adjacent: {spans[i]} and {spans[i + 1]}");
        }
    }

    [Fact]
    public void Union_AdjacentRangesAtMaxBoundary_MergesCorrectly()
    {
        // Arrange: ranges adjacent at uint.MaxValue boundary
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue - 5));
        Ip4Range[] elements1 = [r1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue - 4), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements2 = [r2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.Union1(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    [Fact]
    public void Except_RangeStartingAtZero_NoUnderflow()
    {
        // Arrange: set [0, 20], except [0, 10] - tests underflow prevention at 0
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var r1 = new Ip4Range(new Ip4Address(0), new Ip4Address(20));
        Ip4Range[] elements1 = [r1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var r2 = new Ip4Range(new Ip4Address(0), new Ip4Address(10));
        Ip4Range[] elements2 = [r2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(11), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Union_RangeEndingAtMaxIp_HandlesCorrectly()
    {
        // Arrange: union with range ending at uint.MaxValue
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue - 5));
        Ip4Range[] elements1 = [r1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue - 4), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements2 = [r2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.Union1(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    [Fact]
    public void Except_RangeEndingAtMaxIp_NoOverflow()
    {
        // Arrange: set [uint.MaxValue-10, uint.MaxValue], except [uint.MaxValue-5, uint.MaxValue] - tests overflow prevention at uint.MaxValue
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements1 = [r1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue - 5), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements2 = [r2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        // Act
        set1.ExceptSorted(set2.ToReadOnlySpan());

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 6), spans[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_AtBoundaries_HandlesOverflowCorrectly()
    {
        // Test IntersectableExcept via ExceptSorted at boundaries
        // Case 1: Excepting range ending at uint.MaxValue that covers the end
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[20];
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 5), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements1 = [r1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, elements1);

        Span<Ip4Range> buffer2 = stackalloc Ip4Range[20];
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue), new Ip4Address(uint.MaxValue));
        Ip4Range[] elements2 = [r2];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, elements2);

        set1.ExceptSorted(set2.ToReadOnlySpan());
        var spans1 = set1.ToReadOnlySpan();
        Assert.Equal(1, spans1.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 5), spans1[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 1), spans1[0].LastAddress);

        // Case 2: Excepting range starting at 0 that covers the start
        Span<Ip4Range> buffer3 = stackalloc Ip4Range[20];
        var r3 = new Ip4Range(new Ip4Address(0), new Ip4Address(10));
        Ip4Range[] elements3 = [r3];
        var set3 = new Ip4RangeSetStackAlloc(buffer3, elements3);

        Span<Ip4Range> buffer4 = stackalloc Ip4Range[20];
        var r4 = new Ip4Range(new Ip4Address(0), new Ip4Address(0));
        Ip4Range[] elements4 = [r4];
        var set4 = new Ip4RangeSetStackAlloc(buffer4, elements4);

        set3.ExceptSorted(set4.ToReadOnlySpan());
        var spans2 = set3.ToReadOnlySpan();
        Assert.Equal(1, spans2.Length);
        Assert.Equal(new Ip4Address(1), spans2[0].FirstAddress);
        Assert.Equal(new Ip4Address(10), spans2[0].LastAddress);
    }

    #endregion

    #region Stress Tests for Buffer Size Validation

    [Fact]
    public void Constructor_WithMoreThanMaxStackAllocRanges_ThrowsInvalidOperationException()
    {
        // Arrange: Create 1025 ranges (exceeds MAX_STACK_ALLOC of 1024)
        const int rangeCount = 1025;
        var ranges = new Ip4Range[rangeCount];
        for (int i = 0; i < rangeCount; i++)
        {
            ranges[i] = new Ip4Range(new Ip4Address((uint)i * 10), new Ip4Address((uint)i * 10 + 5));
        }

        // Act & Assert: Should throw InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<Ip4Range> buffer = stackalloc Ip4Range[rangeCount];
            new Ip4RangeSetStackAlloc(buffer, ranges);
        });
    }

    [Fact]
    public void Constructor_WithExactlyMaxStackAllocRanges_WorksCorrectly()
    {
        // Arrange: Create exactly 1024 ranges (MAX_STACK_ALLOC)
        const int rangeCount = 1024;
        var ranges = new Ip4Range[rangeCount];
        for (int i = 0; i < rangeCount; i++)
        {
            ranges[i] = new Ip4Range(new Ip4Address((uint)i * 10), new Ip4Address((uint)i * 10 + 5));
        }

        Span<Ip4Range> buffer = stackalloc Ip4Range[rangeCount];

        // Act: Should not throw
        var set = new Ip4RangeSetStackAlloc(buffer, ranges);

        // Assert: Should contain all ranges
        var spans = set.ToReadOnlySpan();
        Assert.Equal(rangeCount, spans.Length);
    }

    [Fact]
    public void Union1_WithCombinedSizeExceedingMaxStackAlloc_ThrowsInvalidOperationException()
    {
        // Act & Assert: Should throw InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
        {
            // Arrange: Create two sets where combined size > 1024
            const int set1Size = 600;
            const int set2Size = 500; // Combined = 1100 > 1024

            Span<Ip4Range> buffer1 = stackalloc Ip4Range[set1Size];
            var ranges1 = new Ip4Range[set1Size];
            for (int i = 0; i < set1Size; i++)
            {
                ranges1[i] = new Ip4Range(new Ip4Address((uint)i * 20), new Ip4Address((uint)i * 20 + 10));
            }
            var set1 = new Ip4RangeSetStackAlloc(buffer1, ranges1);

            Span<Ip4Range> buffer2 = stackalloc Ip4Range[set2Size];
            var ranges2 = new Ip4Range[set2Size];
            for (int i = 0; i < set2Size; i++)
            {
                ranges2[i] = new Ip4Range(new Ip4Address((uint)(i + 10000) * 20), new Ip4Address((uint)(i + 10000) * 20 + 10));
            }
            var set2 = new Ip4RangeSetStackAlloc(buffer2, ranges2);

            set1.Union1(set2);
        });
    }

    [Fact]
    public void ExceptSorted_WithCombinedSizeExceedingMaxStackAlloc_ThrowsInvalidOperationException()
    {
        // Act & Assert: Should throw InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
        {
            // Arrange: Create two sets where combined size > 1024
            const int set1Size = 600;
            const int set2Size = 500; // Combined = 1100 > 1024

            Span<Ip4Range> buffer1 = stackalloc Ip4Range[set1Size];
            var ranges1 = new Ip4Range[set1Size];
            for (int i = 0; i < set1Size; i++)
            {
                ranges1[i] = new Ip4Range(new Ip4Address((uint)i * 20), new Ip4Address((uint)i * 20 + 10));
            }
            var set1 = new Ip4RangeSetStackAlloc(buffer1, ranges1);

            Span<Ip4Range> buffer2 = stackalloc Ip4Range[set2Size];
            var ranges2 = new Ip4Range[set2Size];
            for (int i = 0; i < set2Size; i++)
            {
                ranges2[i] = new Ip4Range(new Ip4Address((uint)i * 20 + 5), new Ip4Address((uint)i * 20 + 15)); // Overlapping
            }
            var set2 = new Ip4RangeSetStackAlloc(buffer2, ranges2);

            set1.ExceptSorted(set2.ToReadOnlySpan());
        });
    }

    #endregion
}