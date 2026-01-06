namespace routes;

// unsorted - unsorted, overlapping/adjacent
// sorted - sorted but overlapping/adjacent
// normalized - sorted, not oveplaped, non-adjacent
public static class SpanHelper
{
    public static int MakeNormalizedFromSorted(ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> result)
    {
        var resultList = new ListStackAlloc<Ip4Range>(result);

        if (sorted.Length > 0)
        {
            resultList.Add(sorted[0]);
            for (int i = 1; i < sorted.Length; i++)
            {
                var current = sorted[i];
                ref var last = ref resultList.Last();

                if (last.LastAddress.ToUInt32() + 1UL >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, current.LastAddress);
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        return resultList.Count;
    }

    public static int MakeNormalizedFromUnsorted(Span<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        unsorted.Sort(Ip4RangeComparer.Instance);
        return MakeNormalizedFromSorted(unsorted, result);
    }

    public static int MakeNormalizedFromUnsorted(ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return MakeNormalizedFromSorted(temp, result);
    }

    public static int UnionSortedSorted(ReadOnlySpan<Ip4Range> sorted1, ReadOnlySpan<Ip4Range> sorted2, Span<Ip4Range> result)
    {
        ListStackAlloc<Ip4Range> temp = new ListStackAlloc<Ip4Range>(stackalloc Ip4Range[sorted1.Length + sorted2.Length]);
        int i = 0;
        int j = 0;
        while (i < sorted1.Length || j < sorted2.Length)
        {
            Ip4Range curr;
            if (i >= sorted1.Length)
            {
                curr = sorted2[j++];
            }
            else if (j >= sorted2.Length)
            {
                curr = sorted1[i++];
            }
            else
            {
                var left = sorted1[i];
                var right = sorted2[j];
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
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, curr.LastAddress));
                }
                else
                {
                    temp.Add(curr);
                }
            }
        }

        temp.AsReadOnlySpan().CopyTo(result);
        return temp.Count;
    }

    public static int UnionNormalizedNormalized(ReadOnlySpan<Ip4Range> normalized1, ReadOnlySpan<Ip4Range> normalized2, Span<Ip4Range> result)
    {
        return UnionSortedSorted(normalized1, normalized2, result);
    }

    public static int UnionNormalizedSorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> result)
    {
        return UnionSortedSorted(normalized, sorted, result);
    }

    public static int UnionNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return UnionNormalizedSorted(normalized, temp, result);
    }

    public static int UnionNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, Span<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        unsorted.Sort(Ip4RangeComparer.Instance);
        return UnionSortedSorted(normalized, unsorted, result);
    }
}
