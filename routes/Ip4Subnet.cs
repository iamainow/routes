using System.Diagnostics;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Subnet : IEquatable<Ip4Subnet>
{
    public static implicit operator Ip4Range(Ip4Subnet subnet)
    {
        return subnet.ToIp4Range();
    }

    public static implicit operator Ip4RangeSet2(Ip4Subnet subnet)
    {
        return subnet.ToIp4RangeSet2();
    }

    /// <param name="text">x.x.x.x/yy or x.x.x.x y.y.y.y</param>
    public static Ip4Subnet Parse(string text)
    {
        return !TryParse(text, out Ip4Subnet result) ? throw new FormatException() : result;
    }

    /// <param name="text">x.x.x.x/yy or x.x.x.x y.y.y.y</param>
    public static bool TryParse(string text, out Ip4Subnet result)
    {
        ArgumentNullException.ThrowIfNull(text);

        string[] step1 = text.Split('/', ' ');
        if (step1.Length == 2 && Ip4Address.TryParse(step1[0], out Ip4Address address) && Ip4Mask.TryParse(step1[1], out Ip4Mask mask))
        {
            result = new Ip4Subnet(address, mask);
            return true;
        }

        result = default;
        return false;
    }

    public static bool IsValid(Ip4Mask mask, Ip4Address firstAddress)
    {
        return (firstAddress.ToUInt32() & ~mask.AsUInt32()) == 0;
    }

    public static void Validate(Ip4Mask mask, Ip4Address firstAddress)
    {
        if (!IsValid(mask, firstAddress))
        {
            throw new ArgumentException($"The first address {firstAddress} is not valid for the mask {mask}.", nameof(firstAddress));
        }
    }

    public static readonly Ip4Subnet All = new(new Ip4Address(0x00000000), Ip4Mask.All);

    public Ip4Address FirstAddress { get; }
    public Ip4Mask Mask { get; }

    public Ip4Address LastAddress => new(FirstAddress.ToUInt32() | ~Mask.AsUInt32());
    public ulong Count => Mask.Count;

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

    public Ip4Range ToIp4Range()
    {
        return new Ip4Range(FirstAddress, LastAddress);
    }

    public Ip4RangeSet2 ToIp4RangeSet2()
    {
        return new Ip4RangeSet2(this);
    }

    public string ToFullString()
    {
        return $"{FirstAddress} {Mask.ToFullString()}";
    }

    public string ToCidrString()
    {
        return $"{FirstAddress}/{Mask.Cidr}";
    }

    public override string ToString()
    {
        return ToCidrString();
    }

    public override bool Equals(object? obj)
    {
        return obj is Ip4Subnet other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstAddress, Mask);
    }

    public static bool operator ==(Ip4Subnet left, Ip4Subnet right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ip4Subnet left, Ip4Subnet right)
    {
        return !(left == right);
    }

    public bool Equals(Ip4Subnet other)
    {
        return FirstAddress.Equals(other.FirstAddress) && Mask.Equals(other.Mask);
    }

    public Ip4Subnet GetSupernet()
    {
        if (!HasSupernet())
        {
            throw new InvalidOperationException("The subnet is the all-encompassing subnet and has no super-subnet.");
        }
        var mask = new Ip4Mask(Mask.Cidr - 1);
        return new Ip4Subnet(new Ip4Address(FirstAddress.ToUInt32() & mask.AsUInt32()), mask);
    }

    public bool HasSupernet()
    {
        return this.Mask != Ip4Mask.All;
    }

    public Ip4Subnet[] GetSubnets()
    {
        if (!HasSubnets())
        {
            throw new InvalidOperationException("The subnet is a single address and has no subnets.");
        }

        var newMask = new Ip4Mask(Mask.Cidr + 1);
        var firstSubnet = new Ip4Subnet(FirstAddress, newMask);
        var secondSubnet = new Ip4Subnet(new Ip4Address(firstSubnet.LastAddress.ToUInt32() + 1), newMask);
        return [firstSubnet, secondSubnet];
    }

    public bool HasSubnets()
    {
        return this.Mask != Ip4Mask.SingleAddress;
    }
}