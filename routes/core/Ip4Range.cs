namespace routes.core;

public readonly struct Ip4Range
{
    public readonly Ip4Address FirstAddress;
    public readonly Ip4Address LastAddress;
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

    public Ip4Subnet[] ToSubnets()
    {
        List<Ip4Subnet> result = new List<Ip4Subnet>();
        SearchForBiggestSubnetWithin(this, result, 32);

        return result.ToArray();
    }

    private void SearchForBiggestSubnetWithin(Ip4Range ipRange, List<Ip4Subnet> result, int bit)
    {
        if (bit == 0)
        {
            result.Add(new Ip4Subnet(ipRange.FirstAddress, 32));
        }
        else
        {
            var startBit = GetNthBit(ipRange.FirstAddress.AsUInt32(), bit);
            var endBit = GetNthBit(ipRange.LastAddress.AsUInt32(), bit);
            if (startBit < endBit)
            {
                if (GetLastNthBits(ipRange.FirstAddress.AsUInt32(), bit) == 0 && GetLastNthBits(ipRange.LastAddress.AsUInt32(), bit) == GetLastNthBits(0xFFFFFFFF, bit))
                {
                    result.Add(new Ip4Subnet(ipRange.FirstAddress, 32 - bit));
                }

                Ip4Address end1 = new Ip4Address(ipRange.FirstAddress.AsUInt32() | GetLastNthBits(0xFFFFFFFF, bit - 1));
                SearchForBiggestSubnetWithin(new Ip4Range(ipRange.LastAddress, end1), result, bit - 1);

                Ip4Address start2 = new Ip4Address(GetFirstNthBits(ipRange.LastAddress.AsUInt32(), bit));
                SearchForBiggestSubnetWithin(new Ip4Range(start2, ipRange.LastAddress), result, bit - 1);
            }
            else
            {
                SearchForBiggestSubnetWithin(ipRange, result, bit - 1);
            }
        }
    }

    private static uint GetNthBit(uint number, int bit)
    {
        if (bit == 0)
        {
            return number & 1;
        }

        return (number >> (bit - 1)) & 1;
    }

    private static uint GetLastNthBits(uint number, int bit)
    {
        if (bit == 0)
        {
            return 0;
        }

        return (number << (32 - bit)) >> (32 - bit);
    }

    private static uint GetFirstNthBits(uint number, int bit)
    {
        if (bit == 0)
        {
            return 0;
        }

        return (number >> (32 - bit)) << (32 - bit);
    }
}
