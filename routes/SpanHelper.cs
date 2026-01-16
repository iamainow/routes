using routes.Generic;

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

            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
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
            return MakeNormalizedFromSorted(result);
        }

        if (sorted2.Length == 0)
        {
            sorted1.CopyTo(result);
            return MakeNormalizedFromSorted(result);
        }

        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);
        int index1 = 0;
        int index2 = 0;

        // first pass
        {
            var item1 = sorted1[0];
            var item2 = sorted2[0];
            if (item1.FirstAddress <= item2.FirstAddress)
            {
                resultList.Add(item1);
                index1++;
            }
            else
            {
                resultList.Add(item2);
                index2++;
            }
        }

        while (index1 < sorted1.Length && index2 < sorted2.Length)
        {
            Ip4Range current;
            var item1 = sorted1[index1];
            var item2 = sorted2[index2];
            if (item1.FirstAddress <= item2.FirstAddress)
            {
                current = item1;
                index1++;
            }
            else
            {
                current = item2;
                index2++;
            }

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
            }
            else
            {
                resultList.Add(current);
            }
        }

        while (index2 < sorted2.Length)
        {
            Ip4Range current = sorted2[index2];
            index2++;

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else
            {
                if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        while (index1 < sorted1.Length)
        {
            Ip4Range current = sorted1[index1];
            index1++;

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else
            {
                if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        return resultList.Count;
    }

    public static int UnionNormalizedNormalized(ReadOnlySpan<Ip4Range> normalized1, ReadOnlySpan<Ip4Range> normalized2, Span<Ip4Range> result)
    {
        if (result.Overlaps(normalized1))
        {
            throw new ArgumentException($"result can't overlap with sorted1", nameof(result));
        }

        if (result.Overlaps(normalized2))
        {
            throw new ArgumentException("result can't overlap with sorted2", nameof(result));
        }

        if (normalized1.Length == 0)
        {
            normalized2.CopyTo(result);
            return normalized2.Length;
        }

        if (normalized2.Length == 0)
        {
            normalized1.CopyTo(result);
            return normalized1.Length;
        }

        ListStackAlloc<Ip4Range> resultList = new ListStackAlloc<Ip4Range>(result);
        int index1 = 0;
        int index2 = 0;

        // first pass
        {
            var item1 = normalized1[0];
            var item2 = normalized2[0];
            if (item1.FirstAddress <= item2.FirstAddress)
            {
                resultList.Add(item1);
                index1++;
            }
            else
            {
                resultList.Add(item2);
                index2++;
            }
        }

        while (index1 < normalized1.Length && index2 < normalized2.Length)
        {
            Ip4Range current;
            var item1 = normalized1[index1];
            var item2 = normalized2[index2];
            if (item1.FirstAddress <= item2.FirstAddress)
            {
                current = item1;
                index1++;
            }
            else
            {
                current = item2;
                index2++;
            }

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else
            {
                if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        while (index2 < normalized2.Length)
        {
            Ip4Range current = normalized2[index2];
            index2++;

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else
            {
                if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
                }
                else
                {
                    resultList.Add(current);
                    resultList.AddRange(normalized2[index2..]);
                    return resultList.Count;
                }
            }
        }

        while (index1 < normalized1.Length)
        {
            Ip4Range current = normalized1[index1];
            index1++;

            ref var last = ref resultList.Last();
            if (last.LastAddress == Ip4Address.MaxValue)
            {
                last = new Ip4Range(last.FirstAddress, Ip4Address.MaxValue);
                return resultList.Count;
            }
            else
            {
                if (last.LastAddress.ToUInt32() + 1U >= current.FirstAddress.ToUInt32())
                {
                    last = new Ip4Range(last.FirstAddress, Ip4Address.Max(last.LastAddress, current.LastAddress));
                }
                else
                {
                    resultList.Add(current);
                    resultList.AddRange(normalized1[index1..]);
                    return resultList.Count;
                }
            }
        }

        return resultList.Count;
    }

    public static int UnionNormalizedSortedViaNormalizedNormalized(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[sorted.Length];
        sorted.CopyTo(temp);
        int length = MakeNormalizedFromSorted(temp);
        return UnionNormalizedNormalized(normalized, temp[..length], result);
    }

    public static int UnionNormalizedSortedViaSortedSorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> sorted, Span<Ip4Range> result)
    {
        return UnionSortedSorted(normalized, sorted, result);
    }

    public static int UnionNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        int length = MakeNormalizedFromUnsorted(temp);
        return UnionNormalizedNormalized(normalized, temp[..length], result);
    }

    public static int UnionSortedUnsorted(ReadOnlySpan<Ip4Range> sorted, ReadOnlySpan<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp = stackalloc Ip4Range[unsorted.Length];
        unsorted.CopyTo(temp);
        temp.Sort(Ip4RangeComparer.Instance);
        return UnionSortedSorted(sorted, temp, result);
    }

    public static int UnionUnsortedUnsortedViaSortedSorted(ReadOnlySpan<Ip4Range> unsorted1, ReadOnlySpan<Ip4Range> unsorted2, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp1 = stackalloc Ip4Range[unsorted1.Length];
        unsorted1.CopyTo(temp1);
        temp1.Sort(Ip4RangeComparer.Instance);

        Span<Ip4Range> temp2 = stackalloc Ip4Range[unsorted2.Length];
        unsorted2.CopyTo(temp2);
        temp2.Sort(Ip4RangeComparer.Instance);

        return UnionSortedSorted(temp1, temp2, result);
    }

    public static int UnionUnsortedUnsortedViaNormalizedNormalized(ReadOnlySpan<Ip4Range> unsorted1, ReadOnlySpan<Ip4Range> unsorted2, Span<Ip4Range> result)
    {
        Span<Ip4Range> temp1 = stackalloc Ip4Range[unsorted1.Length];
        unsorted1.CopyTo(temp1);
        int length1 = MakeNormalizedFromUnsorted(temp1);

        Span<Ip4Range> temp2 = stackalloc Ip4Range[unsorted2.Length];
        unsorted2.CopyTo(temp2);
        int length2 = MakeNormalizedFromUnsorted(temp2);

        return UnionSortedSorted(temp1[..length1], temp2[..length2], result);
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
        Ip4Range currentRange = normalized[0];

        while (true)
        {
            if (j >= sorted.Length)
            {
                // No more exclusion ranges, add current and remaining ranges
                resultList.Add(currentRange);
                i++;
                resultList.AddRange(normalized[i..]);
                break;
            }

            var otherCurr = sorted[j];

            if (currentRange.LastAddress < otherCurr.FirstAddress)
            {
                // Current range is entirely before exclusion range - keep it and move to next
                resultList.Add(currentRange);
                i++;
                if (i >= normalized.Length)
                {
                    break;
                }
                currentRange = normalized[i];
            }
            else if (currentRange.FirstAddress > otherCurr.LastAddress)
            {
                // Current range is entirely after exclusion range - move to next exclusion
                j++;
            }
            else
            {
                // Ranges overlap - compute the difference
                (var leftPart, var rightPart) = currentRange.IntersectableExcept(otherCurr);

                if (leftPart.HasValue)
                {
                    resultList.Add(leftPart.Value);
                }

                if (rightPart.HasValue)
                {
                    currentRange = rightPart.Value;
                    j++;
                }
                else
                {
                    i++;
                    if (i >= normalized.Length)
                    {
                        break;
                    }
                    currentRange = normalized[i];
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
        int length = MakeNormalizedFromUnsorted(temp);
        return ExceptNormalizedNormalized(normalized, temp[..length], result);
    }

    public static int ExceptNormalizedUnsorted(ReadOnlySpan<Ip4Range> normalized, Span<Ip4Range> unsorted, Span<Ip4Range> result)
    {
        unsorted.Sort(Ip4RangeComparer.Instance);
        return ExceptNormalizedSorted(normalized, unsorted, result);
    }
}