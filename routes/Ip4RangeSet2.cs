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

    public void Union(Ip4RangeSet2 other) => Union3(other);

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

    public void Except(Ip4RangeSet2 other) => Except4(other);

    public void Except1(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var current = _list.First;
        var currentOther = other._list.First;

        while (current is not null && currentOther is not null)
        {
            switch (Ip4Range.GeneralComparison(current.Value, currentOther.Value))
            {
                case < 0: // current < currentOther
                    current = current.Next;
                    break;

                case > 0: // current > currentOther
                    currentOther = currentOther.Next;
                    break;

                case 0:
                    var newElements = current.Value.IntersectableExcept(currentOther.Value);
                    switch (newElements.Length)
                    {
                        case 0:
                            var toDelete = current;
                            current = current.Next;
                            _list.Remove(toDelete);
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

    public void Except2(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null) return;
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            switch (Ip4Range.GeneralComparison(current.Value, currentOther.Value))
            {
                case < 0: // current < currentOther
                    if (current.Next is null) return;
                    current = current.Next;
                    break;

                case > 0: // current > currentOther
                    if (currentOther.Next is null) return;
                    currentOther = currentOther.Next;
                    break;

                case 0:
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

    public void Except3(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null) return;
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            switch (Ip4Range.GeneralComparison(current.Value, currentOther.Value))
            {
                case -1: // current < currentOther
                    if (current.Next is null) return;
                    current = current.Next;
                    break;

                case 1: // current > currentOther
                    if (currentOther.Next is null) return;
                    currentOther = currentOther.Next;
                    break;

                case 0:
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
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}