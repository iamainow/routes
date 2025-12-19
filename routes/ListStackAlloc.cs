namespace routes;

public ref struct ListStackAlloc<T>
{
    private Span<T> _items;
    private int _size;

    public ListStackAlloc(Span<T> buffer)
    {
        _items = buffer;
        _size = 0;
    }

    public readonly int Count => _size;

    public readonly int Capacity => _items.Length;

    public readonly T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _items[index];
        }
    }

    public readonly ReadOnlySpan<T> this[Range range]
    {
        get
        {
            return AsReadOnlySpan()[range];
        }
    }

    public void Add(T item)
    {
        if (Count + 1 > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        _items[_size] = item;
        ++_size;
    }

    public void AddRange(Span<T> items)
    {
        if (Count + items.Length > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        items.CopyTo(_items[Count..]);
        _size += items.Length;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        if (Count + items.Length > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        items.CopyTo(_items[Count..]);
        _size += items.Length;
    }

    public void AddRange(ListStackAlloc<T> items)
    {
        AddRange(items.AsReadOnlySpan());
    }

    public void RemoveLast()
    {
        if (Count < 1)
        {
            throw new InvalidOperationException("No items to remove");
        }
        --_size;
    }

    public void RemoveLast(int count)
    {
        if (Count < count)
        {
            throw new InvalidOperationException("Not enough items to remove");
        }
        _size -= count;
    }

    public void RemoveRegion(int start, int count)
    {
        AsReadOnlySpan()[(start + count)..].CopyTo(_items[start..]);
        _size -= count;
    }

    public void RemoveRegion(Range range)
    {
        (int start, int count) = range.GetOffsetAndLength(Count);
        RemoveRegion(start, count);
    }

    public void Sort<TComparer>(Comparison<T> comparer)
    {
        AsSpan().Sort(comparer);
    }

    public readonly Span<T> AsSpan()
    {
        return _items[.._size];
    }

    public readonly ReadOnlySpan<T> AsReadOnlySpan()
    {
        return _items[.._size];
    }

    public readonly T[] ToArray()
    {
        return AsSpan().ToArray();
    }
}