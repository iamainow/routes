using System.Diagnostics;
using System.Text;

namespace routes;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet
{
    public static Ip4RangeSet Empty => new();
    public static Ip4RangeSet All => new(Ip4Range.All);

    // sorted by FirstAddress, elements not overlapping, elements non-adjacent/disjoint
    private LinkedList<Ip4Range> _list;

    public Ip4RangeSet()
    {
        _list = new();
    }

    public Ip4RangeSet(Ip4Range ip4Range) : this()
    {
        _list.AddFirst(ip4Range);
    }

    public Ip4RangeSet(Ip4Subnet subnet) : this()
    {
        _list.AddFirst(subnet);
    }

    public Ip4RangeSet(Ip4Range[] ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSet(IEnumerable<Ip4Range> ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSet(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);
        this.Union(subnets);
    }

    public Ip4RangeSet(Ip4RangeSet set) : this()
    {
        ArgumentNullException.ThrowIfNull(set);
        this._list = new LinkedList<Ip4Range>(set._list);
    }

    public void Union4(Ip4RangeSet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null)
        {
            var currOther = other._list.First;
            while (currOther is not null)
            {
                _list.AddLast(currOther.Value);
                currOther = currOther.Next;
            }
            return;
        }
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            if (current.Value.LastAddress.ToUInt32() + 1L < currentOther.Value.FirstAddress.ToUInt32()) // [current]   [currentOther]
            {
                if (current.Next is null)
                {
                    var currOther = currentOther;
                    while (currOther is not null)
                    {
                        _list.AddLast(currOther.Value);
                        currOther = currOther.Next;
                    }
                    return;
                }
                current = current.Next;
            }
            else if (current.Value.LastAddress.ToUInt32() + 1L == currentOther.Value.FirstAddress.ToUInt32()) // [current][currentOther]
            {
                current.Value = new Ip4Range(current.Value.FirstAddress, currentOther.Value.LastAddress);
                if (current.Next is null)
                {
                    var currOther = currentOther.Next;
                    while (currOther is not null)
                    {
                        _list.AddLast(currOther.Value);
                        currOther = currOther.Next;
                    }
                    return;
                }
                else
                {
                    current = current.Next;
                }
            }
            else if (current.Value.FirstAddress.ToUInt32() > currentOther.Value.LastAddress.ToUInt32() + 1L) // [currentOther]   [current]
            {
                if (currentOther.Next is null) return;
                currentOther = currentOther.Next;
            }
            else if (current.Value.FirstAddress.ToUInt32() == currentOther.Value.LastAddress.ToUInt32() + 1L) // [currentOther][current]
            {
                current.Value = new Ip4Range(currentOther.Value.FirstAddress, current.Value.LastAddress);
                if (currentOther.Next is null)
                {
                    return;
                }
                else
                {
                    currentOther = currentOther.Next;
                }
            }
            else
            {
                var newElement = current.Value.IntersectableUnion(currentOther.Value);
                bool moveOther = current.Value.LastAddress >= currentOther.Value.LastAddress;
                current.Value = newElement;
                if (moveOther)
                {
                    if (currentOther.Next is null)
                    {
                        return;
                    }
                    else
                    {
                        currentOther = currentOther.Next;
                    }
                }
                else
                {
                    if (current.Next is null)
                    {
                        var currOther = currentOther.Next;
                        while (currOther is not null)
                        {
                            _list.AddLast(currOther.Value);
                            currOther = currOther.Next;
                        }
                        return;
                    }
                    else
                    {
                        if (current.Value.IsIntersects(current.Next.Value))
                        {
                            // The static analyzer incorrectly flags this as dead code because it doesn't understand that removing current.Next changes the linked list structure, making the condition dynamic.
#pragma warning disable CA1508 // Avoid dead conditional code
                            do
                            {
                                current.Value = current.Value.IntersectableUnion(current.Next.Value);
                                _list.Remove(current.Next);
                            } while (current.Next is not null && current.Value.IsIntersects(current.Next.Value));
#pragma warning restore CA1508 // Avoid dead conditional code
                        }
                        else
                        {
                            current = current.Next;
                        }
                    }
                }
            }
        }
    }

    public void Union(Ip4RangeSet other)
    {
#pragma warning disable IDE0010 // Add missing cases
        ArgumentNullException.ThrowIfNull(other);

        if (_list.First is null)
        {
            var currOther = other._list.First;
            while (currOther is not null)
            {
                _list.AddLast(currOther.Value);
                currOther = currOther.Next;
            }
            return;
        }
        LinkedListNode<Ip4Range> current = _list.First;

        if (other._list.First is null) return;
        LinkedListNode<Ip4Range> currentOther = other._list.First;

        while (true)
        {
            if (currentOther.Value.FirstAddress > current.Value.LastAddress)
            {
                switch ((currentOther.Value.FirstAddress.ToUInt32() - current.Value.LastAddress.ToUInt32()).CompareTo(1U))
                {
                    case > 0:
                        {
                            if (current.Next is null)
                            {
                                var currOther = currentOther;
                                while (currOther is not null)
                                {
                                    _list.AddLast(currOther.Value);
                                    currOther = currOther.Next;
                                }
                                return;
                            }
                            current = current.Next;
                            continue;
                        }

                    case 0:
                        {
                            current.Value = new Ip4Range(current.Value.FirstAddress, currentOther.Value.LastAddress);
                            if (current.Next is null)
                            {
                                var currOther = currentOther.Next;
                                while (currOther is not null)
                                {
                                    _list.AddLast(currOther.Value);
                                    currOther = currOther.Next;
                                }
                                return;
                            }
                            else
                            {
                                current = current.Next;
                            }
                            continue;
                        }
                }
            }

            if (current.Value.FirstAddress > currentOther.Value.LastAddress)
            {
                switch ((current.Value.FirstAddress.ToUInt32() - currentOther.Value.LastAddress.ToUInt32()).CompareTo(1U))
                {
                    case > 0:
                        if (currentOther.Next is null) return;
                        currentOther = currentOther.Next;
                        continue;
                    case 0:
                        current.Value = new Ip4Range(currentOther.Value.FirstAddress, current.Value.LastAddress);
                        if (currentOther.Next is null)
                        {
                            return;
                        }
                        else
                        {
                            currentOther = currentOther.Next;
                        }
                        continue;
                }
            }
            {
                var newElement = current.Value.IntersectableUnion(currentOther.Value);
                bool moveOther = current.Value.LastAddress >= currentOther.Value.LastAddress;
                current.Value = newElement;
                if (moveOther)
                {
                    if (currentOther.Next is null)
                    {
                        return;
                    }
                    else
                    {
                        currentOther = currentOther.Next;
                        continue;
                    }
                }
                else
                {
                    if (current.Next is null)
                    {
                        var currOther = currentOther.Next;
                        while (currOther is not null)
                        {
                            _list.AddLast(currOther.Value);
                            currOther = currOther.Next;
                        }
                        return;
                    }
                    else
                    {
                        if (current.Value.IsIntersects(current.Next.Value))
                        {
                            // The static analyzer incorrectly flags this as dead code because it doesn't understand that removing current.Next changes the linked list structure, making the condition dynamic.
#pragma warning disable CA1508 // Avoid dead conditional code
                            do
                            {
                                current.Value = current.Value.IntersectableUnion(current.Next.Value);
                                _list.Remove(current.Next);
                            } while (current.Next is not null && current.Value.IsIntersects(current.Next.Value));
#pragma warning restore CA1508 // Avoid dead conditional code

                            continue;
                        }
                        else
                        {
                            current = current.Next;
                            continue;
                        }
                    }
                }
            }
        }
#pragma warning restore IDE0010 // Add missing cases
    }

    public void Union(Ip4Range[] ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        Array.Sort(ranges, Ip4RangeComparer.Instance);

        InternalUnionSortedRanges(ranges);
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

    //public void Except(Ip4RangeSet other) => Except4(other);

    public void Except(Ip4RangeSet other)
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
                    if (currentOther.Next is not null)
                    {
                        currentOther = currentOther.Next;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }

    public void Except(Ip4Range range)
    {
        Ip4RangeSet temp = new Ip4RangeSet(range);
        Except(temp);
    }

    public void Union(Ip4Range range)
    {
        Ip4RangeSet temp = new Ip4RangeSet(range);
        Union(temp);
    }

    public Ip4RangeSet MinimizeSubnets(uint delta)
    {
        return new Ip4RangeSet(ToIp4Subnets().Where(x => x.Count >= delta));
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

    public int RangesCount => _list.Count;
}