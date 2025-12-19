namespace routes.Test;

using Xunit;

public class Ip4RangeSetStackAllocTest
{
    #region Constructor Tests

    [Fact]
    public void Constructor_EmptyElements_ProducesEmptySet()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[1];
        Span<Ip4Range> elements = default;
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        Assert.Equal(0, set.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Constructor_SingleElement_PreservesRange()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[1];
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        Span<Ip4Range> elements = [range];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(1, result.Length);
        Assert.Equal(range, result[0]);
    }

    [Fact]
    public void Constructor_MultipleDisjointSorted_NoChanges()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[3];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var r3 = new Ip4Range(new Ip4Address(50), new Ip4Address(60));
        Span<Ip4Range> elements = [r1, r2, r3];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(3, result.Length);
        Assert.Equal(r1, result[0]);
        Assert.Equal(r2, result[1]);
        Assert.Equal(r3, result[2]);
    }

    [Fact]
    public void Constructor_OverlappingElements_MergesIntoSingleRange()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[2];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        Span<Ip4Range> elements = [r1, r2];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(1, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), result[0].LastAddress);
    }

    [Fact]
    public void Constructor_AdjacentElements_Merges()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[2];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
        Span<Ip4Range> elements = [r1, r2];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(1, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Constructor_UnsortedElements_SortsAndNormalizes()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[3];
        var r1 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var r2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r3 = new Ip4Range(new Ip4Address(15), new Ip4Address(35)); // overlaps with both
        Span<Ip4Range> elements = [r1, r2, r3];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(1, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[0].LastAddress);
    }

    [Fact]
    public void Constructor_MixedOverlappingAndAdjacent_ComplexMerge()
    {
        Span<Ip4Range> buffer = stackalloc Ip4Range[4];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25)); // overlap
        var r3 = new Ip4Range(new Ip4Address(26), new Ip4Address(30)); // adjacent
        var r4 = new Ip4Range(new Ip4Address(40), new Ip4Address(50)); // disjoint
        Span<Ip4Range> elements = [r1, r2, r3, r4];
        var set = new Ip4RangeSetStackAlloc(buffer, elements);
        var result = set.ToReadOnlySpan();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
        Assert.Equal(new Ip4Address(40), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), result[1].LastAddress);
    }

    #endregion

    #region Union1 Tests

    [Fact]
    public void Union1_EmptySets_ReturnsEmpty()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, default);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, default);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union1(ref result, set2);
        Assert.Equal(0, result.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Union1_DisjointRanges_ConcatenatesSorted()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[2];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union1(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(r1, ranges[0]);
        Assert.Equal(r2, ranges[1]);
    }

    [Fact]
    public void Union1_OverlappingRanges_Merges()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union1(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(1, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), ranges[0].LastAddress);
    }

    [Fact]
    public void Union1_AdjacentRanges_Merges()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union1(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(1, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    #endregion

    #region Union2 Tests

    [Fact]
    public void Union2_EmptySets_ReturnsEmpty()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, default);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, default);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union2(ref result, set2);
        Assert.Equal(0, result.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Union2_DisjointRanges_ConcatenatesSorted()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[2];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union2(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(r1, ranges[0]);
        Assert.Equal(r2, ranges[1]);
    }

    [Fact]
    public void Union2_OverlappingRanges_Merges()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[2];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Union2(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(1, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), ranges[0].LastAddress);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_EmptySets_ReturnsEmpty()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var set1 = new Ip4RangeSetStackAlloc(buffer1, default);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, default);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Except(ref result, set2);
        Assert.Equal(0, result.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Except_Disjoint_NoChange()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(30), new Ip4Address(40));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Except(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(1, ranges.Length);
        Assert.Equal(r1, ranges[0]);
    }

    [Fact]
    public void Except_CompleteOverlap_ReturnsEmpty()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r1]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Except(ref result, set2);
        Assert.Equal(0, result.ToReadOnlySpan().Length);
    }

    [Fact]
    public void Except_PartialOverlap_Truncates()
    {
        Span<Ip4Range> buffer1 = stackalloc Ip4Range[1];
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        var set1 = new Ip4RangeSetStackAlloc(buffer1, [r1]);
        Span<Ip4Range> buffer2 = stackalloc Ip4Range[1];
        var r2 = new Ip4Range(new Ip4Address(20), new Ip4Address(40));
        var set2 = new Ip4RangeSetStackAlloc(buffer2, [r2]);
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[1];
        Ip4RangeSetStackAlloc result = new Ip4RangeSetStackAlloc(resultBuffer);
        set1.Except(ref result, set2);
        var ranges = result.ToReadOnlySpan();
        Assert.Equal(1, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
    }

    #endregion
}