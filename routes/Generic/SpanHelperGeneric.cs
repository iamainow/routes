using System.Numerics;

namespace routes.Generic;

// unsorted - unsorted, overlapping/adjacent
// sorted - sorted but overlapping/adjacent
// normalized - sorted, not overlapped, non-adjacent
public static class SpanHelperGeneric
{
    public static int MakeNormalizedFromSorted<T>(Span<CustomRange<T>> result, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>
    {
        if (result.Length <= 1)
        {
            return result.Length;
        }

        var resultList = new ListStackAlloc<CustomRange<T>>(result, 1);

        for (int i = 1; i < result.Length; i++)
        {
            var current = result[i];
            ref var last = ref resultList.Last();

            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
            {
                T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                last = new CustomRange<T>(last.FirstAddress, max);
            }
            else
            {
                resultList.Add(current);
            }
        }

        return resultList.Count;
    }

    public static int MakeNormalizedFromUnsorted<T>(Span<CustomRange<T>> result, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>
    {
        result.Sort(CustomRangeComparer<T>.Instance);
        return MakeNormalizedFromSorted(result, one);
    }



    public static int UnionSortedSorted<T>(ReadOnlySpan<CustomRange<T>> sorted1, ReadOnlySpan<CustomRange<T>> sorted2, Span<CustomRange<T>> result, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>
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
            return MakeNormalizedFromSorted(result[..sorted2.Length], one);
        }

        if (sorted2.Length == 0)
        {
            sorted1.CopyTo(result);
            return MakeNormalizedFromSorted(result[..sorted1.Length], one);
        }

        ListStackAlloc<CustomRange<T>> resultList = new ListStackAlloc<CustomRange<T>>(result);
        int index1 = 0;
        int index2 = 0;

        // first pass
        {
            var item1 = sorted1[0];
            var item2 = sorted2[0];
            if (item1.FirstAddress.CompareTo(item2.FirstAddress) <= 0)
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
            CustomRange<T> current;
            var item1 = sorted1[index1];
            var item2 = sorted2[index2];
            if (item1.FirstAddress.CompareTo(item2.FirstAddress) <= 0)
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
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
            {
                T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                last = new CustomRange<T>(last.FirstAddress, max);
            }
            else
            {
                resultList.Add(current);
            }
        }

        while (index2 < sorted2.Length)
        {
            CustomRange<T> current = sorted2[index2];
            index2++;

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
                {
                    T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                    last = new CustomRange<T>(last.FirstAddress, max);
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        while (index1 < sorted1.Length)
        {
            CustomRange<T> current = sorted1[index1];
            index1++;

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
                {
                    T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                    last = new CustomRange<T>(last.FirstAddress, max);
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        return resultList.Count;
    }

    public static int UnionNormalizedNormalized<T>(ReadOnlySpan<CustomRange<T>> normalized1, ReadOnlySpan<CustomRange<T>> normalized2, Span<CustomRange<T>> result, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>
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

        ListStackAlloc<CustomRange<T>> resultList = new ListStackAlloc<CustomRange<T>>(result);
        int index1 = 0;
        int index2 = 0;

        // first pass
        {
            var item1 = normalized1[0];
            var item2 = normalized2[0];
            if (item1.FirstAddress.CompareTo(item2.FirstAddress) <= 0)
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
            CustomRange<T> current;
            var item1 = normalized1[index1];
            var item2 = normalized2[index2];
            if (item1.FirstAddress.CompareTo(item2.FirstAddress) <= 0)
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
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
                {
                    T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                    last = new CustomRange<T>(last.FirstAddress, max);
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        while (index2 < normalized2.Length)
        {
            CustomRange<T> current = normalized2[index2];
            index2++;

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
                {
                    T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                    last = new CustomRange<T>(last.FirstAddress, max);
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
            CustomRange<T> current = normalized1[index1];
            index1++;

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.LastAddress))
            {
                last = new CustomRange<T>(last.FirstAddress, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                if ((last.LastAddress + one).CompareTo(current.FirstAddress) >= 0)
                {
                    T max = last.LastAddress.CompareTo(current.LastAddress) >= 0 ? last.LastAddress : current.LastAddress;
                    last = new CustomRange<T>(last.FirstAddress, max);
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

    /// <returns>left and right parts</returns>
    private static ValueTuple<CustomRange<T>?, CustomRange<T>?> IntersectableExcept<T>(CustomRange<T> range, CustomRange<T> other, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
    {
        bool hasLeftPart = other.FirstAddress.CompareTo(range.FirstAddress) > 0 && !T.MinValue.Equals(other.FirstAddress);
        bool hasRightPart = other.LastAddress.CompareTo(range.LastAddress) < 0 && !T.MaxValue.Equals(other.LastAddress);

        if (hasLeftPart)
        {
            if (hasRightPart)
            {
                return ValueTuple.Create<CustomRange<T>?, CustomRange<T>?>(new CustomRange<T>(range.FirstAddress, other.FirstAddress - one), new CustomRange<T>(other.LastAddress + one, range.LastAddress));
            }
            else
            {
                return ValueTuple.Create<CustomRange<T>?, CustomRange<T>?>(new CustomRange<T>(range.FirstAddress, other.FirstAddress - one), null);
            }
        }
        else
        {
            if (hasRightPart)
            {
                return ValueTuple.Create<CustomRange<T>?, CustomRange<T>?>(null, new CustomRange<T>(other.LastAddress + one, range.LastAddress));
            }
            else
            {
                return ValueTuple.Create<CustomRange<T>?, CustomRange<T>?>(null, null);
            }
        }
    }



    public static int ExceptNormalizedSorted<T>(ReadOnlySpan<CustomRange<T>> normalized, ReadOnlySpan<CustomRange<T>> sorted, Span<CustomRange<T>> result, T one)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
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

        ListStackAlloc<CustomRange<T>> resultList = new ListStackAlloc<CustomRange<T>>(result);

        int i = 0;
        int j = 0;
        CustomRange<T> currentRange = normalized[0];

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

            if (currentRange.LastAddress.CompareTo(otherCurr.FirstAddress) < 0)
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
            else if (currentRange.FirstAddress.CompareTo(otherCurr.LastAddress) > 0)
            {
                // Current range is entirely after exclusion range - move to next exclusion
                j++;
            }
            else
            {
                // Ranges overlap - compute the difference
                (var leftPart, var rightPart) = IntersectableExcept(currentRange, otherCurr, one);

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
}