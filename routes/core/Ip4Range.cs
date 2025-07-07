using System.Diagnostics;

namespace routes.core;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Range
{
    private static uint GetNthBit(uint number, int position)
    {
        if (position == 0)
        {
            return number & 1;
        }

        return (number >> (position - 1)) & 1;
    }

    private static uint GetLastBits(uint number, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        return (number << (32 - count)) >> (32 - count);
    }

    private static uint GetFirstBits(uint number, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        return (number >> (32 - count)) << (32 - count);
    }

    public static implicit operator Ip4RangeSet(Ip4Range range)
    {
        return new Ip4RangeSet(range);
    }

    public readonly Ip4Address FirstAddress;
    public readonly Ip4Address LastAddress;

    public uint Count => LastAddress.AsUInt32() - FirstAddress.AsUInt32() + 1;

    public Ip4Range(Ip4Address start, Ip4Address end)
    {
        FirstAddress = start;
        LastAddress = end;
    }

    public bool IsIntersects(Ip4Range other)
    {
        return other.FirstAddress <= this.LastAddress && other.LastAddress >= this.FirstAddress;
    }

    public Ip4Range IntersectableUnion(Ip4Range other)
    {
        Ip4Address start = this.FirstAddress < other.FirstAddress ? this.FirstAddress : other.FirstAddress;
        Ip4Address end = this.LastAddress > other.LastAddress ? this.LastAddress : other.LastAddress;

        return new Ip4Range(start, end);
    }

    public Ip4Range[] Union(Ip4Range other)
    {
        if (!IsIntersects(other))
        {
            return [this, other];
        }

        return [IntersectableUnion(other)];
    }

    public Ip4Range IntersectableIntersect(Ip4Range other)
    {
        Ip4Address start = Ip4Address.Max(this.FirstAddress, other.FirstAddress);
        Ip4Address end = Ip4Address.Min(this.LastAddress, other.LastAddress);
        return new Ip4Range(start, end);
    }

    public Ip4Range? Intersect(Ip4Range other)
    {
        if (!IsIntersects(other))
        {
            return null;
        }

        return IntersectableIntersect(other);
    }

    public Ip4Range[] IntersectableExcept(Ip4Range other)
    {
        if (other.FirstAddress <= this.FirstAddress)
        {
            if (other.LastAddress < this.LastAddress)
            {
                return [new Ip4Range(new Ip4Address((uint)other.LastAddress + 1), this.LastAddress)];
            }
            else
            {
                return [];
            }
        }
        else
        {
            if (other.LastAddress < this.LastAddress)
            {
                return [new Ip4Range(this.FirstAddress, new Ip4Address((uint)other.FirstAddress - 1)), new Ip4Range(new Ip4Address((uint)other.LastAddress + 1), this.LastAddress)];
            }
            else
            {
                return [new Ip4Range(this.FirstAddress, new Ip4Address((uint)other.FirstAddress - 1))];
            }
        }
    }

    public Ip4Range[] Except(Ip4Range other)
    {
        if (IsIntersects(other))
        {
            return IntersectableExcept(other);
        }

        return [this];
    }

    public Ip4Subnet[] ToSubnets()
    {
        List<Ip4Subnet> result = new List<Ip4Subnet>();
        SearchForBiggestSubnetWithin(this, result, 32);

        return result.ToArray();
    }

    private void SearchForBiggestSubnetWithin(Ip4Range ipRange, List<Ip4Subnet> result, int position)
    {
        if (position == 0)
        {
            result.Add(new Ip4Subnet(ipRange.FirstAddress, 32));
        }
        else
        {
            var startBit = GetNthBit(ipRange.FirstAddress.AsUInt32(), position);
            var endBit = GetNthBit(ipRange.LastAddress.AsUInt32(), position);
            if (startBit < endBit)
            {
                if (GetLastBits(ipRange.FirstAddress.AsUInt32(), position) == 0 && GetLastBits(ipRange.LastAddress.AsUInt32(), position) == GetLastBits(0xFFFFFFFF, position))
                {
                    result.Add(new Ip4Subnet(ipRange.FirstAddress, 32 - position));
                }
                else
                {
                    Ip4Address end1 = new Ip4Address(ipRange.FirstAddress.AsUInt32() | GetLastBits(0xFFFFFFFF, position - 1));
                    SearchForBiggestSubnetWithin(new Ip4Range(ipRange.FirstAddress, end1), result, position - 1);

                    Ip4Address start2 = new Ip4Address(GetFirstBits(ipRange.LastAddress.AsUInt32(), 32 - position + 1));
                    SearchForBiggestSubnetWithin(new Ip4Range(start2, ipRange.LastAddress), result, position - 1);
                }
            }
            else
            {
                SearchForBiggestSubnetWithin(ipRange, result, position - 1);
            }
        }
    }

    public Ip4RangeSet ToIp4RangeSet()
    {
        return new Ip4RangeSet(this);
    }

    public override string ToString()
    {
        return $"{FirstAddress}-{LastAddress}";
    }
}