namespace routes;

public ref struct Ip4RangeSetStackAlloc
{
    private ListStackAlloc<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _ranges.AsReadOnlySpan();

    public ReadOnlySpan<Ip4Range> ToSpan() => _ranges.AsSpan();

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer)
    {
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer);
    }

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer, ReadOnlySpan<Ip4Range> elements) // elements may be unsorted, may overlapping, may adjacent/disjoint
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
                /*
                condition should be if (last.FirstAddress (1) <= current.LastAddress (3) && last.LastAddress >= current.FirstAddress)
                lets current.FirstAddress = (2)
                but (1) <= (2) due sorting 
                and (2) <= (3) due FirstAddress <= LastAddress
                so when (1) <= (2) <= (3) then (1) <= (3)
                it means that last.FirstAddress (1) <= current.LastAddress (3) is always true
                so original condition equals to if (true && last.LastAddress >= current.FirstAddress)
                and then equals to if (last.LastAddress >= current.FirstAddress)
                */
                if (last.LastAddress.ToUInt32() + 1 >= current.FirstAddress.ToUInt32())
                {
                    var merged = new Ip4Range(last.FirstAddress, current.LastAddress);
                    last = merged;
                }
                else
                {
                    _ranges.Add(current);
                }
            }
        }
    }

    /// <summary>
    /// dumb way to do union - sorting already sorted _ranges
    /// </summary>
    /// <param name="other"></param>
    public void Union1(Ip4RangeSetStackAlloc other)
    {
        Span<Ip4Range> thisAndOtherSorted = stackalloc Ip4Range[_ranges.Count + other._ranges.Count];

        _ranges.AsReadOnlySpan().CopyTo(thisAndOtherSorted);
        other._ranges.AsReadOnlySpan().CopyTo(thisAndOtherSorted[_ranges.Count..]);

        thisAndOtherSorted.Sort(Ip4RangeComparer.Instance);

        this._ranges.Clear();

        if (thisAndOtherSorted.Length > 0)
        {
            this._ranges.Add(thisAndOtherSorted[0]);
            for (int i = 1; i < thisAndOtherSorted.Length; i++)
            {
                var current = thisAndOtherSorted[i];
                ref var last = ref this._ranges.Last();
                if (last.LastAddress.ToUInt32() == uint.MaxValue)
                {
                    last = new Ip4Range(last.FirstAddress, new Ip4Address(uint.MaxValue));
                    return;
                }
                else if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    var maxLast = Math.Max(last.LastAddress.ToUInt32(), current.LastAddress.ToUInt32());
                    last = new Ip4Range(last.FirstAddress, new Ip4Address(maxLast));
                }
                else
                {
                    this._ranges.Add(current);
                }
            }
        }
    }

    public void Union2(Ip4RangeSetStackAlloc other)
    {
        SmartUnionSorted(other.ToReadOnlySpan());
    }

    public void Union1(ReadOnlySpan<Ip4Range> other)
    {
        Span<Ip4Range> thisAndOtherSorted = stackalloc Ip4Range[_ranges.Count + other.Length];

        _ranges.AsReadOnlySpan().CopyTo(thisAndOtherSorted);
        other.CopyTo(thisAndOtherSorted[_ranges.Count..]);

        thisAndOtherSorted.Sort(Ip4RangeComparer.Instance);

        this._ranges.Clear();

        if (thisAndOtherSorted.Length > 0)
        {
            this._ranges.Add(thisAndOtherSorted[0]);
            for (int i = 1; i < thisAndOtherSorted.Length; i++)
            {
                var current = thisAndOtherSorted[i];
                ref var last = ref this._ranges.Last();
                if (last.LastAddress.ToUInt32() + 1UL >= current.FirstAddress.ToUInt32())
                {
                    var maxLast = Math.Max(last.LastAddress.ToUInt32(), current.LastAddress.ToUInt32());
                    last = new Ip4Range(last.FirstAddress, new Ip4Address(maxLast));
                }
                else
                {
                    this._ranges.Add(current);
                }
            }
        }
    }

    public void Union2(Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        SmartUnionSorted(other);

        SmartUnionUnordered(other);
    }

    public void SmartUnionUnordered(Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        SmartUnionSorted(other);
    }

    // merge sorted arrays into some temp span and then in the end copy to this._range
    public void SmartUnionSorted(ReadOnlySpan<Ip4Range> other)
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
        return (left + right) * 2;
    }

    public void ExceptUnsorted(Span<Ip4Range> other)
    {
        other.Sort(Ip4RangeComparer.Instance);
        ExceptSorted(other);
    }

    public void ExceptSorted(ReadOnlySpan<Ip4Range> other)
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

    public int RangesCount => _ranges.Count;
}