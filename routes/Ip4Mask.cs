using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;

namespace routes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Mask : IEquatable<Ip4Mask>
{
    public static readonly Ip4Mask All = new(0x00000000);
    public static readonly Ip4Mask SingleAddress = new(0xFFFFFFFF);

    /// <param name="text">x.x.x.x format</param>
    /// <exception cref="FormatException"></exception>
    public static Ip4Mask ParseFullString(string text)
    {
        return !TryParseFullString(text, out Ip4Mask result) ? throw new FormatException() : result;
    }

    /// <param name="text">x.x.x.x format</param>
    public static bool TryParseFullString(string text, out Ip4Mask result)
    {
        ArgumentNullException.ThrowIfNull(text);
        string[] step1 = text.Split('.');
        if (step1.Length != 4)
        {
            result = default;
            return false;
        }

        List<byte> step2 = new(4);
        foreach (string step1Item in step1)
        {
            if (!byte.TryParse(step1Item, out byte step2Item))
            {
                result = default;
                return false;
            }

            step2.Add(step2Item);
        }

        result = new Ip4Mask(step2[0], step2[1], step2[2], step2[3]);
        return true;
    }

    /// <param name="text">/xx or xx format</param>
    public static Ip4Mask ParseCidrString(string text)
    {
        return !TryParseCidrString(text, out Ip4Mask mask) ? throw new FormatException() : mask;
    }

    /// <param name="text">/xx or xx format</param>
    public static bool TryParseCidrString(string text, out Ip4Mask result)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.StartsWith('/'))
        {
            text = text[1..];
        }

        if (!int.TryParse(text, out int cidr))
        {
            result = default;
            return false;
        }

        if (cidr is < 0 or > 32)
        {
            result = default;
            return false;
        }

        result = new Ip4Mask(cidr);
        return true;
    }

    /// <param name="text">/xx, xx or x.x.x.x</param>
    public static Ip4Mask Parse(string text)
    {
        return !TryParse(text, out Ip4Mask result) ? throw new FormatException() : result;
    }

    /// <param name="text">/xx, xx or x.x.x.x</param>
    public static bool TryParse(string text, out Ip4Mask result)
    {
        if (TryParseCidrString(text, out Ip4Mask result2))
        {
            result = result2;
            return true;
        }

        if (TryParseFullString(text, out Ip4Mask result1))
        {
            result = result1;
            return true;
        }

        result = default;
        return false;
    }

    /// <exception cref="ArgumentException"></exception>
    private static uint GetMaskByCidr(int cidr)
    {
        return cidr switch
        {
            0 => 0x00000000,
            1 => 0x80000000,
            2 => 0xC0000000,
            3 => 0xE0000000,
            4 => 0xF0000000,
            5 => 0xF8000000,
            6 => 0xFC000000,
            7 => 0xFE000000,
            8 => 0xFF000000,
            9 => 0xFF800000,
            10 => 0xFFC00000,
            11 => 0xFFE00000,
            12 => 0xFFF00000,
            13 => 0xFFF80000,
            14 => 0xFFFC0000,
            15 => 0xFFFE0000,
            16 => 0xFFFF0000,
            17 => 0xFFFF8000,
            18 => 0xFFFFC000,
            19 => 0xFFFFE000,
            20 => 0xFFFFF000,
            21 => 0xFFFFF800,
            22 => 0xFFFFFC00,
            23 => 0xFFFFFE00,
            24 => 0xFFFFFF00,
            25 => 0xFFFFFF80,
            26 => 0xFFFFFFC0,
            27 => 0xFFFFFFE0,
            28 => 0xFFFFFFF0,
            29 => 0xFFFFFFF8,
            30 => 0xFFFFFFFC,
            31 => 0xFFFFFFFE,
            32 => 0xFFFFFFFF,
            _ => throw new ArgumentException($"cidr invalid value: {cidr}", nameof(cidr))
        };
    }

    private static bool IsValidMask(uint mask)
    {
        return BitOperations.TrailingZeroCount(mask) + BitOperations.LeadingZeroCount(~mask) == 32;
    }

    /// <exception cref="ArgumentException"></exception>
    private static void ValidateMask(uint mask)
    {
        if (!IsValidMask(mask))
        {
            throw new ArgumentException($"mask invalid value: {mask:x2}", nameof(mask));
        }
    }
    private static int GetCidrByMask(uint mask)
    {
        return 32 - BitOperations.TrailingZeroCount(mask);
    }

    public static Ip4Mask FromIPAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return new Ip4Mask(address.GetAddressBytes());
    }

    public static implicit operator IPAddress(Ip4Mask mask)
    {
        return mask.ToIPAddress();
    }

    public static implicit operator Ip4Mask(IPAddress address)
    {
        return FromIPAddress(address);
    }

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

    public int Cidr => GetCidrByMask(_mask);

    public ulong Count => 0x100000000UL - _mask;

    /// <exception cref="ArgumentException"></exception>
    public Ip4Mask(uint mask)
    {
        _mask = mask;
        ValidateMask(mask);
    }

    /// <exception cref="ArgumentException"></exception>
    public Ip4Mask(int cidr)
    {
        _mask = GetMaskByCidr(cidr);
    }

    /// <exception cref="ArgumentException"></exception>
    public Ip4Mask(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        _byte1 = byte1;
        _byte2 = byte2;
        _byte3 = byte3;
        _byte4 = byte4;
        ValidateMask(_mask);
    }

    /// <exception cref="ArgumentException"></exception>
    public Ip4Mask(byte[] bytes)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes), "Byte array cannot be null");
        }

        if (bytes.Length != 4)
        {
            throw new ArgumentException("Byte array must contain exactly 4 bytes", nameof(bytes));
        }

        _byte1 = bytes[0];
        _byte2 = bytes[1];
        _byte3 = bytes[2];
        _byte4 = bytes[3];
        ValidateMask(_mask);
    }

    public uint AsUInt32()
    {
        return _mask;
    }

    public byte[] AsByteArray()
    {
        return [_byte1, _byte2, _byte3, _byte4];
    }

    public IPAddress ToIPAddress()
    {
        return new IPAddress([_byte1, _byte2, _byte3, _byte4]);
    }

    public string ToFullString()
    {
        return $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";
    }
    public string ToCidrString()
    {
        return $"/{Cidr}";
    }

    public override string ToString()
    {
        return ToCidrString();
    }

    public override bool Equals(object? obj)
    {
        return obj is Ip4Mask other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _mask.GetHashCode();
    }

    public static bool operator ==(Ip4Mask left, Ip4Mask right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ip4Mask left, Ip4Mask right)
    {
        return !(left == right);
    }

    public bool Equals(Ip4Mask other)
    {
        return _mask.Equals(other._mask);
    }
}