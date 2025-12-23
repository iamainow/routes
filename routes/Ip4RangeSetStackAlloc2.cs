//namespace routes;

//public readonly ref struct Ip4RangeSetStackAlloc2
//{
//    private readonly ListStackAlloc<Ip4Range> _ranges; // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint

//    public ReadOnlySpan<Ip4Range> ToReadOnlySpan() => _ranges.AsReadOnlySpan();

//    public ReadOnlySpan<Ip4Range> ToSpan() => _ranges.AsSpan();

//    public Ip4RangeSetStackAlloc2(Span<Ip4Range> buffer)
//    {
//        _ranges = new ListStackAlloc<Ip4Range>(buffer);
//    }

//    // Set operations with fluent API
//    public Ip4RangeSetStackAlloc2 Union(Ip4RangeSetStackAlloc2 other, Span<Ip4Range> resultBuffer);
//    public Ip4RangeSetStackAlloc2 Except(Ip4RangeSetStackAlloc2 other, Span<Ip4Range> resultBuffer);
//    public Ip4RangeSetStackAlloc2 Intersect(Ip4RangeSetStackAlloc2 other, Span<Ip4Range> resultBuffer)
//    {
//        throw new NotImplementedException();
//    }

//    // Buffer management
//    public static int CalcUnionBufferSize(Ip4RangeSetStackAlloc2 left, Ip4RangeSetStackAlloc2 right)
//    {
//        return left._ranges.Count + right._ranges.Count;
//    }
//    public static int CalcExceptBufferSize(Ip4RangeSetStackAlloc2 left, Ip4RangeSetStackAlloc2 right)
//    {
//        return left._ranges.Count + right._ranges.Count;
//    }
//    public static int CalcIntersectBufferSize(Ip4RangeSetStackAlloc2 left, Ip4RangeSetStackAlloc2 right)
//    {
//        return Math.Max(left._ranges.Count, right._ranges.Count);
//    }

//    // Initial union support
//    public Ip4RangeSetStackAlloc2 Union(Span<Ip4Range> ranges, Span<Ip4Range> resultBuffer)
//    {
//        // Sort the input ranges
//        Span<Ip4Range> sortedRanges = stackalloc Ip4Range[ranges.Length];
//        ranges.CopyTo(sortedRanges);
//        sortedRanges.Sort(Ip4RangeComparer.Instance);

//        // Create result
//        var result = new Ip4RangeSetStackAlloc2(resultBuffer);

//        // Merge
//        int i = 0, j = 0;
//        while (i < _ranges.Count && j < sortedRanges.Length)
//        {
//            var current = _ranges[i];
//            var currentOther = sortedRanges[j];

//            if (currentOther.FirstAddress > current.LastAddress)
//            {
//                result._ranges.Add(current);
//                i++;
//            }
//            else if (current.FirstAddress > currentOther.LastAddress)
//            {
//                result._ranges.Add(currentOther);
//                j++;
//            }
//            else
//            {
//                var newElement = current.IntersectableUnion(currentOther);
//                result._ranges.Add(newElement);
//                if (current.LastAddress >= currentOther.LastAddress)
//                {
//                    j++;
//                }
//                else
//                {
//                    i++;
//                }
//            }
//        }

//        // Add remaining
//        while (i < _ranges.Count)
//        {
//            result._ranges.Add(_ranges[i]);
//            i++;
//        }

//        while (j < sortedRanges.Length)
//        {
//            result._ranges.Add(sortedRanges[j]);
//            j++;
//        }

//        // Normalize result if needed
//        if (result._ranges.Count > 1)
//        {
//            Span<Ip4Range> normalized = stackalloc Ip4Range[result._ranges.Count];
//            result._ranges.AsReadOnlySpan().CopyTo(normalized);
//            int writeIndex = 0;
//            if (normalized.Length > 0)
//            {
//                normalized[writeIndex++] = normalized[0];
//                for (int k = 1; k < normalized.Length; k++)
//                {
//                    var curr = normalized[k];
//                    var last = normalized[writeIndex - 1];
//                    if (last.IsIntersects(curr))
//                    {
//                        normalized[writeIndex - 1] = last.IntersectableUnion(curr);
//                    }
//                    else if (last.LastAddress.ToUInt32() + 1 == curr.FirstAddress.ToUInt32())
//                    {
//                        normalized[writeIndex - 1] = new Ip4Range(last.FirstAddress, curr.LastAddress);
//                    }
//                    else
//                    {
//                        normalized[writeIndex++] = curr;
//                    }
//                }
//            }
//            result._ranges.RemoveRegion(0, result._ranges.Count);
//            for (int idx = 0; idx < writeIndex; idx++)
//            {
//                result._ranges.Add(normalized[idx]);
//            }
//        }

//        return result;
//    }
//    public static int CalcUnionBufferSize(Ip4RangeSetStackAlloc2 left, Span<Ip4Range> right)
//    {
//        return left._ranges.Count + right.Length;
//    }
//}
