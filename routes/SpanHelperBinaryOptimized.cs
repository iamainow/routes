using System.Diagnostics;

namespace routes;

public static class SpanHelperBinaryOptimized
{
    private static SearchResult SearchFirstAddressGreaterOrEqualsTo(ReadOnlySpan<Ip4Range> sorted, Predicate<Ip4Range> ascPredicate, out int index)
    {
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
                Debug.Assert(sorted.Length > 3, "sortedByFirstAddress.Length > 3");

                if (ascPredicate(sorted[0])) // optional perf optimization? - if first element in sorted asc array >= firstAddress then all elements >= firstAddress
                {
                    index = 0;
                    return SearchResult.ElementFound;
                }

                if (!ascPredicate(sorted[^1]))
                {
                    index = default;
                    return SearchResult.AllElementsNotSatisfiesCondition; // if last element in sorted asc array < firstAddress then all elements < firstAddress
                }

                index = 1 + FindIndexFirstAddressGreaterOrEqualsTo(sorted[1..], ascPredicate);
                return SearchResult.ElementFound;

                //or

                //if (!(sortedByFirstAddress[^1].FirstAddress >= firstAddress))
                //{
                //    index = default;
                //    return SearchResult.AllElementsNotSatisfiesCondition; // if last element in sorted asc array < firstAddress then all elements < firstAddress
                //}

                //index = FindIndexFirstAddressGreaterOrEqualsTo(sortedByFirstAddress, firstAddress, 0, sortedByFirstAddress.Length);
                //return SearchResult.ElementFound;
        }
    }

    private static int FindIndexFirstAddressGreaterOrEqualsTo(ReadOnlySpan<Ip4Range> sorted, Predicate<Ip4Range> ascPredicate)
    {
        Debug.Assert(ascPredicate(sorted[^1]), "ascPredicate(sorted[^1].FirstAddress)");
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
                    return FindIndexFirstAddressGreaterOrEqualsTo(sorted[..(mid + 1)], ascPredicate);
                }
                else
                {
                    // [?] [?] [f] [?] [?] [t]
                    //         ^^^
                    //             [         ]
                    return (mid + 1) + FindIndexFirstAddressGreaterOrEqualsTo(sorted[(mid + 1)..], ascPredicate);
                }
        }
    }
}
