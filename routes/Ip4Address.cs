using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Address : IComparable<Ip4Address>, IEquatable<Ip4Address>
{
    /// <param name="text">x.x.x.x format</param>
    /// <exception cref="FormatException"></exception>
    public static Ip4Address Parse(ReadOnlySpan<char> text)
    {
        if (TryParse(text, out Ip4Address result))
        {
            return result;
        }

        throw new FormatException();
    }

    public static bool TryParse(ReadOnlySpan<char> text, out Ip4Address result)
    {
        Span<byte> bytes = stackalloc byte[4];
        var enumerator = text.Split('.');
        int i = 0;
        foreach (var range in enumerator)
        {
            if (i >= 4 || !byte.TryParse(text[range], out bytes[i]))
            {
                result = default;
                return false;
            }
            i++;
        }
        if (i != 4)
        {
            result = default;
            return false;
        }

        result = new Ip4Address(bytes[0], bytes[1], bytes[2], bytes[3]);
        return true;
    }

    public static Ip4Address Min(Ip4Address left, Ip4Address right)
    {
        return left < right ? left : right;
    }

    public static Ip4Address Max(Ip4Address left, Ip4Address right)
    {
        return left > right ? left : right;
    }

    public static bool operator <=(Ip4Address left, Ip4Address right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(Ip4Address left, Ip4Address right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(Ip4Address left, Ip4Address right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(Ip4Address left, Ip4Address right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator ==(Ip4Address left, Ip4Address right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ip4Address left, Ip4Address right)
    {
        return !left.Equals(right);
    }

    public static explicit operator uint(Ip4Address address)
    {
        return address.ToUInt32();
    }

    public static implicit operator Ip4Range(Ip4Address address)
    {
        return address.ToIp4Range();
    }

    public static implicit operator Ip4Subnet(Ip4Address address)
    {
        return address.ToIp4Subnet();
    }

    public static implicit operator Ip4RangeSet(Ip4Address address)
    {
        return address.ToIp4RangeSet();
    }

    public static implicit operator IPAddress(Ip4Address address)
    {
        return address.ToIPAddress();
    }

    public static implicit operator Ip4Address(IPAddress address)
    {
        return FromIPAddress(address);
    }

    [FieldOffset(0)]
    private readonly uint _address;

    [FieldOffset(3)]
    private readonly byte _byte1;
    [FieldOffset(2)]
    private readonly byte _byte2;
    [FieldOffset(1)]
    private readonly byte _byte3;
    [FieldOffset(0)]
    private readonly byte _byte4;

    public Ip4Address(uint address)
    {
        _address = address;
    }

    public Ip4Address(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        _byte1 = byte1;
        _byte2 = byte2;
        _byte3 = byte3;
        _byte4 = byte4;
    }

    public Ip4Address(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 4)
        {
            throw new ArgumentException("Byte array must contain exactly 4 bytes", nameof(bytes));
        }

        _byte1 = bytes[0];
        _byte2 = bytes[1];
        _byte3 = bytes[2];
        _byte4 = bytes[3];
    }

    public uint ToUInt32()
    {
        return _address;
    }

    public byte[] AsByteArray()
    {
        return [_byte1, _byte2, _byte3, _byte4];
    }

    public int CompareTo(Ip4Address other)
    {
        return _address.CompareTo(other._address);
    }

    public bool Equals(Ip4Address other)
    {
        return _address.Equals(other._address);
    }

    public override bool Equals(object? obj)
    {
        return obj is Ip4Address address && Equals(address);
    }

    public override int GetHashCode()
    {
        return _address.GetHashCode();
    }

    public Ip4Range ToIp4Range()
    {
        return new Ip4Range(this, this);
    }

    public Ip4Subnet ToIp4Subnet()
    {
        return new Ip4Subnet(this, Ip4Mask.SingleAddress);
    }

    public Ip4RangeSet ToIp4RangeSet()
    {
        return new Ip4RangeSet(this);
    }

    public override string ToString()
    {
        return $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";
    }

    public static Ip4Address FromIPAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return new Ip4Address(address.GetAddressBytes());
    }

    public IPAddress ToIPAddress()
    {
        return new IPAddress([_byte1, _byte2, _byte3, _byte4]);
    }
}