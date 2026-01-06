using System.Diagnostics;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Range : IEquatable<Ip4Range>
{
    public static readonly Ip4Range All = new(Ip4Address.MinValue, Ip4Address.MaxValue);

    public Ip4Address FirstAddress { get; }
    public Ip4Address LastAddress { get; }

    public ulong Count => 1UL + LastAddress.ToUInt32() - FirstAddress.ToUInt32();

    public Ip4Range(Ip4Address start, Ip4Address end)
    {
        if (end < start)
            throw new ArgumentException("End address must be greater than or equal to start address.");

        FirstAddress = start;
        LastAddress = end;
    }

    public static int GeneralComparison(Ip4Range first, Ip4Range second)
    {
        if (first.LastAddress < second.FirstAddress)
            return -1;
        if (first.FirstAddress > second.LastAddress)
            return 1;
        return 0;
    }

    public static (int, int) OverlappingComparison(Ip4Range first, Ip4Range second)
    {
        return (first.FirstAddress.CompareTo(second.FirstAddress), first.LastAddress.CompareTo(second.LastAddress));
    }

    public static implicit operator Ip4RangeSet(Ip4Range range) => new(range);

    public static bool operator ==(Ip4Range left, Ip4Range right) => left.Equals(right);
    public static bool operator !=(Ip4Range left, Ip4Range right) => !left.Equals(right);

    public int GeneralComparison(Ip4Range second) => GeneralComparison(this, second);

    public (int, int) OverlappingComparison(Ip4Range second) => OverlappingComparison(this, second);

    public bool IsIntersects(Ip4Range other)
    {
        return other.FirstAddress <= LastAddress && other.LastAddress >= FirstAddress;
    }

    public Ip4Range IntersectableUnion(Ip4Range other)
    {
        var start = Ip4Address.Min(FirstAddress, other.FirstAddress);
        var end = Ip4Address.Max(LastAddress, other.LastAddress);
        return new Ip4Range(start, end);
    }

    public Ip4Range IntersectableIntersect(Ip4Range other)
    {
        var start = Ip4Address.Max(FirstAddress, other.FirstAddress);
        var end = Ip4Address.Min(LastAddress, other.LastAddress);
        return new Ip4Range(start, end);
    }

    public Ip4Range[] IntersectableExcept(Ip4Range other)
    {
        bool hasLeftPart = other.FirstAddress > FirstAddress && other.FirstAddress > Ip4Address.MinValue;
        bool hasRightPart = other.LastAddress < LastAddress && other.LastAddress < Ip4Address.MaxValue;

        if (hasLeftPart)
        {
            if (hasRightPart)
            {
                return [CreateLeftPart(other.FirstAddress), CreateRightPart(other.LastAddress)];
            }
            else
            {
                return [CreateLeftPart(other.FirstAddress)];
            }
        }
        else
        {
            if (hasRightPart)
            {
                return [CreateRightPart(other.LastAddress)];
            }
            else
            {
                return [];
            }
        }
    }

    public (bool hasLeft, bool hasRight) IntersectableExcept(Ip4Range other, out Ip4Range leftResult, out Ip4Range rightResult)
    {
        bool hasLeftPart = other.FirstAddress > FirstAddress && other.FirstAddress > Ip4Address.MinValue;
        bool hasRightPart = other.LastAddress < LastAddress && other.LastAddress < Ip4Address.MaxValue;

        if (hasLeftPart)
        {
            if (hasRightPart)
            {
                leftResult = CreateLeftPart(other.FirstAddress);
                rightResult = CreateRightPart(other.LastAddress);
                return (true, true);
            }
            else
            {
                leftResult = CreateLeftPart(other.FirstAddress);
                rightResult = default;
                return (true, false);
            }
        }
        else
        {
            if (hasRightPart)
            {
                leftResult = default;
                rightResult = CreateRightPart(other.LastAddress);
                return (false, true);
            }
            else
            {
                leftResult = default;
                rightResult = default;
                return (false, false);
            }
        }
    }

    private Ip4Range CreateLeftPart(Ip4Address otherStart)
    {
        return new Ip4Range(FirstAddress, new Ip4Address(otherStart.ToUInt32() - 1));
    }

    private Ip4Range CreateRightPart(Ip4Address otherEnd)
    {
        return new Ip4Range(new Ip4Address(otherEnd.ToUInt32() + 1), LastAddress);
    }

    public Ip4Subnet[] ToSubnets()
    {
        List<Ip4Subnet> result = [];
        FindSubnetsInRange(this, result, 32);
        return [.. result];
    }

    private static void FindSubnetsInRange(Ip4Range range, List<Ip4Subnet> result, int bitPosition)
    {
        if (bitPosition == 0)
        {
            result.Add(new Ip4Subnet(range.FirstAddress, Ip4Mask.SingleAddress));
            return;
        }

        uint startBit = GetBitAt(range.FirstAddress.ToUInt32(), bitPosition);
        uint endBit = GetBitAt(range.LastAddress.ToUInt32(), bitPosition);

        if (startBit < endBit)
        {
            bool isAlignedStart = GetTrailingBits(range.FirstAddress.ToUInt32(), bitPosition) == 0;
            bool isAlignedEnd = GetTrailingBits(range.LastAddress.ToUInt32(), bitPosition) == GetTrailingBits(uint.MaxValue, bitPosition);

            if (isAlignedStart && isAlignedEnd)
            {
                result.Add(new Ip4Subnet(range.FirstAddress, new Ip4Mask(32 - bitPosition)));
            }
            else
            {
                var midPoint = range.FirstAddress.ToUInt32() | GetTrailingBits(uint.MaxValue, bitPosition - 1);
                FindSubnetsInRange(new Ip4Range(range.FirstAddress, new Ip4Address(midPoint)), result, bitPosition - 1);

                var secondHalfStart = GetLeadingBits(range.LastAddress.ToUInt32(), 32 - bitPosition + 1);
                FindSubnetsInRange(new Ip4Range(new Ip4Address(secondHalfStart), range.LastAddress), result, bitPosition - 1);
            }
        }
        else
        {
            FindSubnetsInRange(range, result, bitPosition - 1);
        }
    }

    private static uint GetBitAt(uint value, int position)
    {
        return position == 0 ? value & 1 : (value >> (position - 1)) & 1;
    }

    private static uint GetTrailingBits(uint value, int count)
    {
        return count == 0 ? 0 : (value << (32 - count)) >> (32 - count);
    }

    private static uint GetLeadingBits(uint value, int count)
    {
        return count == 0 ? 0 : (value >> (32 - count)) << (32 - count);
    }

    public Ip4RangeSet ToIp4RangeSet() => new(this);

    public override string ToString() => $"{FirstAddress}-{LastAddress}";

    public bool Equals(Ip4Range other)
    {
        return FirstAddress.Equals(other.FirstAddress) && LastAddress.Equals(other.LastAddress);
    }

    public override bool Equals(object? obj) => obj is Ip4Range other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(FirstAddress, LastAddress);
}