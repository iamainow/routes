namespace routes.core;

public readonly struct Ip4Subnet
{
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

    public Ip4Address LastAddress => FirstAddress | ~Mask;

    public Ip4Range ToIpRange()
    {
        return new Ip4Range(FirstAddress, LastAddress);
    }

    public override string ToString()
    {
        return $"{FirstAddress} {Mask}";
    }
}