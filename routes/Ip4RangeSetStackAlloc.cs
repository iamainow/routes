namespace routes;

public ref struct Ip4RangeSetStackAlloc
{
    private ListStackAlloc<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _ranges.AsReadOnlySpan();

    public readonly int RangesCount => _ranges.Count;

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer)
    {
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer);
    }

    /// <param name="elements">span of values, values may be unsorted, may overlapping, may adjacent/disjoint</param>
    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer, scoped ReadOnlySpan<Ip4Range> elements)
    {
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer);

        Span<Ip4Range> temp = stackalloc Ip4Range[elements.Length];
        elements.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);

        // here temp sorted, but may overlap or be adjacent/disjoint

        if (temp.Length > 0)
        {
            _ranges.Add(temp[0]);
            for (int i = 1; i < temp.Length; i++)
            {
                var current = temp[i];
                ref var last = ref _ranges.Last();

                if (last.LastAddress.ToUInt32() + 1UL >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, current.LastAddress);
                }
                else
                {
                    _ranges.Add(current);
                }
            }
        }
    }

    public void Union(Ip4RangeSetStackAlloc other)
    {
        SmartUnionSorted(other.ToReadOnlySpan());
    }

    public void Union2ModifySpan(scoped Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        SmartUnionSorted(other);
    }

    public void SmartUnionUnorderedModifySpan(scoped Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        SmartUnionSorted(other);
    }

    // merge sorted arrays into some temp span and then in the end copy to this._range
    private void SmartUnionSorted(scoped ReadOnlySpan<Ip4Range> other)
    {
        ListStackAlloc<Ip4Range> temp = new ListStackAlloc<Ip4Range>(stackalloc Ip4Range[_ranges.Count + other.Length]);
        int i = 0;
        int j = 0;
        while (i < _ranges.Count || j < other.Length)
        {
            Ip4Range curr;
            if (i >= _ranges.Count)
            {
                curr = other[j++];
            }
            else if (j >= other.Length)
            {
                curr = _ranges[i++];
            }
            else
            {
                var left = _ranges[i];
                var right = other[j];
                if (left.FirstAddress.ToUInt32() <= right.FirstAddress.ToUInt32())
                {
                    curr = left;
                    i++;
                }
                else
                {
                    curr = right;
                    j++;
                }
            }
            if (temp.Count == 0)
            {
                temp.Add(curr);
            }
            else
            {
                ref var last = ref temp.Last();
                if (last.LastAddress.ToUInt32() == uint.MaxValue)
                {
                    last = new Ip4Range(last.FirstAddress, new Ip4Address(uint.MaxValue));
                    break;
                }
                if (last.LastAddress.ToUInt32() + 1UL >= curr.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, new Ip4Address(Math.Max(last.LastAddress.ToUInt32(), curr.LastAddress.ToUInt32())));
                }
                else
                {
                    temp.Add(curr);
                }
            }
        }

        _ranges.Clear();
        for (int k = 0; k < temp.Count; k++)
        {
            _ranges.Add(temp.AsSpan()[k]);
        }
    }

    private static int CalcExceptBufferSize(int left, int right)
    {
        return (left + right);
    }

    public void ExceptModifySpan(scoped Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        InternalExceptSorted(other);
    }

    private void InternalExceptSorted(scoped ReadOnlySpan<Ip4Range> other)
    {
        if (_ranges.Count == 0 || other.Length == 0)
            return;

        Span<Ip4Range> temp = stackalloc Ip4Range[CalcExceptBufferSize(_ranges.Count, other.Length)];
        int count = 0;

        int i = 0;
        int j = 0;

        while (i < _ranges.Count)
        {
            if (j >= other.Length)
            {
                // Add remaining ranges from this set
                while (i < _ranges.Count)
                {
                    temp[count++] = _ranges[i++];
                }
                break;
            }

            var curr = _ranges[i];
            var otherCurr = other[j];

            if (curr.LastAddress.ToUInt32() < otherCurr.FirstAddress.ToUInt32())
            {
                temp[count++] = curr;
                i++;
            }
            else if (curr.FirstAddress.ToUInt32() > otherCurr.LastAddress.ToUInt32())
            {
                j++;
            }
            else
            {
                var excepted = curr.IntersectableExcept(otherCurr);
                foreach (var ex in excepted)
                {
                    temp[count++] = ex;
                }
                i++;
            }
        }

        _ranges.Clear();
        for (int k = 0; k < count; k++)
        {
            _ranges.Add(temp[k]);
        }
    }
}