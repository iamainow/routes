using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Address : IEquatable<Ip4Address>,
    IComparable<Ip4Address>,
    IMinMaxValue<Ip4Address>,
    IAdditionOperators<Ip4Address, Ip4Address, Ip4Address>,
    IAdditionOperators<Ip4Address, uint, Ip4Address>,
    ISubtractionOperators<Ip4Address, Ip4Address, Ip4Address>,
    ISubtractionOperators<Ip4Address, uint, Ip4Address>,
    IEqualityOperators<Ip4Address, Ip4Address, bool>,
    IComparisonOperators<Ip4Address, Ip4Address, bool>
{
    private static readonly Ip4Address minValue = new(0x00000000);
    private static readonly Ip4Address maxValue = new(0xFFFFFFFF);

    public static Ip4Address MinValue => minValue;
    public static Ip4Address MaxValue => maxValue;

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

    public Ip4Address(scoped ReadOnlySpan<byte> bytes)
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

    public static Ip4Address Parse(scoped ReadOnlySpan<char> text)
    {
        if (TryParse(text, out var result))
        {
            return result;
        }

        throw new FormatException();
    }

    public static bool TryParse(scoped ReadOnlySpan<char> text, out Ip4Address result)
    {
        Span<byte> bytes = stackalloc byte[4];
        if (!TryParseOctets(text, bytes))
        {
            result = default;
            return false;
        }

        result = new Ip4Address(bytes[0], bytes[1], bytes[2], bytes[3]);
        return true;
    }

    private static bool TryParseOctets(scoped ReadOnlySpan<char> text, Span<byte> bytes)
    {
        var enumerator = text.Split('.');
        int i = 0;

        foreach (var range in enumerator)
        {
            if (i >= 4 || !byte.TryParse(text[range], out bytes[i]))
            {
                return false;
            }

            i++;
        }

        return i == 4;
    }

    public static Ip4Address Min(Ip4Address left, Ip4Address right) => left <= right ? left : right;

    public static Ip4Address Max(Ip4Address left, Ip4Address right) => left >= right ? left : right;

    public static Ip4Address FromIPAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return new Ip4Address(address.GetAddressBytes());
    }

    public static bool operator <=(Ip4Address left, Ip4Address right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Ip4Address left, Ip4Address right) => left.CompareTo(right) >= 0;
    public static bool operator <(Ip4Address left, Ip4Address right) => left.CompareTo(right) < 0;
    public static bool operator >(Ip4Address left, Ip4Address right) => left.CompareTo(right) > 0;
    public static bool operator ==(Ip4Address left, Ip4Address right) => left.Equals(right);
    public static bool operator !=(Ip4Address left, Ip4Address right) => !left.Equals(right);

    public static explicit operator uint(Ip4Address address) => address.ToUInt32();
    public static implicit operator Ip4Range(Ip4Address address) => address.ToIp4Range();
    public static implicit operator Ip4Subnet(Ip4Address address) => address.ToIp4Subnet();
    public static implicit operator IPAddress(Ip4Address address) => address.ToIPAddress();
    public static implicit operator Ip4Address(IPAddress address) => FromIPAddress(address);

    public static Ip4Address Add(Ip4Address left, Ip4Address right)
    {
        return new Ip4Address(left._address + right._address);
    }

    public static Ip4Address Add(Ip4Address left, uint right)
    {
        return new Ip4Address(left._address + right);
    }

    public static Ip4Address Subtract(Ip4Address left, Ip4Address right)
    {
        return new Ip4Address(left._address - right._address);
    }

    public static Ip4Address Subtract(Ip4Address left, uint right)
    {
        return new Ip4Address(left._address - right);
    }

    public static Ip4Address operator +(Ip4Address left, Ip4Address right) => Add(left, right);
    public static Ip4Address operator +(Ip4Address left, uint right) => Add(left, right);
    public static Ip4Address operator -(Ip4Address left, Ip4Address right) => Subtract(left, right);
    public static Ip4Address operator -(Ip4Address left, uint right) => Subtract(left, right);

    public uint ToUInt32() => _address;

    public byte[] ToByteArray() => [_byte1, _byte2, _byte3, _byte4];

    public int CompareTo(Ip4Address other) => _address.CompareTo(other._address);

    public bool Equals(Ip4Address other) => _address.Equals(other._address);

    public override bool Equals(object? obj) => obj is Ip4Address address && Equals(address);

    public override int GetHashCode() => _address.GetHashCode();

    public Ip4Range ToIp4Range() => new(this, this);

    public Ip4Subnet ToIp4Subnet() => new(this, Ip4Mask.SingleAddress);

    public IPAddress ToIPAddress() => new([_byte1, _byte2, _byte3, _byte4]);

    public override string ToString() => $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";

    public static bool IsSortedAscendingSIMD(ReadOnlySpan<Ip4Address> addresses)
    {
        if (addresses.Length <= 1)
        {
            return true;
        }

        int vectorSize = Vector<uint>.Count;

        var uints = MemoryMarshal.Cast<Ip4Address, uint>(addresses);

        // Process in vector-sized chunks, comparing current vector with next shifted
        int i = 0;
        for (; i < addresses.Length - vectorSize; i += vectorSize)
        {
            // Load current and next vectors
            var current = new Vector<uint>(uints.Slice(i, vectorSize));
            var next = new Vector<uint>(uints.Slice(i + 1, vectorSize));

            if (!Vector.LessThanAll(current, next))
            {
                return false;
            }
        }

        // Handle remaining elements scalarly
        for (; i < addresses.Length - 1; i++)
        {
            if (addresses[i]._address > addresses[i + 1]._address)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsSortedAscending(ReadOnlySpan<Ip4Address> addresses)
    {
        for (int i = 0; i < addresses.Length - 1; i++)
        {
            if (addresses[i]._address > addresses[i + 1]._address)
            {
                return false;
            }
        }

        return true;
    }
}