namespace routes.core;

public readonly struct Ip4Subnet
{
    private readonly Ip4Address _address;
    private readonly Ip4Mask _mask;

    public Ip4Subnet(Ip4Address address, Ip4Mask mask)
    {
        _address = address;
        _mask = mask;
    }

    public Ip4Subnet(Ip4Address firstAddress, Ip4Address lastAddress)
    {
        _address = firstAddress;
        _mask = new Ip4Mask(~(firstAddress.AsUInt32() ^ lastAddress.AsUInt32()));
    }

    public Ip4Address FirstAddress => _address;

    public Ip4Address LastAddress => _address | ~_mask;

    public Ip4Mask Mask => _mask;
}