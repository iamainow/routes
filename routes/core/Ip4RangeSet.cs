using System.Collections.Immutable;

namespace routes.core;

public readonly struct Ip4RangeSet
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

    public Ip4RangeSet Except(Ip4Range subnet)
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }

    public Ip4RangeSet Intersect(Ip4Range other)
    {
        List<Ip4Range> result = [];
        foreach (var item in _list)
        {
            if (other.IsIntersects(item))
            {
                result.Add();
            }
        }

        return new Ip4RangeSet(result.ToImmutableList());
    }
}