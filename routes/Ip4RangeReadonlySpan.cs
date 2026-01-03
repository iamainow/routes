namespace routes;

public readonly ref struct Ip4RangeReadonlySpan
{
    private readonly ReadOnlySpan<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public readonly int RangesCount => _ranges.Length;

    public Ip4RangeReadonlySpan(Span<Ip4Range> ranges)
    {
        _ranges = ranges;
    }

    public Ip4RangeReadonlySpan(ReadOnlySpan<Ip4Range> ranges)
    {
        _ranges = ranges;
    }

    public Ip4RangeReadonlySpan()
    {
        _ranges = ReadOnlySpan<Ip4Range>.Empty;
    }

    public int CalcUnionBuffer(int otherLength)
    {
        return (_ranges.Length + otherLength);
    }
    public int Union(scoped Span<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        other.Sort(Ip4RangeComparer.Instance);
        return UnionSorted(other, result);
    }
    public int Union(scoped ReadOnlySpan<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[other.Length];
        other.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return UnionSorted(temp, result);
    }
    private int UnionSorted(scoped ReadOnlySpan<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);

        int i = 0;
        int j = 0;
        while (i < _ranges.Length || j < other.Length)
        {
            Ip4Range curr;
            if (i >= _ranges.Length)
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
            if (resultList.Count == 0)
            {
                resultList.Add(curr);
            }
            else
            {
                ref var last = ref resultList.Last();
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
                    resultList.Add(curr);
                }
            }
        }

        return resultList.Count;

    }

    public int CalcExceptBuffer(int otherLength)
    {
        return (_ranges.Length + otherLength);
    }
    public int Except(scoped Span<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        other.Sort(Ip4RangeComparer.Instance);
        return ExceptSorted(other, result);
    }
    public int Except(scoped ReadOnlySpan<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[other.Length];
        other.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return ExceptSorted(temp, result);
    }
    private int ExceptSorted(scoped ReadOnlySpan<Ip4Range> other, scoped Span<Ip4Range> result)
    {
        if (_ranges.Length == 0 || other.Length == 0)
            return 0;

        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);
        int i = 0;
        int j = 0;
        while (i < _ranges.Length)
        {
            if (j >= other.Length)
            {
                // Add remaining ranges from this set
                while (i < _ranges.Length)
                {
                    resultList.Add(_ranges[i++]);
                }
                break;
            }
            var curr = _ranges[i];
            var otherCurr = other[j];
            if (curr.LastAddress.ToUInt32() < otherCurr.FirstAddress.ToUInt32())
            {
                resultList.Add(curr);
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
                    resultList.Add(ex);
                }
                i++;
            }
        }
        return resultList.Count;
    }
}
