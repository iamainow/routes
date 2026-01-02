using System.Diagnostics;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Ip4Subnet : IEquatable<Ip4Subnet>
{
    public static readonly Ip4Subnet All = new(Ip4Address.MinValue, Ip4Mask.All);

    public Ip4Address FirstAddress { get; }
    public Ip4Mask Mask { get; }

    public Ip4Address LastAddress => new(FirstAddress.ToUInt32() | ~Mask.ToUInt32());
    public ulong Count => Mask.Count;

    public Ip4Subnet(Ip4Address address, Ip4Mask mask)
    {
        Validate(mask, address);
        FirstAddress = address;
        Mask = mask;
    }

    public static Ip4Subnet Parse(scoped ReadOnlySpan<char> text)
    {
        if (TryParse(text, out var result))
            return result;

        throw new FormatException();
    }

    public static bool TryParse(scoped ReadOnlySpan<char> text, out Ip4Subnet result)
    {
        ReadOnlySpan<char> separators = ['/', ' '];
        var enumerator = text.SplitAny(separators);

        if (!enumerator.MoveNext() || !Ip4Address.TryParse(text[enumerator.Current], out var address))
        {
            result = default;
            return false;
        }

        if (!enumerator.MoveNext() || !Ip4Mask.TryParse(text[enumerator.Current], out var mask))
        {
            result = default;
            return false;
        }

        if (enumerator.MoveNext())
        {
            result = default;
            return false;
        }

        result = new Ip4Subnet(address, mask);
        return true;
    }

    public static bool IsValid(Ip4Mask mask, Ip4Address firstAddress)
    {
        return (firstAddress.ToUInt32() & ~mask.ToUInt32()) == 0;
    }

    public static void Validate(Ip4Mask mask, Ip4Address firstAddress)
    {
        if (!IsValid(mask, firstAddress))
            throw new ArgumentException($"Address {firstAddress} is not valid for mask {mask}.", nameof(firstAddress));
    }

    public static implicit operator Ip4Range(Ip4Subnet subnet) => subnet.ToIp4Range();
    public static implicit operator Ip4RangeSet(Ip4Subnet subnet) => subnet.ToIp4RangeSet();

    public static bool operator ==(Ip4Subnet left, Ip4Subnet right) => left.Equals(right);
    public static bool operator !=(Ip4Subnet left, Ip4Subnet right) => !left.Equals(right);

    public Ip4Range ToIp4Range() => new(FirstAddress, LastAddress);

    public Ip4RangeSet ToIp4RangeSet() => new(this);

    public bool HasSupernet() => Mask != Ip4Mask.All;

    public Ip4Subnet GetSupernet()
    {
        if (!HasSupernet())
            throw new InvalidOperationException("The subnet is the all-encompassing subnet and has no supernet.");

        var supernetMask = new Ip4Mask(Mask.Cidr - 1);
        var supernetAddress = new Ip4Address(FirstAddress.ToUInt32() & supernetMask.ToUInt32());
        return new Ip4Subnet(supernetAddress, supernetMask);
    }

    public bool HasSubnets() => Mask != Ip4Mask.SingleAddress;

    public Ip4Subnet[] GetSubnets()
    {
        if (!HasSubnets())
            throw new InvalidOperationException("The subnet is a single address and has no subnets.");

        var subnetMask = new Ip4Mask(Mask.Cidr + 1);
        var firstSubnet = new Ip4Subnet(FirstAddress, subnetMask);
        var secondSubnet = new Ip4Subnet(new Ip4Address(firstSubnet.LastAddress.ToUInt32() + 1), subnetMask);
        return [firstSubnet, secondSubnet];
    }

    public string ToFullString() => $"{FirstAddress} {Mask.ToFullString()}";

    public string ToCidrString() => $"{FirstAddress}/{Mask.Cidr}";

    public override string ToString() => ToCidrString();

    public bool Equals(Ip4Subnet other)
    {
        return FirstAddress.Equals(other.FirstAddress) && Mask.Equals(other.Mask);
    }

    public override bool Equals(object? obj) => obj is Ip4Subnet other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(FirstAddress, Mask);
}