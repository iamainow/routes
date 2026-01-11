using System.Runtime.InteropServices;
using System.Text;

namespace routes;

public readonly ref struct Ip4RangeArray
{
    public static Ip4RangeArray Create(scoped Ip4RangeArray other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[other._items.Length];
        other._items.CopyTo(resultBuffer);
        return new Ip4RangeArray(resultBuffer);
    }

    public static Ip4RangeArray Create(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[other.Length];
        int length = SpanHelper.MakeNormalizedFromUnsorted(resultBuffer);
        return new Ip4RangeArray(resultBuffer[..length]);
    }

    private readonly ReadOnlySpan<Ip4Range> _items; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _items;

    public readonly int RangesCount => _items.Length;

    public Ip4RangeArray()
    {
        _items = ReadOnlySpan<Ip4Range>.Empty;
    }

    private Ip4RangeArray(ReadOnlySpan<Ip4Range> normalizedItems)
    {
        _items = normalizedItems;
    }

    public readonly Ip4RangeArray Union(scoped Ip4RangeArray other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[this._items.Length + other._items.Length];
        int length = SpanHelper.UnionNormalizedNormalized(_items, other._items, resultBuffer);
        return new Ip4RangeArray(resultBuffer[..length]);
    }

    public readonly Ip4RangeArray Union(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[this._items.Length + other.Length];
        int length = SpanHelper.UnionNormalizedUnsorted(_items, other, resultBuffer);
        return new Ip4RangeArray(resultBuffer[..length]);
    }

    public Ip4RangeArray Except(scoped Ip4RangeArray other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[_items.Length + other._items.Length];
        int length = SpanHelper.ExceptNormalizedNormalized(_items, other._items, resultBuffer);
        return new Ip4RangeArray(resultBuffer[..length]);
    }

    public Ip4RangeArray Except(scoped ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> resultBuffer = new Ip4Range[_items.Length + other.Length];
        int length = SpanHelper.ExceptNormalizedUnsorted(_items, other, resultBuffer);
        return new Ip4RangeArray(resultBuffer[..length]);
    }

    public Ip4RangeArray MinimizeSubnets(uint delta)
    {
        List<Ip4Range> result = new();
        foreach (Ip4Range range in _items)
        {
            if (range.Count >= delta)
            {
                result.Add(range);
            }
        }
        return new Ip4RangeArray(CollectionsMarshal.AsSpan(result));
    }

    public Ip4Subnet[] ToIp4Subnets()
    {
        List<Ip4Subnet> result = new();
        foreach (var item in _items)
        {
            result.AddRange(item.ToSubnets());
        }

        return result.ToArray();
    }

    public Ip4Range[] ToIp4Ranges()
    {
        Ip4Range[] result = new Ip4Range[_items.Length];
        _items.CopyTo(result);
        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (Ip4Range item in _items)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}