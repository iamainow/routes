using System.Diagnostics;
using System.Text;

namespace routes;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet2
{
    public static Ip4RangeSet2 Empty => new();
    public static Ip4RangeSet2 All => new(Ip4Range.All);

    // sorted by FirstAddress, elements not overlapping
    private readonly LinkedList<Ip4Range> _list;

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

    public Ip4RangeSet2(IEnumerable<Ip4Range> ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        foreach (Ip4Range range in ranges)
        {
            this.Union(range);
        }
    }

    public Ip4RangeSet2(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);
        foreach (Ip4Subnet subnet in subnets)
        {
            this.Union(subnet);
        }
    }

    public Ip4RangeSet2(Ip4RangeSet2 set) : this()
    {
        ArgumentNullException.ThrowIfNull(set);
        foreach (Ip4Range range in set._list)
        {
            this._list.AddLast(range);
        }
    }

    public void Union(Ip4Range other)
    {
        var current = _list.First;
        while (current is not null)
        {
            if (current.Value.IsIntersects(other))
            {
                var unioned = current.Value.IntersectableUnion(other);
                var next = current.Next;
                while (next is not null && next.Value.IsIntersects(unioned))
                {
                    unioned = unioned.IntersectableUnion(next.Value);
                    var toRemove = next;
                    next = next.Next;
                    _list.Remove(toRemove);
                }
                current.Value = unioned;
                return;
            }
            else if (current.Value.FirstAddress > other.LastAddress)
            {
                _list.AddBefore(current, other);
                return;
            }
            current = current.Next;
        }
        _list.AddLast(other);
    }

    public void Union(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        List<Ip4Range> allRanges = new List<Ip4Range>(this._list);
        allRanges.AddRange(other._list);
        allRanges.Sort((a, b) => a.FirstAddress.CompareTo(b.FirstAddress));
        LinkedList<Ip4Range> newList = new LinkedList<Ip4Range>();
        if (allRanges.Count > 0)
        {
            Ip4Range current = allRanges[0];
            for (int i = 1; i < allRanges.Count; i++)
            {
                if (current.IsIntersects(allRanges[i]))
                {
                    current = current.IntersectableUnion(allRanges[i]);
                }
                else
                {
                    newList.AddLast(current);
                    current = allRanges[i];
                }
            }
            newList.AddLast(current);
        }
        this._list.Clear();
        foreach (Ip4Range range in newList)
        {
            this._list.AddLast(range);
        }
    }

    public void Union2(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        List<Ip4Range> allRanges = new List<Ip4Range>(this._list);
        allRanges.AddRange(other._list);
        allRanges.Sort((a, b) => a.FirstAddress.CompareTo(b.FirstAddress));
        LinkedList<Ip4Range> newList = new LinkedList<Ip4Range>();
        if (allRanges.Count > 0)
        {
            Ip4Range current = allRanges[0];
            for (int i = 1; i < allRanges.Count; i++)
            {
                if (current.IsIntersects(allRanges[i]))
                {
                    current = current.IntersectableUnion(allRanges[i]);
                }
                else
                {
                    newList.AddLast(current);
                    current = allRanges[i];
                }
            }
            newList.AddLast(current);
        }
        this._list.Clear();
        foreach (Ip4Range range in newList)
        {
            this._list.AddLast(range);
        }
    }

    public void Except(Ip4Range other)
    {
        var current = _list.First;
        while (current is not null)
        {
            if (current.Value.IsIntersects(other))
            {
                while (current is not null && current.Value.IsIntersects(other))
                {
                    var newElements = current.Value.IntersectableExcept(other);
                    switch (newElements.Length)
                    {
                        case 0:
                            var toDelete = current;
                            current = current.Next;
                            _list.Remove(toDelete);
                            break;
                        case 1:
                            current.Value = newElements[0];
                            current = current.Next;
                            break;
                        case 2:
                            _list.AddBefore(current, newElements[0]);
                            current.Value = newElements[1];
                            current = current.Next;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                return;
            }
            else if (current.Value.FirstAddress > other.LastAddress) // already passed the intersection point - this means there are no overlapping elements and nothing to exclude
            {
                return;
            }

            current = current.Next;
        }
    }

    private enum GeneralComparisonResult
    {
        NonOverlapping_LessThan = -1,
        Overlaps = 0,
        NonOverlapping_GreaterThan = 1,
    }

    private static GeneralComparisonResult GeneralComparison(Ip4Range first, Ip4Range second)
    {
        if (first.LastAddress < second.FirstAddress)
        {
            return GeneralComparisonResult.NonOverlapping_LessThan;
        }
        else if (first.FirstAddress > second.LastAddress)
        {
            return GeneralComparisonResult.NonOverlapping_GreaterThan;
        }
        else
        {
            return GeneralComparisonResult.Overlaps;
        }
    }

    private enum OverlappingComparisonResult
    {
        /// <summary>
        ///   [___]
        /// [___]
        /// </summary>
        Overlaps_LL,
        /// <summary>
        ///     [___]
        /// [_______]
        /// </summary>
        Overlaps_LE,
        /// <summary>
        ///    [___]
        /// [_________]
        /// </summary>
        Overlaps_LR,
        /// <summary>
        /// [_______]
        /// [___]
        /// </summary>
        Overlaps_EL,
        /// <summary>
        /// [___]
        /// [___]
        /// </summary>
        Overlaps_EE,
        /// <summary>
        /// [___]
        /// [_______]
        /// </summary>
        Overlaps_ER,
        /// <summary>
        /// [_________]
        ///    [___]
        /// </summary>
        Overlaps_RL,
        /// <summary>
        /// [_______]
        ///     [___]
        /// </summary>
        Overlaps_RE,
        /// <summary>
        /// [___]
        ///   [___]
        /// </summary>
        Overlaps_RR,
    }

    private static OverlappingComparisonResult OverlappingComparison(Ip4Range first, Ip4Range second)
    {
        var leftCompare = first.FirstAddress.CompareTo(second.FirstAddress);
        var rightCompare = first.LastAddress.CompareTo(second.LastAddress);
        if (leftCompare < 0)
        {
            if (rightCompare < 0)
            {
                return OverlappingComparisonResult.Overlaps_LL;
            }
            else if (rightCompare > 0)
            {
                return OverlappingComparisonResult.Overlaps_LR;
            }
            else
            {
                return OverlappingComparisonResult.Overlaps_LE;
            }
        }
        else if (leftCompare > 0)
        {
            if (rightCompare < 0)
            {
                return OverlappingComparisonResult.Overlaps_RL;
            }
            else if (rightCompare > 0)
            {
                return OverlappingComparisonResult.Overlaps_RR;
            }
            else
            {
                return OverlappingComparisonResult.Overlaps_RE;
            }
        }
        else
        {
            if (rightCompare < 0)
            {
                return OverlappingComparisonResult.Overlaps_EL;
            }
            else if (rightCompare > 0)
            {
                return OverlappingComparisonResult.Overlaps_ER;
            }
            else
            {
                return OverlappingComparisonResult.Overlaps_EE;
            }
        }
    }

    public void Except(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null) return;
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            switch (GeneralComparison(current.Value, currentOther.Value))
            {
                case GeneralComparisonResult.NonOverlapping_LessThan: // current < currentOther
                    if (current.Next is null) return;
                    current = current.Next;
                    break;

                case GeneralComparisonResult.NonOverlapping_GreaterThan: // current > currentOther
                    if (currentOther.Next is null) return;
                    currentOther = currentOther.Next;
                    break;

                case GeneralComparisonResult.Overlaps:
                    var newElements = current.Value.IntersectableExcept(currentOther.Value);
                    switch (newElements.Length)
                    {
                        case 0:
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
                            break;
                        case 1:
                            current.Value = newElements[0];
                            break;
                        case 2:
                            _list.AddBefore(current, newElements[0]);
                            current.Value = newElements[1];
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;

                default:
                    throw new NotImplementedException();
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

    private static bool ExpandSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    {
        bool wasListChanged = false;
        LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
        while (current is not null && current.Next is not null)
        {
            LinkedListNode<Ip4Range> next = current.Next;
            // if gap between neighbors equals or more than delta, union them
            if ((ulong)(uint)current.Value.LastAddress + delta + 1 >= (uint)next.Value.FirstAddress)
            {
                current.Value = new Ip4Range(current.Value.FirstAddress, next.Value.LastAddress);
                sortedLinkedList.Remove(next);
                wasListChanged = true;
            }
            else
            {
                current = current.Next;
            }
        }

        return wasListChanged;
    }

    public void ExpandSet(uint delta, out bool wasListChanged)
    {
        wasListChanged = ExpandSortedLinkedList(this._list, delta);
    }

    private static bool ShrinkSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    {
        bool wasElementRemoved = false;
        LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
        while (current is not null)
        {
            // if current range is equals or smaller than delta, remove it
            if (current.Value.Count <= delta)
            {
                LinkedListNode<Ip4Range> toDelete = current;
                current = current.Next;
                sortedLinkedList.Remove(toDelete);
                wasElementRemoved = true;
            }
            else
            {
                current = current.Next;
            }
        }

        return wasElementRemoved;
    }

    public void ShrinkSet(uint delta, out bool wasListChanged)
    {
        wasListChanged = ShrinkSortedLinkedList(this._list, delta);
    }

    public void Simplify(uint delta)
    {
        while (this._list.Count >= 2)
        {
            ulong minSize = this.ToIp4Ranges().Min(x => x.Count);

            var temp = All;
            temp.Except(this);

            ulong minGap = temp.ToIp4Ranges().Min(x => x.Count);

            if (minSize <= minGap && minSize <= delta && minSize <= uint.MaxValue)
            {
                ShrinkSet((uint)minSize, out _);
                continue;
            }
            else if (minGap <= minSize && minGap <= delta && minGap <= uint.MaxValue)
            {
                ExpandSet((uint)minGap, out _);
                continue;
            }
            else
            {
                break;
            }
        }
    }

    public void Normalize()
    {
        ExpandSet(0, out _);
    }

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
            _ = result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}