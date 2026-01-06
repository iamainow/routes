namespace routes;

public ref struct Ip4RangeSetStackAlloc
{
    private ListStackAlloc<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _ranges.AsReadOnlySpan();

    public readonly int RangesCount => _ranges.Count;

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer, int count = 0)
    {
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer, count);
    }

    /// <param name="elements">span of values, values may be unsorted, may overlapping, may adjacent/disjoint</param>
    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer, scoped ReadOnlySpan<Ip4Range> elements)
    {
        elements.CopyTo(rewritableInternalBuffer);
        int length = SpanHelper.MakeNormalizedFromUnsorted(rewritableInternalBuffer[..elements.Length]);
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer, length);
    }

    public void Union(scoped Ip4RangeSetStackAlloc other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.RangesCount];
        int length = SpanHelper.UnionNormalizedNormalized(_ranges.AsReadOnlySpan(), other.ToReadOnlySpan(), temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }

    public void Union(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.Length];
        int length = SpanHelper.UnionNormalizedUnsorted(_ranges.AsReadOnlySpan(), other, temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }

    public void Union(scoped Span<Ip4Range> other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.Length];
        int length = SpanHelper.UnionNormalizedUnsorted(_ranges.AsReadOnlySpan(), other, temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }

    public void Except(scoped Ip4RangeSetStackAlloc other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.RangesCount];
        int length = SpanHelper.ExceptNormalizedNormalized(_ranges.AsReadOnlySpan(), other.ToReadOnlySpan(), temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }

    public void Except(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.Length];
        int length = SpanHelper.ExceptNormalizedUnsorted(_ranges.AsReadOnlySpan(), other, temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }

    public void Except(scoped Span<Ip4Range> other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[this.RangesCount + other.Length];
        int length = SpanHelper.ExceptNormalizedUnsorted(_ranges.AsReadOnlySpan(), other, temp);
        _ranges.Clear();
        _ranges.AddRange(temp[..length]);
    }
}