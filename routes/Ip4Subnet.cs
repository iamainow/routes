using System.Diagnostics;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Subnet : IEquatable<Ip4Subnet>
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
    public static Ip4Subnet Parse(scoped ReadOnlySpan<char> text)
    {
        return TryParse(text, out Ip4Subnet result) ? result : throw new FormatException();
    }

    /// <param name="text">x.x.x.x/yy or x.x.x.x y.y.y.y</param>
    public static bool TryParse(scoped ReadOnlySpan<char> text, out Ip4Subnet result)
    {
        ReadOnlySpan<char> separators = ['/', ' '];
        var enumerator = text.SplitAny(separators);
        if (!enumerator.MoveNext())
        {
            result = default;
            return false;
        }
        if (!Ip4Address.TryParse(text[enumerator.Current], out Ip4Address address))
        {
            result = default;
            return false;
        }

        if (!enumerator.MoveNext())
        {
            result = default;
            return false;
        }
        if (!Ip4Mask.TryParse(text[enumerator.Current], out Ip4Mask mask))
        {
            result = default;
            return false;
        }

        if (enumerator.MoveNext())
        {
            result = default;
            return false;
        }
        else
        {
            result = new Ip4Subnet(address, mask);
            return true;
        }
    }

    public static bool IsValid(Ip4Mask mask, Ip4Address firstAddress)
    {
        return (firstAddress.ToUInt32() & ~mask.ToUInt32()) == 0;
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

    public Ip4Address LastAddress => new(FirstAddress.ToUInt32() | ~Mask.ToUInt32());
    public ulong Count => Mask.Count;

    public Ip4Subnet(Ip4Address address, Ip4Mask mask)
    {
        FirstAddress = address;
        Mask = mask;

        Validate(Mask, FirstAddress);
    }

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
        return new Ip4Subnet(new Ip4Address(FirstAddress.ToUInt32() & mask.ToUInt32()), mask);
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