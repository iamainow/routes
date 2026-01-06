namespace routes;

// unsorted - unsorted, overlapping/adjacent
// sorted - sorted but overlapping/adjacent
// normalized - sorted, not overlapped, non-adjacent
public static class SpanHelper
{
    public static int MakeNormalizedFromSorted(Span<Ip4Range> result)
    {
        if (result.Length <= 1) return result.Length;

        var resultList = new ListStackAlloc<Ip4Range>(result, 1);

        for (int i = 1; i < result.Length; i++)
        {
            var current = result[i];
            ref var last = ref resultList.Last();

            if (last.LastAddress.ToUInt32() + 1UL >= current.FirstAddress.ToUInt32())
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
            }
            else
            {
                resultList.Add(current);
            }
        }

        return resultList.Count;
    }

    public static int MakeNormalizedFromUnsorted(Span<Ip4Range> result)
    {
        result.Sort(Ip4RangeComparer.Instance);
        return MakeNormalizedFromSorted(result);
    }



    public static int UnionSortedSorted(ReadOnlySpan<Ip4Range> sorted1, ReadOnlySpan<Ip4Range> sorted2, Span<Ip4Range> result)
    {
        if (result.Overlaps(sorted1))
        {
            throw new ArgumentException($"result can't overlap with sorted1", nameof(result));
        }

        if (result.Overlaps(sorted2))
        {
            throw new ArgumentException("result can't overlap with sorted2", nameof(result));
        }

        if (sorted1.Length == 0)
        {
            sorted2.CopyTo(result);
            return sorted2.Length;
        }

        if (sorted2.Length == 0)
        {
            sorted1.CopyTo(result);
            return sorted1.Length;
        }

        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);
        int i = 0;
        int j = 0;

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

            resultList.Add(curr);
        }

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

            ref var last = ref resultList.Last();
            if (last.LastAddress.ToUInt32() + 1UL >= curr.FirstAddress.ToUInt32())
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, curr.LastAddress));
            }
            else
            {
                resultList.Add(curr);
            }
        }

        return resultList.Count;
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
        return UnionNormalizedSorted(normalized, unsorted, result);
    }

    public static int UnionSortedUnsorted(ReadOnlySpan<Ip4Range> sorted, ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return UnionSortedSorted(sorted, temp, result);
    }

    public static int UnionSortedUnsorted(ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        unsorted.Sort(Ip4RangeComparer.Instance);
        return UnionSortedSorted(sorted, unsorted, result);
    }

    public static int UnionUnsortedUnsorted(ReadOnlySpan<Ip4Range> unsorted1, ReadOnlySpan<Ip4Range> unsorted2, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp1 = stackalloc Ip4Range[unsorted1.Length];
        unsorted1.CopyTo(temp1);
        temp1.Sort(Ip4RangeComparer.Instance);

        Span<Ip4Range> temp2 = stackalloc Ip4Range[unsorted2.Length];
        unsorted2.CopyTo(temp2);
        temp2.Sort(Ip4RangeComparer.Instance);

        return UnionSortedSorted(temp1, temp2, result);
    }

    public static int UnionUnsortedUnsorted(ReadOnlySpan<Ip4Range> unsorted1, Span<Ip4Range> unsorted2, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp1 = stackalloc Ip4Range[unsorted1.Length];
        unsorted1.CopyTo(temp1);
        temp1.Sort(Ip4RangeComparer.Instance);

        unsorted2.Sort(Ip4RangeComparer.Instance);

        return UnionSortedSorted(temp1, unsorted2, result);
    }

    public static int UnionUnsortedUnsorted(Span<Ip4Range> unsorted1, Span<Ip4Range> unsorted2, Span<Ip4Range> result)
    {
        unsorted1.Sort(Ip4RangeComparer.Instance);

        unsorted2.Sort(Ip4RangeComparer.Instance);

        return UnionSortedSorted(unsorted1, unsorted2, result);
    }



    public static int ExceptNormalizedSorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> result)
    {
        if (result.Overlaps(normalized))
        {
            throw new ArgumentException($"result can't overlap with normalized", nameof(result));
        }

        if (result.Overlaps(sorted))
        {
            throw new ArgumentException("result can't overlap with sorted", nameof(result));
        }

        if (normalized.Length == 0)
        {
            return 0;
        }

        if (sorted.Length == 0)
        {
            normalized.CopyTo(result);
            return normalized.Length;
        }

        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);

        int i = 0;
        int j = 0;
        Ip4Range? curr = normalized[i];

        while (curr.HasValue)
        {
            if (j >= sorted.Length)
            {
                // No more exclusion ranges, add current and remaining ranges
                resultList.Add(curr.Value);
                i++;
                while (i < normalized.Length)
                {
                    resultList.Add(normalized[i++]);
                }
                break;
            }

            var currentRange = curr.Value;
            var otherCurr = sorted[j];

            if (currentRange.LastAddress.ToUInt32() < otherCurr.FirstAddress.ToUInt32())
            {
                // Current range is entirely before exclusion range - keep it and move to next
                resultList.Add(currentRange);
                i++;
                curr = i < normalized.Length ? normalized[i] : null;
            }
            else if (currentRange.FirstAddress.ToUInt32() > otherCurr.LastAddress.ToUInt32())
            {
                // Current range is entirely after exclusion range - move to next exclusion
                j++;
            }
            else
            {
                // Ranges overlap - compute the difference
                (bool hasLeftPart, bool hasRightPart) = currentRange.IntersectableExcept(otherCurr, out var leftPart, out var rightPart);

                if (hasLeftPart)
                {
                    if (hasRightPart)
                    {
                        // Two parts: left part is finalized, right part needs further processing
                        resultList.Add(leftPart);
                        curr = rightPart;
                        j++;
                    }
                    else
                    {
                        // Left part only - it ends before the exclusion starts, so it's finalized
                        resultList.Add(leftPart);
                        i++;
                        curr = i < normalized.Length ? normalized[i] : null;
                    }
                }
                else
                {
                    if (hasRightPart)
                    {
                        // Right part only - it starts after exclusion ends, may overlap with next exclusions
                        curr = rightPart;
                        j++;
                    }
                    else
                    {
                        // Current range completely covered by exclusion - move to next normalized range
                        i++;
                        curr = i < normalized.Length ? normalized[i] : null;
                    }
                }
            }
        }

        return resultList.Count;
    }

    public static int ExceptNormalizedNormalized(ReadOnlySpan<Ip4Range> normalized1, ReadOnlySpan<Ip4Range> normalized2, Span<Ip4Range> result)
    {
        return ExceptNormalizedSorted(normalized1, normalized2, result);
    }

    public static int ExceptNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return ExceptNormalizedSorted(normalized, temp, result);
    }

    public static int ExceptNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, Span<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        unsorted.Sort(Ip4RangeComparer.Instance);
        return ExceptNormalizedSorted(normalized, unsorted, result);
    }
}