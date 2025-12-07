using System.Diagnostics;
using System.Text;

namespace routes;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet2
{
    public static Ip4RangeSet2 Empty => new();
    public static Ip4RangeSet2 All => new(Ip4Range.All);

    // sorted by FirstAddress, elements not overlapping
    private LinkedList<Ip4Range> _list;

    public Ip4RangeSet2()
    {
        _list = new();
    }

    public Ip4RangeSet2(Ip4Range ip4Range) : this()
    {
        _list.AddFirst(ip4Range);
    }

    public Ip4RangeSet2(Ip4Subnet subnet) : this()
    {
        _list.AddFirst(subnet);
    }

    public Ip4RangeSet2(Ip4Range[] ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSet2(IEnumerable<Ip4Range> ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSet2(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);
        this.Union(subnets);
    }

    public Ip4RangeSet2(Ip4RangeSet2 set) : this()
    {
        ArgumentNullException.ThrowIfNull(set);
        this._list = new LinkedList<Ip4Range>(set._list);
    }

    public void Union(Ip4RangeSet2 other) => Union4(other);

    /// <summary>
    /// Defines the spatial relationship between two IP ranges.
    /// </summary>
    private enum RangeRelationship
    {
        /// <summary>
        /// Current range precedes other with gap:
        /// [current]   [other]
        /// </summary>
        PrecedesWithGap,

        /// <summary>
        /// Current range is adjacent before other:
        /// [current][other]
        /// </summary>
        AdjacentBefore,

        /// <summary>
        /// Other range precedes current with gap:
        /// [other]   [current]
        /// </summary>
        FollowsWithGap,

        /// <summary>
        /// Other range is adjacent before current:
        /// [other][current]
        /// </summary>
        AdjacentAfter,

        /// <summary>
        /// Ranges overlap or touch:
        /// [current]
        ///    [other]
        /// </summary>
        Overlaps
    }

    /// <summary>
    /// Merges another Ip4RangeSet2 into this set in-place, maintaining sorted order and merging overlapping/adjacent ranges.
    /// Time Complexity: O(n+m) where n is the size of this set and m is the size of the other set.
    /// Space Complexity: O(1) - modifies this set in-place.
    /// </summary>
    /// <param name="other">The set to merge into this set.</param>
    /// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
    public void Union4(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Phase 1: Edge Cases
        if (other._list.First is null)
        {
            return; // Nothing to merge
        }

        if (_list.First is null)
        {
            // Copy all ranges from other to this empty list
            var currOther = other._list.First;
            while (currOther is not null)
            {
                _list.AddLast(currOther.Value);
                currOther = currOther.Next;
            }
            return;
        }

        // Phase 2: Handle Preceding Ranges (Bug Fix)
        // If other has ranges that precede all ranges in _list, prepend them
        PrependPrecedingRanges(_list, other._list.First, _list.First.Value);

        // Phase 3: Main Merge Loop
        LinkedListNode<Ip4Range>? current = _list.First;
        LinkedListNode<Ip4Range>? currentOther = other._list.First;

        while (current is not null && currentOther is not null)
        {
            RangeRelationship relationship = DetermineRelationship(current.Value, currentOther.Value);

            switch (relationship)
            {
                case RangeRelationship.PrecedesWithGap:
                    // [current]   [other] - move to next current
                    current = current.Next;
                    break;

                case RangeRelationship.AdjacentBefore:
                    // [current][other] - merge and cascade
                    current.Value = new Ip4Range(current.Value.FirstAddress, currentOther.Value.LastAddress);
                    CascadeMergeWithNext(current);
                    currentOther = currentOther.Next;
                    break;

                case RangeRelationship.FollowsWithGap:
                    // [other]   [current] - move to next other
                    currentOther = currentOther.Next;
                    break;

                case RangeRelationship.AdjacentAfter:
                    // [other][current] - merge and cascade
                    current.Value = new Ip4Range(currentOther.Value.FirstAddress, current.Value.LastAddress);
                    CascadeMergeWithNext(current);
                    currentOther = currentOther.Next;
                    break;

                case RangeRelationship.Overlaps:
                    // Ranges overlap or touch - merge them
                    current.Value = current.Value.IntersectableUnion(currentOther.Value);
                    
                    // Determine which pointer to advance based on which range ends first
                    if (current.Value.LastAddress >= currentOther.Value.LastAddress)
                    {
                        // Current consumed other, advance other
                        currentOther = currentOther.Next;
                        // Cascade merge with next nodes in _list if needed
                        CascadeMergeWithNext(current);
                    }
                    else
                    {
                        // Other extends beyond current, advance current
                        current = current.Next;
                    }
                    break;

                default:
                    // This should never happen as all enum values are covered
                    throw new InvalidOperationException($"Unexpected relationship: {relationship}");
            }
        }

        // Phase 4: Append Remaining Ranges
        if (currentOther is not null)
        {
            AppendRemainingRanges(_list, currentOther);
        }

        // Phase 5: Final Cascade Merge
        // Ensure the last node is properly merged with any following nodes
        if (_list.Last is not null && _list.Last.Previous is not null)
        {
            CascadeMergeWithNext(_list.Last.Previous);
        }
    }

    /// <summary>
    /// Determines the spatial relationship between two IP ranges.
    /// </summary>
    /// <param name="current">The current range from this set.</param>
    /// <param name="other">The other range being compared.</param>
    /// <returns>The relationship between the two ranges.</returns>
    private static RangeRelationship DetermineRelationship(Ip4Range current, Ip4Range other)
    {
        long currentEnd = current.LastAddress.ToUInt32();
        long otherStart = other.FirstAddress.ToUInt32();
        long currentStart = current.FirstAddress.ToUInt32();
        long otherEnd = other.LastAddress.ToUInt32();

        if (currentEnd + 1 < otherStart)
        {
            return RangeRelationship.PrecedesWithGap;
        }
        else if (currentEnd + 1 == otherStart)
        {
            return RangeRelationship.AdjacentBefore;
        }
        else if (currentStart > otherEnd + 1)
        {
            return RangeRelationship.FollowsWithGap;
        }
        else if (currentStart == otherEnd + 1)
        {
            return RangeRelationship.AdjacentAfter;
        }
        else
        {
            return RangeRelationship.Overlaps;
        }
    }

    /// <summary>
    /// Checks if two ranges are adjacent (touching but not overlapping).
    /// </summary>
    /// <param name="first">The first range.</param>
    /// <param name="second">The second range.</param>
    /// <returns>True if the ranges are adjacent, false otherwise.</returns>
    private static bool AreAdjacent(Ip4Range first, Ip4Range second)
    {
        return first.LastAddress.ToUInt32() + 1 == second.FirstAddress.ToUInt32();
    }

    /// <summary>
    /// Appends all remaining ranges from the source node onwards to the target list.
    /// </summary>
    /// <param name="target">The target linked list to append to.</param>
    /// <param name="source">The source node to start appending from.</param>
    private static void AppendRemainingRanges(LinkedList<Ip4Range> target, LinkedListNode<Ip4Range>? source)
    {
        while (source is not null)
        {
            target.AddLast(source.Value);
            source = source.Next;
        }
    }

    /// <summary>
    /// Attempts to merge the current node with its next node if they overlap or are adjacent.
    /// </summary>
    /// <param name="current">The current node to potentially merge.</param>
    /// <returns>True if a merge occurred, false otherwise.</returns>
    private bool TryMergeWithNext(LinkedListNode<Ip4Range> current)
    {
        if (current.Next is null)
        {
            return false;
        }

        if (current.Value.IsIntersects(current.Next.Value) || AreAdjacent(current.Value, current.Next.Value))
        {
            current.Value = current.Value.IntersectableUnion(current.Next.Value);
            _list.Remove(current.Next);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cascades merge operations with subsequent nodes as long as they overlap or are adjacent.
    /// </summary>
    /// <param name="current">The starting node for cascade merging.</param>
    private void CascadeMergeWithNext(LinkedListNode<Ip4Range> current)
    {
        while (TryMergeWithNext(current))
        {
            // Continue merging until no more merges are possible
        }
    }

    /// <summary>
    /// Prepends ranges from other that precede the first range in target.
    /// This fixes the bug where preceding ranges were lost.
    /// </summary>
    /// <param name="target">The target list to prepend to.</param>
    /// <param name="otherNode">The starting node from the other list.</param>
    /// <param name="targetFirst">The first range in the target list.</param>
    private static void PrependPrecedingRanges(LinkedList<Ip4Range> target, LinkedListNode<Ip4Range>? otherNode, Ip4Range targetFirst)
    {
        List<Ip4Range> precedingRanges = new();

        while (otherNode is not null)
        {
            // Check if other range precedes target's first range (with or without gap)
            if (otherNode.Value.LastAddress.ToUInt32() < targetFirst.FirstAddress.ToUInt32())
            {
                precedingRanges.Add(otherNode.Value);
                otherNode = otherNode.Next;
            }
            else
            {
                // Stop when we reach a range that doesn't precede
                break;
            }
        }

        // Prepend all collected preceding ranges
        for (int i = precedingRanges.Count - 1; i >= 0; i--)
        {
            target.AddFirst(precedingRanges[i]);
        }
    }

    public void Union3(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other._list.First is null)
        {
            return;
        }

        if (_list.First is null)
        {
            _list = new LinkedList<Ip4Range>(other._list);
            return;
        }

        var current = _list.First;
        var currentOther = other._list.First;

        LinkedList<Ip4Range> result = new();

        if (current.Value.FirstAddress < currentOther.Value.FirstAddress)
        {
            result.AddFirst(current.Value);
            current = current.Next;
        }
        else
        {
            result.AddFirst(currentOther.Value);
            currentOther = currentOther.Next;
        }

        while (current is not null && currentOther is not null)
        {
            if (current.Value.FirstAddress < currentOther.Value.FirstAddress)
            {
                if (result.Last!.Value.IsIntersects(current.Value))
                {
                    result.Last.Value = result.Last.Value.IntersectableUnion(current.Value);
                }
                else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == current.Value.FirstAddress.ToUInt32())
                {
                    result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, current.Value.LastAddress);
                }
                else
                {
                    result.AddLast(current.Value);
                }

                current = current.Next;
            }
            else
            {
                if (result.Last!.Value.IsIntersects(currentOther.Value))
                {
                    result.Last.Value = result.Last.Value.IntersectableUnion(currentOther.Value);
                }
                else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == currentOther.Value.FirstAddress.ToUInt32())
                {
                    result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, currentOther.Value.LastAddress);
                }
                else
                {
                    result.AddLast(currentOther.Value);
                }

                currentOther = currentOther.Next;
            }
        }

        while (current is not null)
        {
            if (result.Last!.Value.IsIntersects(current.Value))
            {
                result.Last.Value = result.Last.Value.IntersectableUnion(current.Value);
            }
            else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == current.Value.FirstAddress.ToUInt32())
            {
                result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, current.Value.LastAddress);
            }
            else
            {
                result.AddLast(current.Value);
            }

            current = current.Next;
        }

        while (currentOther is not null)
        {
            if (result.Last!.Value.IsIntersects(currentOther.Value))
            {
                result.Last.Value = result.Last.Value.IntersectableUnion(currentOther.Value);
            }
            else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == currentOther.Value.FirstAddress.ToUInt32())
            {
                result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, currentOther.Value.LastAddress);
            }
            else
            {
                result.AddLast(currentOther.Value);
            }

            currentOther = currentOther.Next;
        }

        this._list = result;
    }

    public void Union(Ip4Range[] ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        Ip4Range[] temp = new Ip4Range[ranges.Length];
        Array.Copy(ranges, temp, temp.Length);

        Array.Sort(temp, Ip4RangeComparer.Instance);

        InternalUnionSortedRanges(temp);
    }

    public void Union(IEnumerable<Ip4Range> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        Ip4Range[] temp = ranges.ToArray();

        Array.Sort(temp, Ip4RangeComparer.Instance);

        InternalUnionSortedRanges(temp);
    }

    public void Union(IEnumerable<Ip4Subnet> subnets)
    {
        ArgumentNullException.ThrowIfNull(subnets);

        Ip4Range[] temp = subnets.Select(x => x.ToIp4Range()).ToArray();

        Array.Sort(temp, Ip4RangeComparer.Instance);

        InternalUnionSortedRanges(temp);
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private void InternalUnionSortedRanges(IEnumerable<Ip4Range> sortedRanges)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        using IEnumerator<Ip4Range> sortedRangesEnumerator = sortedRanges.GetEnumerator();
        bool sortedRangesValueExists = sortedRangesEnumerator.MoveNext();
        if (!sortedRangesValueExists) // 0 elements
        {
            return;
        }

        var current = _list.First;

        LinkedList<Ip4Range> result = new();

        if (current is not null && sortedRangesValueExists)
        {
            if (current.Value.FirstAddress < sortedRangesEnumerator.Current.FirstAddress)
            {
                result.AddFirst(current.Value);
                current = current.Next;
            }
            else
            {
                result.AddFirst(sortedRangesEnumerator.Current);
                sortedRangesValueExists = sortedRangesEnumerator.MoveNext();
            }
        }
        else
        {
            if (current is not null)
            {
                result.AddFirst(current.Value);
                current = current.Next;
            }
            else
            {
                result.AddFirst(sortedRangesEnumerator.Current);
                sortedRangesValueExists = sortedRangesEnumerator.MoveNext();
            }
        }

        while (current is not null && sortedRangesValueExists)
        {
            if (current.Value.FirstAddress < sortedRangesEnumerator.Current.FirstAddress)
            {
                if (result.Last!.Value.IsIntersects(current.Value))
                {
                    result.Last.Value = result.Last.Value.IntersectableUnion(current.Value);
                }
                else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == current.Value.FirstAddress.ToUInt32())
                {
                    result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, current.Value.LastAddress);
                }
                else
                {
                    result.AddLast(current.Value);
                }

                current = current.Next;
            }
            else
            {
                if (result.Last!.Value.IsIntersects(sortedRangesEnumerator.Current))
                {
                    result.Last.Value = result.Last.Value.IntersectableUnion(sortedRangesEnumerator.Current);
                }
                else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == sortedRangesEnumerator.Current.FirstAddress.ToUInt32())
                {
                    result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, sortedRangesEnumerator.Current.LastAddress);
                }
                else
                {
                    result.AddLast(sortedRangesEnumerator.Current);
                }

                sortedRangesValueExists = sortedRangesEnumerator.MoveNext();
            }
        }

        while (current is not null)
        {
            if (result.Last!.Value.IsIntersects(current.Value))
            {
                result.Last.Value = result.Last.Value.IntersectableUnion(current.Value);
            }
            else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == current.Value.FirstAddress.ToUInt32())
            {
                result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, current.Value.LastAddress);
            }
            else
            {
                result.AddLast(current.Value);
            }

            current = current.Next;
        }

        while (sortedRangesValueExists)
        {
            if (result.Last!.Value.IsIntersects(sortedRangesEnumerator.Current))
            {
                result.Last.Value = result.Last.Value.IntersectableUnion(sortedRangesEnumerator.Current);
            }
            else if (result.Last!.Value.LastAddress.ToUInt32() + 1 == sortedRangesEnumerator.Current.FirstAddress.ToUInt32())
            {
                result.Last.Value = new Ip4Range(result.Last!.Value.FirstAddress, sortedRangesEnumerator.Current.LastAddress);
            }
            else
            {
                result.AddLast(sortedRangesEnumerator.Current);
            }

            sortedRangesValueExists = sortedRangesEnumerator.MoveNext();
        }

        this._list = result;
    }

    public void Except(Ip4RangeSet2 other) => Except4(other);

    public void Except4(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null) return;
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            if (current.Value.LastAddress < currentOther.Value.FirstAddress)
            {
                if (current.Next is null) return;
                current = current.Next;
            }
            else if (current.Value.FirstAddress > currentOther.Value.LastAddress)
            {
                if (currentOther.Next is null) return;
                currentOther = currentOther.Next;
            }
            else
            {
                var newElements = current.Value.IntersectableExcept(currentOther.Value);
                if (newElements.Length == 0)
                {
                    if (current.Next is null)
                    {
                        _list.Remove(current);
                        return;
                    }
                    else
                    {
                        var toDelete = current;
                        current = current.Next;
                        _list.Remove(toDelete);
                    }
                }
                else if (newElements.Length == 1)
                {
                    current.Value = newElements[0];
                }
                else
                {
                    _list.AddBefore(current, newElements[0]);
                    current.Value = newElements[1];
                }
            }
        }
    }

    //public void Intersect(Ip4Range other)
    //{
    //    List<Ip4Range> result = [];
    //    foreach (Ip4Range item in _list)
    //    {
    //        if (other.IsIntersects(item))
    //        {
    //            result.Add(other.IntersectableIntersect(item));
    //        }
    //    }

    //    return new Ip4RangeSet2(result.ToImmutableList());
    //}

    //public void Intersect(Ip4RangeSet2 other)
    //{
    //    ArgumentNullException.ThrowIfNull(other);
    //    Ip4RangeSet2 result = new();
    //    foreach (Ip4Range item in other._list)
    //    {
    //        result = result.Union(Intersect(item));
    //    }
    //    return result;
    //}

    public Ip4RangeSet2 MinimizeSubnets(uint delta)
    {
        return new Ip4RangeSet2(ToIp4Subnets().Where(x => x.Count > delta));
    }

    public Ip4Range[] ToIp4Ranges()
    {
        return _list.ToArray();
    }

    public Ip4Subnet[] ToIp4Subnets()
    {
        return _list.SelectMany(x => x.ToSubnets()).ToArray();
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (Ip4Range item in _list)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}