using System.Diagnostics;
using System.Numerics;

namespace routes.v2;

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
        foreach (Interval<T> item in items)
        {
            Union(item);
        }
    }

    public SortedLinkedListOfIntervals(Interval<T> item)
    {
        list = new();
        _ = list.AddFirst(item);
    }

    public LinkedListNode<Interval<T>>? FindLast(Predicate<Interval<T>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        LinkedListNode<Interval<T>>? current = list.First;
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

        LinkedListNode<Interval<T>>? current = startFrom;
        while (current is not null && !predicate(current.Value))
        {
            current = current.Next;
        }

        return current;
    }

    public void Union(Interval<T> item)
    {
        if (list.Count == 0) // empty
        {
            _ = list.AddFirst(item);
            return;
        }

        Debug.Assert(list.First is not null); // list.Count != 0
        Debug.Assert(list.Last is not null); // list.Count != 0

        if (list.Count == 1) // 1 element
        {
            LinkedListNode<Interval<T>> element = list.First;
            if (element.Value.IsIntersects(item))
            {
                element.Value = element.Value.IntersectableUnion(item);
            }
            else
            {
                _ = item.BeginInclusive < element.Value.BeginInclusive ? list.AddFirst(item) : list.AddLast(item);
            }
            return;
        }

        // minimum 2 elements;

        if (item.EndInclusive < list.First.Value.BeginInclusive)
        {
            _ = list.AddFirst(item);
            return;
        }

        if (item.BeginInclusive > list.Last.Value.EndInclusive)
        {
            _ = list.AddLast(item);
            return;
        }

        // minimum 2 elements;

        LinkedListNode<Interval<T>>? first = FindFirst(x => x.EndInclusive > item.BeginInclusive);

        LinkedListNode<Interval<T>>? last = FindFirst(x => x.BeginInclusive <= item.EndInclusive, first);

        if (first is null)
        {
            if (last is null)
            {
                Interval<T> value = list.First.Value.IntersectableUnion(item).IntersectableUnion(list.Last.Value);
                list.Clear();
                _ = list.AddFirst(value);
            }
            else
            {
                for (LinkedListNode<Interval<T>>? node = list.First; node != last && node is not null; node = node.Next)
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
        _ = list.Remove(item);
    }

    public void Remove(LinkedListNode<Interval<T>> node)
    {
        list.Remove(node);
    }
}
