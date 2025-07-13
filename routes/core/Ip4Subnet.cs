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

    /// <param name="text">x.x.x.x/yy or x.x.x.x y.y.y.y</param>
    public static Ip4Subnet Parse(string text)
    {
        if (!TryParse(text, out var result))
        {
            throw new FormatException();
        }

        return result;
    }

    /// <param name="text">x.x.x.x/yy or x.x.x.x y.y.y.y</param>
    public static bool TryParse(string text, out Ip4Subnet result)
    {
        var step1 = text.Split('/', ' ');
        if (step1.Length == 2 && Ip4Address.TryParse(step1[0], out var address) && Ip4Mask.TryParse(step1[1], out var mask))
        {
            result = new Ip4Subnet(address, mask);
            return true;
        }

        result = default;
        return false;
    }

    public static bool IsValid(Ip4Mask mask, Ip4Address firstAddress)
    {
        return (firstAddress.AsUInt32() & ~mask.AsUInt32()) == 0;
    }

    public static void Validate(Ip4Mask mask, Ip4Address firstAddress)
    {
        if (!IsValid(mask, firstAddress))
        {
            throw new ArgumentException($"The first address {firstAddress} is not valid for the mask {mask}.", nameof(firstAddress));
        }
    }

    public readonly Ip4Address FirstAddress;
    public readonly Ip4Mask Mask;

    public Ip4Subnet(Ip4Address address, Ip4Mask mask)
    {
        FirstAddress = address;
        Mask = mask;

        Validate(Mask, FirstAddress);
    }

    public Ip4Subnet(Ip4Address firstAddress, int cidr)
    {
        FirstAddress = firstAddress;
        Mask = new Ip4Mask(cidr);

        Validate(Mask, FirstAddress);
    }

    public Ip4Subnet(Ip4Address firstAddress, uint mask)
    {
        FirstAddress = firstAddress;
        Mask = new Ip4Mask(mask);

        Validate(Mask, FirstAddress);
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

    public string ToFullString()
    {
        return $"{FirstAddress} {Mask.ToFullString()}";
    }

    public string ToCidrString()
    {
        return $"{FirstAddress}/{Mask.Cidr}";
    }

    public override string ToString() => ToCidrString();
}