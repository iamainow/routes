namespace routes;

public readonly ref struct Ip4RangeSetStackAlloc
{
    private readonly ListStackAlloc<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

    public ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _ranges.AsReadOnlySpan();

    public ReadOnlySpan<Ip4Range> ToSpan() => _ranges.AsSpan();

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer)
    {
        _ranges = new ListStackAlloc<Ip4Range>();
    }

    public Ip4RangeSetStackAlloc(Span<Ip4Range> rewritableInternalBuffer, ReadOnlySpan<Ip4Range> elements) // elements may be unsorted, may overlapping, may adjacent/disjoint
    {
        _ranges = new ListStackAlloc<Ip4Range>(rewritableInternalBuffer);

        Span<Ip4Range> temp = stackalloc Ip4Range[elements.Length];
        elements.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);

        if (temp.Length > 0)
        {
            _ranges.Add(temp[0]);
            for (int i = 1; i < temp.Length; i++)
            {
                var current = temp[i];
                var last = _ranges[_ranges.Count - 1];
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
                    _ranges[_ranges.Count - 1] = merged;
                }
                else
                {
                    _ranges.Add(current);
                }
            }
        }
    }

    public static int CalcUnionBufferSize(Ip4RangeSetStackAlloc left, Ip4RangeSetStackAlloc right)
    {
        return left._ranges.Count + right._ranges.Count;
    }

    public void Union1(ref Ip4RangeSetStackAlloc result, Ip4RangeSetStackAlloc other)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[_ranges.Count + other._ranges.Count];
        _ranges.AsReadOnlySpan().CopyTo(temp);
        other._ranges.AsReadOnlySpan().CopyTo(temp.Slice(_ranges.Count));
        temp.Sort(Ip4RangeComparer.Instance);

        if (temp.Length > 0)
        {
            result._ranges.Add(temp[0]);
            for (int i = 1; i < temp.Length; i++)
            {
                var current = temp[i];
                var last = result._ranges[result._ranges.Count - 1];
                if (last.LastAddress.ToUInt32() + 1 >= current.FirstAddress.ToUInt32())
                {
                    var merged = new Ip4Range(last.FirstAddress, current.LastAddress);
                    result._ranges.RemoveLast();
                    result._ranges.Add(merged);
                }
                else
                {
                    result._ranges.Add(current);
                }
            }
        }
    }

    public void Union2(ref Ip4RangeSetStackAlloc result, Ip4RangeSetStackAlloc other)
    {
        int i = 0, j = 0;
        while (i < _ranges.Count && j < other._ranges.Count)
        {
            var current = _ranges[i];
            var currentOther = other._ranges[j];

            if (currentOther.FirstAddress > current.LastAddress)
            {
                switch ((currentOther.FirstAddress.ToUInt32() - current.LastAddress.ToUInt32()).CompareTo(1U))
                {
                    case > 0:
                        result._ranges.Add(current);
                        i++;
                        continue;
                    case 0:
                        result._ranges.Add(new Ip4Range(current.FirstAddress, currentOther.LastAddress));
                        i++;
                        j++;
                        continue;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (current.FirstAddress > currentOther.LastAddress)
            {
                switch ((current.FirstAddress.ToUInt32() - currentOther.LastAddress.ToUInt32()).CompareTo(1U))
                {
                    case > 0:
                        result._ranges.Add(currentOther);
                        j++;
                        continue;
                    case 0:
                        result._ranges.Add(new Ip4Range(currentOther.FirstAddress, current.LastAddress));
                        i++;
                        j++;
                        continue;
                    default:
                        throw new InvalidOperationException();
                }
            }

            {
                var newElement = current.IntersectableUnion(currentOther);
                result._ranges.Add(newElement);
                bool moveOther = current.LastAddress >= currentOther.LastAddress;
                if (moveOther)
                {
                    j++;
                }
                else
                {
                    i++;
                }
            }
        }

        while (i < _ranges.Count)
        {
            result._ranges.Add(_ranges[i]);
            i++;
        }

        while (j < other._ranges.Count)
        {
            result._ranges.Add(other._ranges[j]);
            j++;
        }

        // Normalize the result
        if (result._ranges.Count > 1)
        {
            Span<Ip4Range> normalized = stackalloc Ip4Range[result._ranges.Count];
            result._ranges.AsReadOnlySpan().CopyTo(normalized);
            int writeIndex = 0;
            if (normalized.Length > 0)
            {
                normalized[writeIndex++] = normalized[0];
                for (int k = 1; k < normalized.Length; k++)
                {
                    var curr = normalized[k];
                    var last = normalized[writeIndex - 1];
                    if (last.IsIntersects(curr))
                    {
                        normalized[writeIndex - 1] = last.IntersectableUnion(curr);
                    }
                    else if (last.LastAddress.ToUInt32() + 1 == curr.FirstAddress.ToUInt32())
                    {
                        normalized[writeIndex - 1] = new Ip4Range(last.FirstAddress, curr.LastAddress);
                    }
                    else
                    {
                        normalized[writeIndex++] = curr;
                    }
                }
            }
            result._ranges.RemoveRegion(0, result._ranges.Count);
            for (int idx = 0; idx < writeIndex; idx++)
            {
                result._ranges.Add(normalized[idx]);
            }
        }
    }

    public static int CalcExceptBufferSize(Ip4RangeSetStackAlloc left, Ip4RangeSetStackAlloc right)
    {
        return left._ranges.Count * right._ranges.Count;
    }

    public void Except(ref Ip4RangeSetStackAlloc result, Ip4RangeSetStackAlloc other)
    {
        int i = 0, j = 0;
        while (i < _ranges.Count && j < other._ranges.Count)
        {
            var current = _ranges[i];
            var currentOther = other._ranges[j];

            if (current.LastAddress < currentOther.FirstAddress)
            {
                result._ranges.Add(current);
                i++;
            }
            else if (current.FirstAddress > currentOther.LastAddress)
            {
                j++;
            }
            else
            {
                var excepted = current.IntersectableExcept(currentOther);
                foreach (var range in excepted)
                {
                    result._ranges.Add(range);
                }
                if (current.LastAddress <= currentOther.LastAddress)
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }
        }

        while (i < _ranges.Count)
        {
            result._ranges.Add(_ranges[i]);
            i++;
        }
    }
}