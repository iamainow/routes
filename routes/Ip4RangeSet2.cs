using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace routes;

public interface ICountable<T>
{
    T GetNext();
    T GetPrevious();
}

public readonly struct Interval<T> : IEquatable<Interval<T>>
    where T : IComparable<T>, IComparisonOperators<T, T, bool>, ICountable<T>
{
    public readonly T BeginInclusive { get; }
    public readonly T EndInclusive { get; }

    public Interval(T beginInclusive, T endInclusive)
    {
        if (beginInclusive > endInclusive)
        {
            throw new ArgumentException("beginInclusive must be less than or equal to endInclusive.");
        }

        BeginInclusive = beginInclusive;
        EndInclusive = endInclusive;
    }

    public bool IsIntersects(Interval<T> other)
    {

        return other.BeginInclusive <= EndInclusive && other.EndInclusive >= BeginInclusive;
    }

    public Interval<T> IntersectableUnion(Interval<T> other)
    {
        T begin = BeginInclusive < other.BeginInclusive ? BeginInclusive : other.BeginInclusive;
        T end = EndInclusive > other.EndInclusive ? EndInclusive : other.EndInclusive;

        return new Interval<T>(begin, end);
    }

    public Interval<T>[] Union(Interval<T> other)
    {
        if (!IsIntersects(other))
        {
            return [this, other];
        }

        return [IntersectableUnion(other)];
    }

    public Interval<T> IntersectableIntersect(Interval<T> other)
    {
        T begin = BeginInclusive > other.BeginInclusive ? BeginInclusive : other.BeginInclusive;
        T end = EndInclusive < other.EndInclusive ? EndInclusive : other.EndInclusive;
        return new Interval<T>(begin, end);
    }

    public Interval<T>? Intersect(Interval<T> other)
    {
        if (!IsIntersects(other))
        {
            return null;
        }

        return IntersectableIntersect(other);
    }

    public Interval<T>[] IntersectableExcept(Interval<T> other)
    {
        if (other.BeginInclusive <= BeginInclusive)
        {
            if (other.EndInclusive < EndInclusive)
            {
                return [new Interval<T>(other.EndInclusive.GetNext(), EndInclusive)];
            }
            else
            {
                return [];
            }
        }
        else
        {
            if (other.EndInclusive < EndInclusive)
            {
                return [new Interval<T>(BeginInclusive, other.BeginInclusive.GetPrevious()), new Interval<T>(other.EndInclusive.GetNext(), EndInclusive)];
            }
            else
            {
                return [new Interval<T>(BeginInclusive, other.BeginInclusive.GetPrevious())];
            }
        }
    }

    public Interval<T>[] Except(Interval<T> other)
    {
        if (IsIntersects(other))
        {
            return IntersectableExcept(other);
        }

        return [this];
    }

    public override bool Equals(object? obj)
    {
        return obj is Interval<T> interval && Equals(interval);
    }

    public bool Equals(Interval<T> other)
    {
        return BeginInclusive == other.BeginInclusive && EndInclusive == other.EndInclusive;
    }

    public static bool operator ==(Interval<T> left, Interval<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Interval<T> left, Interval<T> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BeginInclusive, EndInclusive);
    }
}

public class SortedLinkedListOfIntervals<T>
    where T : IComparable<T>, IComparisonOperators<T, T, bool>, ICountable<T>
{
    private readonly LinkedList<Interval<T>> list;

    public SortedLinkedListOfIntervals()
    {
        list = new();
    }

    public SortedLinkedListOfIntervals(IEnumerable<Interval<T>> items) : this()
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
        {
            Union(item);
        }
    }

    public SortedLinkedListOfIntervals(Interval<T> item)
    {
        list = new();
        list.AddFirst(item);
    }

    public LinkedListNode<Interval<T>>? FindLast(Predicate<Interval<T>> predicate)
    {
        var current = list.First;
        while (current is not null && !predicate(current.Value))
        {
            current = current.Next;
        }

        return current;
    }

    public LinkedListNode<Interval<T>>? FindFirst(Predicate<Interval<T>> predicate)
    {
        return FindFirst(predicate, list.First);
    }

    public LinkedListNode<Interval<T>>? FindFirst(Predicate<Interval<T>> predicate, LinkedListNode<Interval<T>>? startFrom)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var current = startFrom;
        while (current is not null && !predicate(current.Value))
        {
            current = current.Next;
        }

        return current;
    }

    public void Union(Interval<T> item)
    {
        if (list.First is null || list.Last is null) // empty
        {
            list.AddFirst(item);
            return;
        }

        // list.First is not null here
        if (list.First.Next is null) // 1 element
        {
            if (list.First.Value.IsIntersects(item))
            {
                list.First.Value = list.First.Value.IntersectableUnion(item);
            }
            else
            {
                if (item.BeginInclusive < list.First.Value.BeginInclusive)
                {
                    list.AddFirst(item);
                }
                else
                {
                    list.AddLast(item);
                }
            }
            return;
        }

        // minimum 2 elements;

        if (item.EndInclusive < list.First.Value.BeginInclusive)
        {
            list.AddFirst(item);
            return;
        }

        if (item.BeginInclusive > list.Last.Value.EndInclusive)
        {
            list.AddLast(item);
            return;
        }

        // minimum 2 elements;

        var first = FindFirst(x => x.EndInclusive > item.BeginInclusive);

        var last = FindFirst(x => x.BeginInclusive <= item.EndInclusive, first);

        if (first is null)
        {
            if (last is null)
            {
                var value = list.First.Value.IntersectableUnion(item).IntersectableUnion(list.Last.Value);
                list.Clear();
                list.AddFirst(value);
            }
            else
            {
                for (var node = list.First; node != last; node = node.Next)
                {

                }
            }
        }
        else
        {
            if (last is null)
            {

            }
            else
            {

            }
        }
    }

    public void Remove(Interval<T> item)
    {
        list.Remove(item);
    }

    public void Remove(LinkedListNode<Interval<T>> node)
    {
        list.Remove(node);
    }
}

public class SortedLinkedList<T>
    where T : IComparable<T>
{
    private readonly LinkedList<T> _list;

    public SortedLinkedList()
    {
        _list = new();
    }

    public SortedLinkedList(IEnumerable<T> items) : this()
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public SortedLinkedList(SortedLinkedList<T> sortedLinkedList) : this()
    {
        ArgumentNullException.ThrowIfNull(sortedLinkedList);
        foreach (var item in sortedLinkedList._list)
        {
            _list.AddLast(item);
        }
    }
    // a.CompareTo(b)
    // < 0 a < b
    // = 0 a == b
    // > 0 a > b

    public void Union(T item)
    {
        foreach (var node in _list)
        {
            if (item.CompareTo(node) < 0)
            {
                _list.AddBefore(_list.Find(node)!, item);
                return;
            }
        }
    }

    public void Remove(T item)
    {
        _list.Remove(item);
    }

    public void Remove(LinkedListNode<T> node)
    {
        _list.Remove(node);
    }
}

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet2
{
    public static readonly Ip4RangeSet2 Empty = new Ip4RangeSet2();
    public static readonly Ip4RangeSet2 All = new Ip4RangeSet2(Ip4Range.All);

    private SortedSet<Ip4Range> _set;

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

        foreach (var range in ranges)
        {
            Union(range);
        }
    }

    public Ip4RangeSet2(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);

        foreach (var subnet in subnets)
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
        var intersectableRanges = _set.Where(other.IsIntersects);
        var forAdd = other;
        foreach (var intersectableRange in intersectableRanges)
        {
            forAdd = forAdd.IntersectableUnion(intersectableRange);
            _set.Remove(intersectableRange);
        }

        _set.Add(forAdd);
    }

    public void Union(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var item in other._set)
        {
            Union(item);
        }
    }

    public void Except(Ip4Range other)
    {
        var intersectableRanges = _set.Where(other.IsIntersects);
        foreach (var intersectableRange in intersectableRanges)
        {
            var forAdds = intersectableRange.IntersectableExcept(other);
            foreach (var forAdd in forAdds)
            {
                _set.Add(forAdd);
            }
            _set.Remove(intersectableRange);
        }
    }

    public void Except(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var item in other._set)
        {
            Except(item);
        }
    }

    public void Intersect(Ip4Range other)
    {
        var intersectableRanges = _set.Where(other.IsIntersects);
        foreach (var intersectableRange in intersectableRanges)
        {
            var forAdd = intersectableRange.IntersectableIntersect(other);
            _set.Add(forAdd);
            _set.Remove(intersectableRange);
        }
    }

    public void Intersect(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var item in other._set)
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
            var next = current.Next;
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
            Ip4RangeSet2 negative = new Ip4RangeSet2(All.ToIp4Ranges());
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
        StringBuilder result = new StringBuilder();
        foreach (var item in _set)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}