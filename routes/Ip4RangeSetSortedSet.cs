using System.Diagnostics;
using System.Text;

namespace routes;

[DebuggerDisplay("{_set.Count,nq} ip ranges")]
public class Ip4RangeSetSortedSet
{
    public static Ip4RangeSetSortedSet Empty => new();
    public static Ip4RangeSetSortedSet All => new(Ip4Range.All);

    // sorted by FirstAddress, elements not overlapping
    private SortedSet<Ip4Range> _set;

    public Ip4RangeSetSortedSet()
    {
        _set = new(Ip4RangeComparer.Instance);
    }

    public Ip4RangeSetSortedSet(Ip4Range ip4Range) : this()
    {
        _set.Add(ip4Range);
    }

    public Ip4RangeSetSortedSet(Ip4Subnet subnet) : this()
    {
        _set.Add(subnet);
    }

    public Ip4RangeSetSortedSet(Ip4Range[] ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSetSortedSet(IEnumerable<Ip4Range> ranges) : this()
    {
        ArgumentNullException.ThrowIfNull(ranges);
        Union(ranges);
    }

    public Ip4RangeSetSortedSet(IEnumerable<Ip4Subnet> subnets) : this()
    {
        ArgumentNullException.ThrowIfNull(subnets);
        this.Union(subnets);
    }

    public Ip4RangeSetSortedSet(Ip4RangeSetSortedSet set) : this()
    {
        ArgumentNullException.ThrowIfNull(set);
        this._set = new SortedSet<Ip4Range>(set._set, Ip4RangeComparer.Instance);
    }

    public void Union(Ip4RangeSetSortedSet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_set.Count == 0)
        {
            foreach (var range in other._set)
            {
                _set.Add(range);
            }
            return;
        }
        var current = _set.GetEnumerator();
        var currentOther = other._set.GetEnumerator();

        bool hasCurrent = current.MoveNext();
        bool hasOther = currentOther.MoveNext();

        var result = new SortedSet<Ip4Range>(Ip4RangeComparer.Instance);

        while (hasCurrent || hasOther)
        {
            if (!hasCurrent)
            {
                result.Add(currentOther.Current);
                hasOther = currentOther.MoveNext();
                continue;
            }
            if (!hasOther)
            {
                result.Add(current.Current);
                hasCurrent = current.MoveNext();
                continue;
            }

            var c = current.Current.FirstAddress.CompareTo(currentOther.Current.FirstAddress);
            if (c < 0)
            {
                result.Add(current.Current);
                hasCurrent = current.MoveNext();
            }
            else if (c > 0)
            {
                result.Add(currentOther.Current);
                hasOther = currentOther.MoveNext();
            }
            else
            {
                // Same start, merge
                var merged = current.Current.IntersectableUnion(currentOther.Current);
                result.Add(merged);
                hasCurrent = current.MoveNext();
                hasOther = currentOther.MoveNext();
            }
        }

        // Now merge overlapping in result
        var final = new SortedSet<Ip4Range>(Ip4RangeComparer.Instance);
        var enumerator = result.GetEnumerator();
        if (!enumerator.MoveNext()) return;
        var prev = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var curr = enumerator.Current;
            if (prev.IsIntersects(curr) || prev.LastAddress.ToUInt32() + 1 == curr.FirstAddress.ToUInt32())
            {
                prev = prev.IntersectableUnion(curr);
            }
            else
            {
                final.Add(prev);
                prev = curr;
            }
        }
        final.Add(prev);

        _set = final;
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

    private void InternalUnionSortedRanges(IEnumerable<Ip4Range> sortedRanges)
    {
        var result = new SortedSet<Ip4Range>(_set, Ip4RangeComparer.Instance);

        foreach (var range in sortedRanges)
        {
            result.Add(range);
        }

        // Merge overlaps
        var final = new SortedSet<Ip4Range>(Ip4RangeComparer.Instance);
        var enumerator = result.GetEnumerator();
        if (!enumerator.MoveNext()) return;
        var prev = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var curr = enumerator.Current;
            if (prev.IsIntersects(curr) || prev.LastAddress.ToUInt32() + 1 == curr.FirstAddress.ToUInt32())
            {
                prev = prev.IntersectableUnion(curr);
            }
            else
            {
                final.Add(prev);
                prev = curr;
            }
        }
        final.Add(prev);

        _set = final;
    }

    public void Except(Ip4RangeSetSortedSet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (_set.Count == 0 || other._set.Count == 0) return;

        var result = new SortedSet<Ip4Range>(Ip4RangeComparer.Instance);

        var current = _set.GetEnumerator();
        var currentOther = other._set.GetEnumerator();

        bool hasCurrent = current.MoveNext();
        bool hasOther = currentOther.MoveNext();

        while (hasCurrent)
        {
            if (!hasOther)
            {
                result.Add(current.Current);
                hasCurrent = current.MoveNext();
                continue;
            }

            var curr = current.Current;
            var otherCurr = currentOther.Current;

            if (curr.LastAddress < otherCurr.FirstAddress)
            {
                result.Add(curr);
                hasCurrent = current.MoveNext();
            }
            else if (curr.FirstAddress > otherCurr.LastAddress)
            {
                hasOther = currentOther.MoveNext();
            }
            else
            {
                var excepted = curr.IntersectableExcept(otherCurr);
                foreach (var ex in excepted)
                {
                    result.Add(ex);
                }
                hasCurrent = current.MoveNext();
            }
        }

        _set = result;
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
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}