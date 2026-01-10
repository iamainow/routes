namespace routes;

public readonly ref struct Ip4RangeSetStackAlloc2
{
    public static Ip4RangeSetStackAlloc2 Create(scoped Ip4RangeSetStackAlloc2 other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[other._items.Length];
        other._items.CopyTo(resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer);
    }

    public static Ip4RangeSetStackAlloc2 Create(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[other.Length];
        int length = SpanHelper.MakeNormalizedFromUnsorted(resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer[..length]);
    }

    private readonly ReadOnlySpan<Ip4Range> _items; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _items;

    public readonly int RangesCount => _items.Length;

    public Ip4RangeSetStackAlloc2()
    {
        _items = ReadOnlySpan<Ip4Range>.Empty;
    }

    private Ip4RangeSetStackAlloc2(ReadOnlySpan<Ip4Range> normalizedItems)
    {
        _items = normalizedItems;
    }

    public readonly Ip4RangeSetStackAlloc2 Union(scoped Ip4RangeSetStackAlloc2 other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[this._items.Length + other._items.Length];
        int length = SpanHelper.UnionNormalizedNormalized(_items, other._items, resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer[..length]);
    }

    public readonly Ip4RangeSetStackAlloc2 Union(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[this._items.Length + other.Length];
        int length = SpanHelper.UnionNormalizedUnsorted(_items, other, resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer[..length]);
    }

    public Ip4RangeSetStackAlloc2 Except(scoped Ip4RangeSetStackAlloc2 other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[_items.Length + other._items.Length];
        int length = SpanHelper.ExceptNormalizedNormalized(_items, other._items, resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer[..length]);
    }

    public Ip4RangeSetStackAlloc2 Except(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[_items.Length + other.Length];
        int length = SpanHelper.ExceptNormalizedUnsorted(_items, other, resultBuffer);
        return new Ip4RangeSetStackAlloc2(resultBuffer[..length]);
    }
}