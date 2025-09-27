using System.Diagnostics;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Range : IEquatable<Ip4Range>
{
    private static uint GetNthBit(uint number, int position)
    {
        return position == 0 ? number & 1 : (number >> (position - 1)) & 1;
    }

    private static uint GetLastBits(uint number, int count)
    {
        return count == 0 ? 0 : (number << (32 - count)) >> (32 - count);
    }

    private static uint GetFirstBits(uint number, int count)
    {
        return count == 0 ? 0 : (number >> (32 - count)) << (32 - count);
    }

    public static implicit operator Ip4RangeSet(Ip4Range range)
    {
        return new Ip4RangeSet(range);
    }

    public static readonly Ip4Range All = new(new Ip4Address(0x00000000), new Ip4Address(0xFFFFFFFF));

    public Ip4Address FirstAddress { get; }
    public Ip4Address LastAddress { get; }

    public ulong Count => 1UL + LastAddress.ToUInt32() - FirstAddress.ToUInt32();

    public Ip4Range(Ip4Address start, Ip4Address end)
    {
        if (end < start)
        {
            throw new ArgumentException("End address must be greater than or equal to start address.");
        }

        FirstAddress = start;
        LastAddress = end;
    }

    public bool IsIntersects(Ip4Range other)
    {
        return other.FirstAddress <= LastAddress && other.LastAddress >= FirstAddress;
    }

    public Ip4Range IntersectableUnion(Ip4Range other)
    {
        Ip4Address start = FirstAddress < other.FirstAddress ? FirstAddress : other.FirstAddress;
        Ip4Address end = LastAddress > other.LastAddress ? LastAddress : other.LastAddress;

        return new Ip4Range(start, end);
    }

    public Ip4Range[] Union(Ip4Range other)
    {
        return !IsIntersects(other) ? [this, other] : [IntersectableUnion(other)];
    }

    public Ip4Range IntersectableIntersect(Ip4Range other)
    {
        var start = Ip4Address.Max(FirstAddress, other.FirstAddress);
        var end = Ip4Address.Min(LastAddress, other.LastAddress);
        return new Ip4Range(start, end);
    }

    public Ip4Range? Intersect(Ip4Range other)
    {
        return !IsIntersects(other) ? null : IntersectableIntersect(other);
    }

    public Ip4Range[] IntersectableExcept(Ip4Range other)
    {
        return other.FirstAddress <= FirstAddress
            ? other.LastAddress < LastAddress ? [new Ip4Range(new Ip4Address((uint)other.LastAddress + 1), LastAddress)] : []
            : other.LastAddress < LastAddress
                ? [new Ip4Range(FirstAddress, new Ip4Address((uint)other.FirstAddress - 1)), new Ip4Range(new Ip4Address((uint)other.LastAddress + 1), LastAddress)]
                : [new Ip4Range(FirstAddress, new Ip4Address((uint)other.FirstAddress - 1))];
    }

    public Ip4Range[] Except(Ip4Range other)
    {
        return IsIntersects(other) ? IntersectableExcept(other) : [this];
    }

    public Ip4Subnet[] ToSubnets()
    {
        List<Ip4Subnet> result = [];
        SearchSubnetыWithinRange(this, result, 32);

        return result.ToArray();
    }

    private static void SearchSubnetыWithinRange(Ip4Range ipRange, List<Ip4Subnet> result, int position)
    {
        if (position == 0)
        {
            result.Add(new Ip4Subnet(ipRange.FirstAddress, 32));
        }
        else
        {
            uint startBit = GetNthBit(ipRange.FirstAddress.ToUInt32(), position);
            uint endBit = GetNthBit(ipRange.LastAddress.ToUInt32(), position);
            if (startBit < endBit)
            {
                if (GetLastBits(ipRange.FirstAddress.ToUInt32(), position) == 0 && GetLastBits(ipRange.LastAddress.ToUInt32(), position) == GetLastBits(0xFFFFFFFF, position))
                {
                    result.Add(new Ip4Subnet(ipRange.FirstAddress, 32 - position));
                }
                else
                {
                    Ip4Address end1 = new(ipRange.FirstAddress.ToUInt32() | GetLastBits(0xFFFFFFFF, position - 1));
                    SearchSubnetыWithinRange(new Ip4Range(ipRange.FirstAddress, end1), result, position - 1);

                    Ip4Address start2 = new(GetFirstBits(ipRange.LastAddress.ToUInt32(), 32 - position + 1));
                    SearchSubnetыWithinRange(new Ip4Range(start2, ipRange.LastAddress), result, position - 1);
                }
            }
            else
            {
                SearchSubnetыWithinRange(ipRange, result, position - 1);
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

    public override bool Equals(object? obj)
    {
        return obj is Ip4Range other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstAddress, LastAddress);
    }

    public static bool operator ==(Ip4Range left, Ip4Range right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ip4Range left, Ip4Range right)
    {
        return !left.Equals(right);
    }

    public bool Equals(Ip4Range other)
    {
        return FirstAddress.Equals(other.FirstAddress) && LastAddress.Equals(other.LastAddress);
    }
}