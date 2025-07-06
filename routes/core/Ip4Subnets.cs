using System.Collections.Immutable;

namespace routes.core;

public readonly struct Ip4RangeCollection
{
    private readonly IImmutableList<Ip4Range> _list;

    public Ip4RangeCollection()
    {
        _list = [];
    }

    private Ip4RangeCollection(IImmutableList<Ip4Range> list)
    {
        _list = list;
    }

    public Ip4RangeCollection Union(Ip4Range other)
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

        return new Ip4RangeCollection(result.ToImmutableList());
    }

    public void Except(Ip4Subnet subnet)
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }
}