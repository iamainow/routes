using System.Diagnostics;
using System.Text;

namespace routes;

[DebuggerDisplay("{_list.Count,nq} ip ranges")]
public class Ip4RangeSet2
{
    public static Ip4RangeSet2 Empty => new();
    public static Ip4RangeSet2 All => new(Ip4Range.All);

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

    public static IEnumerable<LinkedListNode<Ip4Range>> AsEnumerableLinkedListNode(LinkedListNode<Ip4Range>? startFrom)
    {
        var current = startFrom;
        while (current is not null)
        {
            yield return current;
            current = current.Next;
        }
    }

    public IEnumerable<LinkedListNode<Ip4Range>> AsEnumerableLinkedListNode()
    {
        var current = _list.First;
        while (current is not null)
        {
            yield return current;
            current = current.Next;
        }
    }

    public void Union(Ip4Range other)
    {
        foreach (var node in AsEnumerableLinkedListNode())
        {
            if (node.Value.IsIntersects(other))
            {
                var newElement = node.Value.IntersectableUnion(other);
                // режим прохода до последнего пересекающегося и объединения их
                foreach (var intersectableNode in AsEnumerableLinkedListNode(node.Next).TakeWhile(x => x.Value.IsIntersects(newElement)))
                {
                    newElement = newElement.IntersectableUnion(intersectableNode.Value);
                }

                node.Value = newElement;
                return;
            }
            else if (node.Value.FirstAddress > other.LastAddress) // уже прошли место вставки - это означает что вставляем перед текущим и нет пересекающихся элементов
            {
                _list.AddBefore(node, other);
                return;
            }
        }

        _list.AddLast(other);
    }

    public void Union(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (Ip4Range item in other._list)
        {
            Union(item);
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
            else if (current.Value.FirstAddress > other.LastAddress) // уже прошли место - это означает что нет пересекающихся элементов и нечего исключать
            {
                _list.AddBefore(current, other);
                return;
            }

            current = current.Next;
        }
    }

    public void Except(Ip4RangeSet2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (Ip4Range item in other._list)
        {
            Except(item);
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

    //private static bool ExpandSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    //{
    //    bool wasListChanged = false;
    //    LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
    //    while (current is not null && current.Next is not null)
    //    {
    //        LinkedListNode<Ip4Range> next = current.Next;
    //        // if gap between neighbors equals or more than delta, union them
    //        if ((ulong)(uint)current.Value.LastAddress + delta + 1 >= (uint)next.Value.FirstAddress)
    //        {
    //            current.Value = new Ip4Range(current.Value.FirstAddress, next.Value.LastAddress);
    //            sortedLinkedList.Remove(next);
    //            wasListChanged = true;
    //        }
    //        else
    //        {
    //            current = current.Next;
    //        }
    //    }

    //    return wasListChanged;
    //}

    //public static Ip4RangeSet2 ExpandSet(Ip4RangeSet2 set, uint delta, out bool wasListChanged)
    //{
    //    ArgumentNullException.ThrowIfNull(set);
    //    LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

    //    wasListChanged = ExpandSortedLinkedList(list, delta);
    //    return new Ip4RangeSet2(list);
    //}

    //private static bool ShrinkSortedLinkedList(LinkedList<Ip4Range> sortedLinkedList, uint delta)
    //{
    //    bool wasElementRemoved = false;
    //    LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
    //    while (current is not null)
    //    {
    //        // if current range is equals or smaller than delta, remove it
    //        if (current.Value.Count <= delta)
    //        {
    //            LinkedListNode<Ip4Range> toDelete = current;
    //            current = current.Next;
    //            sortedLinkedList.Remove(toDelete);
    //            wasElementRemoved = true;
    //        }
    //        else
    //        {
    //            current = current.Next;
    //        }
    //    }

    //    return wasElementRemoved;
    //}

    //public static Ip4RangeSet2 ShrinkSet(Ip4RangeSet2 set, uint delta, out bool wasListChanged)
    //{
    //    ArgumentNullException.ThrowIfNull(set);
    //    LinkedList<Ip4Range> list = new(set.ToIp4Ranges().OrderBy(x => x.FirstAddress));

    //    wasListChanged = ShrinkSortedLinkedList(list, delta);
    //    return new Ip4RangeSet2(list);
    //}

    //public Ip4RangeSet2 Simplify(uint delta)
    //{
    //    Ip4RangeSet2 result = this;

    //    while (true)
    //    {
    //        ulong minSize = result.ToIp4Ranges().Min(x => x.Count);
    //        ulong minGap = All.Except(result).ToIp4Ranges().Min(x => x.Count);

    //        if (minSize <= minGap && minSize <= delta && minSize <= uint.MaxValue)
    //        {
    //            result = ShrinkSet(result, (uint)minSize, out _);
    //            continue;
    //        }
    //        else if (minGap <= minSize && minGap <= delta && minGap <= uint.MaxValue)
    //        {
    //            result = ExpandSet(result, (uint)minGap, out _);
    //            continue;
    //        }
    //        else
    //        {
    //            break;
    //        }
    //    }

    //    return result;
    //}

    //public Ip4RangeSet2 Normalize()
    //{
    //    return ExpandSet(this, 0, out _);
    //}

    //public Ip4RangeSet2 MinimizeSubnets(uint delta)
    //{
    //    return new Ip4RangeSet2(ToIp4Subnets().Where(x => x.Count > delta));
    //}

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