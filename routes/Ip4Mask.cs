using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Mask : IEquatable<Ip4Mask>
{
    public static readonly Ip4Mask All = new(0);
    public static readonly Ip4Mask SingleAddress = new(32);

    [FieldOffset(0)]
    private readonly uint _mask;

    [FieldOffset(3)]
    private readonly byte _byte1;
    [FieldOffset(2)]
    private readonly byte _byte2;
    [FieldOffset(1)]
    private readonly byte _byte3;
    [FieldOffset(0)]
    private readonly byte _byte4;

    public int Cidr => 32 - BitOperations.TrailingZeroCount(_mask);

    public ulong Count => 0x100000000UL - _mask;

    public Ip4Mask(int cidr)
    {
        if (cidr < 0 || cidr > 32)
            throw new ArgumentOutOfRangeException(nameof(cidr), cidr, "CIDR must be between 0 and 32");

        _mask = cidr == 0 ? 0U : ~0U << (32 - cidr);
    }

    public Ip4Mask(uint mask)
    {
        ValidateMask(mask);
        _mask = mask;
    }

    public Ip4Mask(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        _byte1 = byte1;
        _byte2 = byte2;
        _byte3 = byte3;
        _byte4 = byte4;
        ValidateMask(_mask);
    }

    public Ip4Mask(scoped ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 4)
            throw new ArgumentException("Byte array must contain exactly 4 bytes", nameof(bytes));

        _byte1 = bytes[0];
        _byte2 = bytes[1];
        _byte3 = bytes[2];
        _byte4 = bytes[3];
        ValidateMask(_mask);
    }

    public static Ip4Mask Parse(scoped ReadOnlySpan<char> text)
    {
        if (TryParse(text, out var result))
            return result;

        throw new FormatException();
    }

    public static bool TryParse(scoped ReadOnlySpan<char> text, out Ip4Mask result)
    {
        return TryParseCidrString(text, out result) || TryParseFullString(text, out result);
    }

    public static Ip4Mask ParseCidrString(scoped ReadOnlySpan<char> text)
    {
        if (TryParseCidrString(text, out var result))
            return result;

        throw new FormatException();
    }

    public static bool TryParseCidrString(scoped ReadOnlySpan<char> text, out Ip4Mask result)
    {
        if (text.StartsWith('/'))
            text = text[1..];

        if (!int.TryParse(text, out int cidr) || cidr < 0 || cidr > 32)
        {
            result = default;
            return false;
        }

        result = new Ip4Mask(cidr);
        return true;
    }

    public static Ip4Mask ParseFullString(scoped ReadOnlySpan<char> text)
    {
        if (TryParseFullString(text, out var result))
            return result;

        throw new FormatException();
    }

    public static bool TryParseFullString(scoped ReadOnlySpan<char> text, out Ip4Mask result)
    {
        Span<byte> bytes = stackalloc byte[4];
        if (!TryParseOctets(text, bytes))
        {
            result = default;
            return false;
        }

        result = new Ip4Mask(bytes);
        return true;
    }

    private static bool TryParseOctets(scoped ReadOnlySpan<char> text, Span<byte> bytes)
    {
        var enumerator = text.Split('.');
        int i = 0;

        foreach (Range range in enumerator)
        {
            if (i >= 4 || !byte.TryParse(text[range], out bytes[i]))
                return false;
            i++;
        }

        return i == 4;
    }

    private static bool IsValidMask(uint mask)
    {
        return BitOperations.TrailingZeroCount(mask) + BitOperations.LeadingZeroCount(~mask) == 32;
    }

    private static void ValidateMask(uint mask)
    {
        if (!IsValidMask(mask))
            throw new ArgumentException($"Invalid mask value: 0x{mask:X8}", nameof(mask));
    }

    public static Ip4Mask FromIPAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return new Ip4Mask(address.GetAddressBytes());
    }

    public static bool operator ==(Ip4Mask left, Ip4Mask right) => left.Equals(right);
    public static bool operator !=(Ip4Mask left, Ip4Mask right) => !left.Equals(right);

    public static implicit operator IPAddress(Ip4Mask mask) => mask.ToIPAddress();
    public static implicit operator Ip4Mask(IPAddress address) => FromIPAddress(address);

    public uint ToUInt32() => _mask;

    public byte[] ToByteArray() => [_byte1, _byte2, _byte3, _byte4];

    public IPAddress ToIPAddress() => new([_byte1, _byte2, _byte3, _byte4]);

    public string ToFullString() => $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";

    public string ToCidrString() => $"/{Cidr}";

    public override string ToString() => ToCidrString();

    public bool Equals(Ip4Mask other) => _mask.Equals(other._mask);

    public override bool Equals(object? obj) => obj is Ip4Mask other && Equals(other);

    public override int GetHashCode() => _mask.GetHashCode();
}