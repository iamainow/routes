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
        set1.Union(set2);

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
        set1.Union(set2);

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
        set1.Union(set2);

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
        set1.Union(set2);

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
        set1.Union(set2);

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
        set1.Union(set2);

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
        set1.Union(set2);

        // Assert: should result in full range
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Range(new Ip4Address(0), new Ip4Address(uint.MaxValue)), spans[0]);
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
        set1.Union(set2);

        // Assert: should merge correctly
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(15_000_000), spans[0].LastAddress);
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
        set1.Union(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
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
        set1.Union(set2);

        // Assert
        var spans = set1.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_SingleRangeWithMultipleExclusions_ExcludesAll()
    {
        // Arrange: [0-100] except [20-30] and [50-60]
        // Expected result: [0-19], [31-49], [61-100]
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [
            new Ip4Range(new Ip4Address(20), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        ];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(3, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(31), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(49), spans[1].LastAddress);
        Assert.Equal(new Ip4Address(61), spans[2].FirstAddress);
        Assert.Equal(new Ip4Address(100), spans[2].LastAddress);
    }

    [Fact]
    public void Except_SingleRangeWithThreeExclusions_ExcludesAll()
    {
        // Arrange: [0-100] except [10-20], [40-50], [70-80]
        // Expected result: [0-9], [21-39], [51-69], [81-100]
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        ];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(4, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(9), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(21), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(39), spans[1].LastAddress);
        Assert.Equal(new Ip4Address(51), spans[2].FirstAddress);
        Assert.Equal(new Ip4Address(69), spans[2].LastAddress);
        Assert.Equal(new Ip4Address(81), spans[3].FirstAddress);
        Assert.Equal(new Ip4Address(100), spans[3].LastAddress);
    }

    [Fact]
    public void Except_ExclusionAtStart_ExcludesCorrectly()
    {
        // Arrange: [0-100] except [0-30]
        // Expected result: [31-100]
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [new Ip4Range(new Ip4Address(0), new Ip4Address(30))];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(31), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(100), spans[0].LastAddress);
    }

    [Fact]
    public void Except_ExclusionAtEnd_ExcludesCorrectly()
    {
        // Arrange: [0-100] except [70-100]
        // Expected result: [0-69]
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [new Ip4Range(new Ip4Address(70), new Ip4Address(100))];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(69), spans[0].LastAddress);
    }

    [Fact]
    public void Except_CompleteOverlap_ReturnsEmpty()
    {
        // Arrange: [10-50] except [0-100]
        // Expected result: empty
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(50));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [new Ip4Range(new Ip4Address(0), new Ip4Address(100))];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(0, spans.Length);
    }

    [Fact]
    public void Except_DisjointRanges_NoChange()
    {
        // Arrange: [10-20] except [50-60]
        // Expected result: [10-20] unchanged
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [new Ip4Range(new Ip4Address(50), new Ip4Address(60))];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Except_EmptyExclusion_NoChange()
    {
        // Arrange: [10-20] except empty
        // Expected result: [10-20] unchanged
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Ip4Range[] elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
    }

    [Fact]
    public void Except_MultipleRangesWithMultipleExclusions_HandlesCorrectly()
    {
        // Arrange: [0-50], [100-150] except [20-30], [120-130]
        // Expected result: [0-19], [31-50], [100-119], [131-150]
        Span<Ip4Range> buffer = stackalloc Ip4Range[20];
        var range1 = new Ip4Range(new Ip4Address(0), new Ip4Address(50));
        var range2 = new Ip4Range(new Ip4Address(100), new Ip4Address(150));
        Ip4Range[] elements = [range1, range2];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        Span<Ip4Range> exclusions = [
            new Ip4Range(new Ip4Address(20), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(120), new Ip4Address(130))
        ];

        // Act
        set.Except(exclusions);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(4, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(31), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), spans[1].LastAddress);
        Assert.Equal(new Ip4Address(100), spans[2].FirstAddress);
        Assert.Equal(new Ip4Address(119), spans[2].LastAddress);
        Assert.Equal(new Ip4Address(131), spans[3].FirstAddress);
        Assert.Equal(new Ip4Address(150), spans[3].LastAddress);
    }

    #endregion
}