using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace routes.core;

public readonly struct Ip4RangeSet : IEnumerable<Ip4Range>
{
    private readonly IImmutableList<Ip4Range> _list;

    public Ip4RangeSet()
    {
        _list = [];
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

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        foreach (var item in _list)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }

    public IEnumerator<Ip4Range> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}