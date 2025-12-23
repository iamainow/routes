namespace routes.Test;

public class Ip4RangeSetStackAllocTest
{
    #region Constructor Tests

    [Fact]
    public void Constructor_EmptyBuffer_CreatesEmptySet()
    {
        // Arrange & Act
        Span<Ip4Range> buffer = stackalloc Ip4Range[0];
        var set = new Ip4RangeSetStackAlloc(buffer);

        // Assert
        Assert.Equal(0, set.ToReadOnlySpan().Length);
        Assert.Equal(0, set.ToSpan().Length);
    }

    [Fact]
    public void Constructor_WithEmptyElements_CreatesEmptySet()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        ReadOnlySpan<Ip4Range> elements = [];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        Assert.Equal(0, set.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Constructor_WithSingleRange_CreatesSetWithOneRange()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        ReadOnlySpan<Ip4Range> elements = [range];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(range, spans[0]);
    }

    [Fact]
    public void Constructor_WithOverlappingRanges_MergesIntoSingleRange()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        ReadOnlySpan<Ip4Range> elements = [range1, range2];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithAdjacentRanges_MergesIntoSingleRange()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(19));
        var range2 = new Ip4Range(new Ip4Address(20), new Ip4Address(30));
        ReadOnlySpan<Ip4Range> elements = [range1, range2];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithDisjointRanges_KeepsSeparateRanges()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        ReadOnlySpan<Ip4Range> elements = [range1, range2];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void Constructor_WithUnsortedInput_SortsAndNormalizes()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range3 = new Ip4Range(new Ip4Address(35), new Ip4Address(45));
        ReadOnlySpan<Ip4Range> elements = [range1, range2, range3];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(30), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(45), spans[1].LastAddress);
    }

    #endregion

    #region Span Accessor Tests

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectRanges()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        ReadOnlySpan<Ip4Range> elements = [range1, range2];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Act
        var spans = set.ToReadOnlySpan();

        // Assert
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void ToSpan_ReturnsCorrectRanges()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        ReadOnlySpan<Ip4Range> elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Act
        var spans = set.ToSpan();

        // Assert
        Assert.Equal(1, spans.Length);
        Assert.Equal(range, spans[0]);
    }

    #endregion

    #region Union Tests

    [Fact]
    public void CalcUnionBufferSize_ReturnsCorrectSize()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);

        // Act
        var size = Ip4RangeSetStackAlloc.CalcUnionBufferSize(set1, set2);

        // Assert
        Assert.Equal(2, size);
    }

    [Fact]
    public void Union1_WithDisjointRanges_CombinesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union1(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void Union1_WithOverlappingRanges_MergesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union1(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), spans[0].LastAddress);
    }

    [Fact]
    public void Union1_WithAdjacentRanges_MergesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(19));
        var range2 = new Ip4Range(new Ip4Address(20), new Ip4Address(30));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union1(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[0].LastAddress);
    }

    [Fact]
    public void Union1_WithEmptySet_NoChange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union1(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(range, spans[0]);
    }

    [Fact]
    public void Union2_WithDisjointRanges_CombinesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union2(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void Union2_WithOverlappingRanges_MergesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [range2]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union2(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), spans[0].LastAddress);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void CalcExceptBufferSize_ReturnsCorrectSize()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1, range2]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [new Ip4Range(new Ip4Address(15), new Ip4Address(35))]);

        // Act
        var size = Ip4RangeSetStackAlloc.CalcExceptBufferSize(set1, set2);

        // Assert
        Assert.Equal(2, size); // 2 ranges * 1 range = 2
    }

    [Fact]
    public void Except_WithDisjointRanges_NoChange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range1, range2]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [new Ip4Range(new Ip4Address(50), new Ip4Address(60))]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Except(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void Except_WithPartialOverlap_SplitsRanges()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        var exceptRange = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [exceptRange]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Except(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(26), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), spans[1].LastAddress);
    }

    [Fact]
    public void Except_WithCompleteOverlap_RemovesRange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var exceptRange = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [exceptRange]);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Except(ref result, set2);

        // Assert
        Assert.Equal(0, result.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Except_WithEmptySet_NoChange()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [range]);
        var set2 = new Ip4RangeSetStackAlloc(buffer2);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Except(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(range, spans[0]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Operations_WithMinimumIpAddress_WorkCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(100));
        var set = new Ip4RangeSetStackAlloc(buffer, [range]);

        // Act & Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(0), spans[0].FirstAddress);
    }

    [Fact]
    public void Operations_WithMaximumIpAddress_WorkCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range = new Ip4Range(new Ip4Address(uint.MaxValue - 100), new Ip4Address(uint.MaxValue));
        var set = new Ip4RangeSetStackAlloc(buffer, [range]);

        // Act & Assert
        var spans = set.ToReadOnlySpan();
        Assert.Equal(1, spans.Length);
        Assert.Equal(new Ip4Address(uint.MaxValue), spans[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithLargeRanges_HandlesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer = stackalloc Ip4Range[1000];
        var range1 = new Ip4Range(new Ip4Address(0), new Ip4Address(1000000));
        var range2 = new Ip4Range(new Ip4Address(2000000), new Ip4Address(3000000));
        ReadOnlySpan<Ip4Range> elements = [range1, range2];

        // Act
        var set = new Ip4RangeSetStackAlloc(buffer, elements);

        // Assert
        var spans = set.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(range1, spans[0]);
        Assert.Equal(range2, spans[1]);
    }

    [Fact]
    public void Union_WithMultipleOverlaps_NormalizesCorrectly()
    {
        // Arrange
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[10];
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[10];
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[10];
        var ranges1 = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        };
        var ranges2 = new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(55), new Ip4Address(65))
        };
        var set1 = new Ip4RangeSetStackAlloc(buffer1, ranges1);
        var set2 = new Ip4RangeSetStackAlloc(buffer2, ranges2);
        var result = new Ip4RangeSetStackAlloc(resultBuffer, []);

        // Act
        set1.Union1(ref result, set2);

        // Assert
        var spans = result.ToReadOnlySpan().ToArray();
        Assert.Equal(2, spans.Length);
        Assert.Equal(new Ip4Address(10), spans[0].FirstAddress);
        Assert.Equal(new Ip4Address(35), spans[0].LastAddress);
        Assert.Equal(new Ip4Address(40), spans[1].FirstAddress);
        Assert.Equal(new Ip4Address(65), spans[1].LastAddress);
    }

    #endregion
}