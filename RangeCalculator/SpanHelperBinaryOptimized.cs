using System.Diagnostics;

namespace RangeCalculator;

public static class SpanHelperBinaryOptimized
{
    /// <summary>
    /// ascPredicate of sorted must be F...F or T...T or F...FT...T
    /// meaning sorted array is sorted in asc order of predicate ascPredicate
    /// 
    /// example of usage:
    /// FindFirst([0,2,4,6,8], x => x >= 5) should return index of number 6, it's 3
    /// FindFirst([0,5,5,5,8], x => x >= 5) should return index of first number 5, it's 1
    /// </summary>
    public static SearchResult FindFirst<T>(ReadOnlySpan<T> sorted, Predicate<T> ascPredicate, out int index)
    {
        ArgumentNullException.ThrowIfNull(ascPredicate);
        switch (sorted.Length)
        {
            case 0:
                index = default;
                return SearchResult.ArrayIsEmpty;
            case 1:
                if (ascPredicate(sorted[0]))
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                index = default;
                return SearchResult.AllElementsNotSatisfiesCondition;
            case 2:
                if (ascPredicate(sorted[0]))
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                if (ascPredicate(sorted[1]))
                {
                    index = 1;
                    return SearchResult.ElementFound;
                }

                index = default;
                return SearchResult.AllElementsNotSatisfiesCondition;
            case 3:
                if (ascPredicate(sorted[1]))
                {
                    if (ascPredicate(sorted[0]))
                    {
                        index = 0;
                        return SearchResult.ElementFound;
                    }
                    else
                    {
                        index = 1;
                        return SearchResult.ElementFound;
                    }
                }
                else
                {
                    if (ascPredicate(sorted[2]))
                    {
                        index = 2;
                        return SearchResult.ElementFound;
                    }
                    else
                    {
                        index = default;
                        return SearchResult.AllElementsNotSatisfiesCondition;
                    }
                }
            default:
                Debug.Assert(sorted.Length > 3, "sorted.Length > 3");

                if (ascPredicate(sorted[0])) // optional perf optimization? - if first element in sorted asc array >= first then all elements >= first
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                if (!ascPredicate(sorted[^1]))
                {
                    index = default;
                    return SearchResult.AllElementsNotSatisfiesCondition; // if last element in sorted asc array < first then all elements < first
                }

                index = 1 + FindFirst(sorted[1..], ascPredicate);
                return SearchResult.ElementFound;
        }
    }

    private static int FindFirst<T>(ReadOnlySpan<T> sorted, Predicate<T> ascPredicate)
    {
        ArgumentNullException.ThrowIfNull(ascPredicate);
        Debug.Assert(ascPredicate(sorted[^1]), "ascPredicate(sorted[^1]");
        Debug.Assert(sorted.Length >= 3, "sorted.Length >= 3");

        switch (sorted.Length)
        {
            //case 1:
            //    return 0;
            //case 2:
            //    if (ascPredicate(sorted[0]))
            //    {
            //        return 0;
            //    }
            //    else
            //    {
            //        return 1;
            //    }
            case 3:
                if (ascPredicate(sorted[0]))
                {
                    return 0;
                }
                else if (ascPredicate(sorted[1]))
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            case 4:
                if (ascPredicate(sorted[1]))
                {
                    if (ascPredicate(sorted[0]))
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (ascPredicate(sorted[2]))
                    {
                        return 2;
                    }
                    else
                    {
                        return 3;
                    }
                }
            case 5:
                if (ascPredicate(sorted[2]))
                {
                    if (ascPredicate(sorted[1]))
                    {
                        if (ascPredicate(sorted[0]))
                        {
                            return 0;
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    if (ascPredicate(sorted[3]))
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }
            default:
                int mid = (sorted.Length - 1) >> 1;
                if (ascPredicate(sorted[mid]))
                {
                    // [?] [?] [t] [t] [t] [t]
                    //         ^^^
                    // [         ]
                    return FindFirst(sorted[..(mid + 1)], ascPredicate);
                }
                else
                {
                    // [?] [?] [f] [?] [?] [t]
                    //         ^^^
                    //             [         ]
                    return (mid + 1) + FindFirst(sorted[(mid + 1)..], ascPredicate);
                }
        }
    }



    /// <summary>
    /// descPredicate of sorted must be F...F or T...T or T...TF...F
    /// meaning sorted array is sorted in desc order of predicate descPredicate
    /// 
    /// example of usage:
    /// FindFirst([0,2,4,6,8], x => x <= 5) should return index of number 4, it's 2
    /// FindFirst([0,5,5,5,8], x => x <= 5) should return index of last number 5, it's 3
    /// </summary>
    public static SearchResult FindLast<T>(ReadOnlySpan<T> sorted, Predicate<T> descPredicate, out int index)
    {
        ArgumentNullException.ThrowIfNull(descPredicate);
        switch (sorted.Length)
        {
            case 0:
                index = default;
                return SearchResult.ArrayIsEmpty;
            case 1:
                if (descPredicate(sorted[0]))
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                index = default;
                return SearchResult.AllElementsNotSatisfiesCondition;
            case 2:
                if (descPredicate(sorted[1]))
                {
                    index = 1;
                    return SearchResult.ElementFound;
                }

                if (descPredicate(sorted[0]))
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                index = default;
                return SearchResult.AllElementsNotSatisfiesCondition;
            case 3:
                if (descPredicate(sorted[1]))
                {
                    if (descPredicate(sorted[2]))
                    {
                        index = 2;
                        return SearchResult.ElementFound;
                    }
                    else
                    {
                        index = 1;
                        return SearchResult.ElementFound;
                    }
                }
                else
                {
                    if (descPredicate(sorted[0]))
                    {
                        index = 0;
                        return SearchResult.ElementFound;
                    }
                    else
                    {
                        index = default;
                        return SearchResult.AllElementsNotSatisfiesCondition;
                    }
                }
            default:
                Debug.Assert(sorted.Length > 3, "sorted.Length > 3");

                if (descPredicate(sorted[^1])) // optional perf optimization?
                {
                    index = sorted.Length - 1;
                    return SearchResult.ElementFound;
                }

                if (!descPredicate(sorted[0]))
                {
                    index = default;
                    return SearchResult.AllElementsNotSatisfiesCondition;
                }

                // [t] [x] [x] [x] [f]
                // [             ]
                index = FindLast(sorted[..^1], descPredicate);
                return SearchResult.ElementFound;
        }
    }

    private static int FindLast<T>(ReadOnlySpan<T> sorted, Predicate<T> descPredicate)
    {
        ArgumentNullException.ThrowIfNull(descPredicate);
        Debug.Assert(descPredicate(sorted[0]), "descPredicate(sorted[0]");
        Debug.Assert(sorted.Length >= 3, "sorted.Length >= 3");

        switch (sorted.Length)
        {
            //case 1:
            //    return 0;
            //case 2:
            //    if (ascPredicate(sorted[0]))
            //    {
            //        return 0;
            //    }
            //    else
            //    {
            //        return 1;
            //    }
            case 3:
                if (descPredicate(sorted[2]))
                {
                    return 2;
                }
                else if (descPredicate(sorted[1]))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            case 4:
                if (descPredicate(sorted[2]))
                {
                    if (descPredicate(sorted[3]))
                    {
                        return 3;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    if (descPredicate(sorted[1]))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            case 5:
                if (descPredicate(sorted[2]))
                {
                    if (descPredicate(sorted[3]))
                    {
                        if (descPredicate(sorted[4]))
                        {
                            return 4;
                        }
                        else
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    if (descPredicate(sorted[1]))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            default:
                int mid = 1 + (sorted.Length - 1) >> 1;
                if (descPredicate(sorted[mid]))
                {
                    // [t] [x] [x] [t] [x] [x]
                    //             ^^^
                    //             [         ]
                    return mid + FindLast(sorted[mid..], descPredicate);
                }
                else
                {
                    // [t] [x] [x] [f] [f] [f]
                    //             ^^^
                    // [         ]
                    return FindLast(sorted[..mid], descPredicate);
                }
        }
    }

    public static SearchResult2 FindFirstAndFindLast<T>(ReadOnlySpan<T> sorted, Predicate<T> ascPredicate, Predicate<T> descPredicate, out int firstIndex, out int lastIndex)
    {
        var findFirst = FindFirst(sorted, ascPredicate, out int firstIndex2);
        if (findFirst == SearchResult.ArrayIsEmpty)
        {
            firstIndex = default;
            lastIndex = default;
            return SearchResult2.ArrayIsEmpty;
        }
        else if (findFirst == SearchResult.AllElementsNotSatisfiesCondition)
        {
            firstIndex = default;
            lastIndex = default;
            return SearchResult2.OutOfBoundsAtRight;
        }

        var findLast = FindLast(sorted[firstIndex2..], descPredicate, out int lastIndex2);
        if (findLast == SearchResult.ArrayIsEmpty)
        {
            firstIndex = default;
            lastIndex = default;
            return SearchResult2.ArrayIsEmpty;
        }
        else if (findLast == SearchResult.AllElementsNotSatisfiesCondition)
        {
            if (firstIndex2 == 0)
            {
                firstIndex = default;
                lastIndex = default;
                return SearchResult2.OutOfBoundsAtLeft;
            }
            else
            {
                throw new ArgumentException($"{nameof(ascPredicate)} and {nameof(descPredicate)} is invalid and return nonsence");
            }
        }

        firstIndex = firstIndex2;
        lastIndex = firstIndex2 + lastIndex2;
        return SearchResult2.ElementFound;
    }
}
