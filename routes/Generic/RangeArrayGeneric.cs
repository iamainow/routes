using System.Numerics;
using System.Text;

namespace routes.Generic;

public readonly ref struct RangeArrayGeneric<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    public static RangeArrayGeneric<T> Create(scoped RangeArrayGeneric<T> other)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[other._items.Length];
        other._items.CopyTo(resultBuffer);
        return new RangeArrayGeneric<T>(resultBuffer);
    }

    public static RangeArrayGeneric<T> Create(scoped ReadOnlySpan<T> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[other.Length];
        int length = SpanHelperGeneric.MakeNormalizedFromUnsorted(resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length]);
    }
#pragma warning restore CA1000 // Do not declare static members on generic types

    private readonly ReadOnlySpan<CustomRange<T>> _items; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly ReadOnlySpan<CustomRange<T>> ToReadOnlySpan() => this._items;

    public readonly int RangesCount => this._items.Length;

    public RangeArrayGeneric()
    {
        this._items = ReadOnlySpan<CustomRange<T>>.Empty;
    }

    private RangeArrayGeneric(ReadOnlySpan<CustomRange<T>> normalizedItems)
    {
        this._items = normalizedItems;
    }

    public readonly RangeArrayGeneric<T> Union(scoped RangeArrayGeneric<T> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other._items.Length];
        int length = SpanHelperGeneric.UnionNormalizedNormalized(this._items, other._items, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length]);
    }

    public readonly RangeArrayGeneric<T> Union(scoped ReadOnlySpan<CustomRange<T>> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other.Length];
        int length = SpanHelperGeneric.UnionNormalizedUnsorted(this._items, other, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length]);
    }

    public RangeArrayGeneric<T> Except(scoped RangeArrayGeneric<T> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other._items.Length];
        int length = SpanHelperGeneric.ExceptNormalizedNormalized(this._items, other._items, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length]);
    }

    public RangeArrayGeneric<T> Except(scoped ReadOnlySpan<CustomRange<T>> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other.Length];
        int length = SpanHelperGeneric.ExceptNormalizedUnsorted(this._items, other, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length]);
    }

    public CustomRange<T>[] ToArray()
    {
        CustomRange<T>[] result = new CustomRange<T>[this._items.Length];
        this._items.CopyTo(result);
        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (CustomRange<T> item in this._items)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}