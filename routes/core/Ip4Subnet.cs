using System.Diagnostics;

namespace routes.core;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Subnet
{
    public static implicit operator Ip4Range(Ip4Subnet subnet)
    {
        return subnet.ToIp4Range();
    }

    public static implicit operator Ip4RangeSet(Ip4Subnet subnet)
    {
        return subnet.ToIp4RangeSet();
    }

    public readonly Ip4Address FirstAddress;
    public readonly Ip4Mask Mask;

    public Ip4Subnet(Ip4Address address, Ip4Mask mask)
    {
        FirstAddress = address;
        Mask = mask;
    }

    public Ip4Subnet(Ip4Address firstAddress, int cidr)
    {
        FirstAddress = firstAddress;
        Mask = new Ip4Mask(cidr);
    }

    public Ip4Subnet(Ip4Address firstAddress, uint mask)
    {
        FirstAddress = firstAddress;
        Mask = new Ip4Mask(mask);
    }

    public Ip4Address LastAddress => new Ip4Address(FirstAddress.AsUInt32() | ~Mask.AsUInt32());

    public Ip4Range ToIp4Range()
    {
        return new Ip4Range(FirstAddress, LastAddress);
    }

    public Ip4RangeSet ToIp4RangeSet()
    {
        return new Ip4RangeSet(this);
    }

    public override string ToString()
    {
        return $"{FirstAddress}{Mask.ToCidrString()}";
    }
}