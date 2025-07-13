using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace routes.core;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public readonly struct Ip4RangeSet
{
    public static readonly Ip4RangeSet Empty = new Ip4RangeSet();
    public static readonly Ip4RangeSet All = new Ip4RangeSet(Ip4Range.All);

    private readonly IImmutableList<Ip4Range> _list;

    public Ip4RangeSet()
    {
        _list = [];
    }

    public Ip4RangeSet(Ip4Range ip4Range)
    {
        _list = [ip4Range];
    }

    public Ip4RangeSet(Ip4Subnet subnet)
    {
        _list = [subnet];
    }

    public Ip4RangeSet(IEnumerable<Ip4Range> ranges)
    {
        var current = new Ip4RangeSet();
        foreach (var range in ranges)
        {
            current = current.Union(range);
        }

        this._list = current._list;
    }

    public Ip4RangeSet(IEnumerable<Ip4Subnet> subnets)
    {
        var current = new Ip4RangeSet();
        foreach (var subnet in subnets)
        {
            current = current.Union(subnet);
        }

        this._list = current._list;
    }

    private Ip4RangeSet(IImmutableList<Ip4Range> list)
    {
        _list = list;
    }

    public Ip4RangeSet Union(Ip4Range other)
    {
        List<Ip4Range> result = [];
        Ip4Range newItem = other;
        foreach (var item in _list)
        {
            if (newItem.IsIntersects(item))
            {
                newItem = newItem.IntersectableUnion(item);
            }
            else
            {
                result.Add(item);
            }
        }

        result.Add(newItem);

        return new Ip4RangeSet(result.ToImmutableList());
    }

    public Ip4RangeSet Union(Ip4RangeSet other)
    {
        Ip4RangeSet result = this;
        foreach (var item in other._list)
        {
            result = result.Union(item);
        }

        return result;
    }

    public Ip4RangeSet Except(Ip4Range other)
    {
        List<Ip4Range> result = [];
        foreach (var item in _list)
        {
            result.AddRange(item.Except(other));
        }

        return new Ip4RangeSet(result.ToImmutableList());
    }

    public Ip4RangeSet Except(Ip4RangeSet other)
    {
        Ip4RangeSet result = this;
        foreach (var item in other._list)
        {
            result = result.Except(item);
        }
        return result;
    }

    public Ip4RangeSet Intersect(Ip4Range other)
    {
        List<Ip4Range> result = [];
        foreach (var item in _list)
        {
            if (other.IsIntersects(item))
            {
                result.Add(other.IntersectableIntersect(item));
            }
        }

        return new Ip4RangeSet(result.ToImmutableList());
    }

    public Ip4RangeSet Intersect(Ip4RangeSet other)
    {
        Ip4RangeSet result = new Ip4RangeSet();
        foreach (var item in other._list)
        {
            result = result.Intersect(item);
        }
        return result;
    }

    private bool ExpandSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    {
        bool wasListChanged = false;
        LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
        while (current is not null && current.Next is not null)
        {
            var next = current.Next;
            // if gap between neighbors equals or more than delta, remove it
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

    public Ip4RangeSet ExpandSet(Ip4RangeSet set, uint delta, out bool wasListChanged)
    {
        LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

        wasListChanged = ExpandSortedLinkedList(list, delta);
        return new Ip4RangeSet(list);
    }

    private bool ShrinkSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    {
        bool wasElementRemoved = false;
        LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
        while (current is not null)
        {
            // if current range is equals or smaller than delta, remove it
            if (current.Value.Count <= delta)
            {
                var toDelete = current;
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

    public Ip4RangeSet ShrinkSet(Ip4RangeSet set, uint delta, out bool wasListChanged)
    {
        LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

        wasListChanged = ShrinkSortedLinkedList(list, delta);
        return new Ip4RangeSet(list);
    }

    public Ip4RangeSet Simplify(uint delta)
    {
        Ip4RangeSet result = this;

        while (true)
        {
            ulong minSize = result.ToIp4Ranges().Min(x => x.Count);
            ulong minGap = All.Except(result).ToIp4Ranges().Min(x => x.Count);

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
        StringBuilder result = new StringBuilder();
        foreach (var item in _list)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}