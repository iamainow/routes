using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace routes.v2;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet2
{
    public static readonly Ip4RangeSet2 Empty = new();
    public static readonly Ip4RangeSet2 All = new(Ip4Range.All);

    private readonly SortedSet<Ip4Range> _set;

    public Ip4RangeSet2()
    {
        _set = [];
    }

    public Ip4RangeSet2(Ip4Range ip4Range)
    {
        _set = [ip4Range];
    }

    public Ip4RangeSet2(Ip4Subnet subnet)
    {
        _set = [subnet];
    }

    public Ip4RangeSet2(IEnumerable<Ip4Range> ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);

        foreach (Ip4Range range in ranges)
        {
            Union(range);
        }
    }

    public Ip4RangeSet2(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);

        foreach (Ip4Subnet subnet in subnets)
        {
            Union(subnet);
        }
    }

    public Ip4RangeSet2(Ip4RangeSet2 rangeSet) : this()
    {
        ArgumentNullException.ThrowIfNull(rangeSet);

        _set = [.. rangeSet._set];
    }

    public void Union(Ip4Range other)
    {
        IEnumerable<Ip4Range> intersectableRanges = _set.Where(other.IsIntersects);
        Ip4Range forAdd = other;
        foreach (Ip4Range intersectableRange in intersectableRanges)
        {
            forAdd = forAdd.IntersectableUnion(intersectableRange);
            _ = _set.Remove(intersectableRange);
        }

        _ = _set.Add(forAdd);
    }

    public void Union(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (Ip4Range item in other._set)
        {
            Union(item);
        }
    }

    public void Except(Ip4Range other)
    {
        IEnumerable<Ip4Range> intersectableRanges = _set.Where(other.IsIntersects);
        foreach (Ip4Range intersectableRange in intersectableRanges)
        {
            Ip4Range[] forAdds = intersectableRange.IntersectableExcept(other);
            foreach (Ip4Range forAdd in forAdds)
            {
                _ = _set.Add(forAdd);
            }
            _ = _set.Remove(intersectableRange);
        }
    }

    public void Except(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (Ip4Range item in other._set)
        {
            Except(item);
        }
    }

    public void Intersect(Ip4Range other)
    {
        IEnumerable<Ip4Range> intersectableRanges = _set.Where(other.IsIntersects);
        foreach (Ip4Range intersectableRange in intersectableRanges)
        {
            Ip4Range forAdd = intersectableRange.IntersectableIntersect(other);
            _ = _set.Add(forAdd);
            _ = _set.Remove(intersectableRange);
        }
    }

    public void Intersect(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (Ip4Range item in other._set)
        {
            Intersect(item);
        }
    }

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

    public static Ip4RangeSet2 ExpandSet(Ip4RangeSet2 set, uint delta, out bool wasListChanged)
    {
        ArgumentNullException.ThrowIfNull(set);
        LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

        wasListChanged = ExpandSortedLinkedList(list, delta);
        return new Ip4RangeSet2(list);
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

    public static Ip4RangeSet2 ShrinkSet(Ip4RangeSet2 set, uint delta, out bool wasListChanged)
    {
        ArgumentNullException.ThrowIfNull(set);
        LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

        wasListChanged = ShrinkSortedLinkedList(list, delta);
        return new Ip4RangeSet2(list);
    }

    public Ip4RangeSet2 Simplify(uint delta)
    {
        Ip4RangeSet2 result = this;

        while (true)
        {
            ulong minSize = result.ToIp4Ranges().Min(x => x.Count);
            Ip4RangeSet2 negative = new(All.ToIp4Ranges());
            negative.Except(result);
            ulong minGap = negative.ToIp4Ranges().Min(x => x.Count);

            if (minSize <= minGap && minSize <= delta && minSize <= uint.MaxValue)
            {
                result = ShrinkSet(result, (uint)minSize, out _);
                continue;
            }
            else if (minGap <= minSize && minGap <= delta && minGap <= uint.MaxValue)
            {
                result = ExpandSet(result, (uint)minGap, out _);
                continue;
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public Ip4RangeSet2 Normalize()
    {
        return ExpandSet(this, 0, out _);
    }

    public Ip4RangeSet2 MinimizeSubnets(uint delta)
    {
        return new Ip4RangeSet2(ToIp4Subnets().Where(x => x.Count > delta));
    }

    public Ip4Range[] ToIp4Ranges()
    {
        return _set.ToArray();
    }

    public Ip4Subnet[] ToIp4Subnets()
    {
        return _set.SelectMany(x => x.ToSubnets()).ToArray();
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (Ip4Range item in _set)
        {
            _ = result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}