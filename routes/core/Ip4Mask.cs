using System.Diagnostics;
using System.Runtime.InteropServices;

namespace routes.core;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Mask
{
    public static readonly Ip4Mask Full = new Ip4Mask(0xFFFFFFFF);
    public static readonly Ip4Mask SingleAddress = new Ip4Mask(0xFFFFFFFF);

    /// <param name="text">x.x.x.x format</param>
    /// <exception cref="FormatException"></exception>
    public static Ip4Mask ParseFullString(string text)
    {
        if (!TryParseFullString(text, out var result))
        {
            throw new FormatException();
        }

        return result;
    }

    /// <param name="text">x.x.x.x format</param>
    public static bool TryParseFullString(string text, out Ip4Mask result)
    {
        var step1 = text.Split('.');
        if (step1.Length != 4)
        {
            result = default;
            return false;
        }

        List<byte> step2 = new List<byte>(4);
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
        if (!TryParseCidrString(text, out var mask))
        {
            throw new FormatException();
        }

        return mask;
    }

    /// <param name="text">/xx or xx format</param>
    public static bool TryParseCidrString(string text, out Ip4Mask result)
    {
        if (text.StartsWith('/'))
        {
            text = text[1..];
        }

        if (!int.TryParse(text, out int cidr))
        {
            result = default;
            return false;
        }

        if (cidr < 0 || cidr > 32)
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
        if (!TryParse(text, out var result))
        {
            throw new FormatException();
        }

        return result;
    }

    /// <param name="text">/xx, xx or x.x.x.x</param>
    public static bool TryParse(string text, out Ip4Mask result)
    {
        if (TryParseFullString(text, out var result1))
        {
            result = result1;
            return true;
        }

        if (TryParseCidrString(text, out var result2))
        {
            result = result2;
            return true;
        }

        result = default;
        return false;
    }

    private static uint GetMaskByCidr(int cidr)
    {
        if (cidr == 0)
        {
            return 0;
        }

        return (0xFFFFFFFF >> (32 - cidr)) << (32 - cidr);
    }
    private static int GetCidrByMask(uint mask)
    {
        return GetCidrByDifferentBits(~mask);
    }
    private static int GetCidrByDifferentBits(uint differentBits)
    {
        return differentBits switch
        {
            0x00000000 => 32,
            0x00000001 => 31,
            0x00000003 => 30,
            0x00000007 => 29,
            0x0000000F => 28,
            0x0000001F => 27,
            0x0000003F => 26,
            0x0000007F => 25,
            0x000000FF => 24,
            0x000001FF => 23,
            0x000003FF => 22,
            0x000007FF => 21,
            0x00000FFF => 20,
            0x00001FFF => 19,
            0x00003FFF => 18,
            0x00007FFF => 17,
            0x0000FFFF => 16,
            0x0001FFFF => 15,
            0x0003FFFF => 14,
            0x0007FFFF => 13,
            0x000FFFFF => 12,
            0x001FFFFF => 11,
            0x003FFFFF => 10,
            0x007FFFFF => 9,
            0x00FFFFFF => 8,
            0x01FFFFFF => 7,
            0x03FFFFFF => 6,
            0x07FFFFFF => 5,
            0x0FFFFFFF => 4,
            0x1FFFFFFF => 3,
            0x3FFFFFFF => 2,
            0x7FFFFFFF => 1,
            0xFFFFFFFF => 0,
            _ => throw new ArgumentException($"differentBits invalid value: {differentBits:x2}", nameof(differentBits))
        };
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

    public Ip4Mask(uint mask)
    {
        _mask = mask;
    }

    /// <exception cref="ArgumentException"></exception>
    public Ip4Mask(int cidr)
    {
        if (cidr < 0 || cidr > 32)
        {
            throw new ArgumentException();
        }

        _mask = GetMaskByCidr(cidr);
    }

    public Ip4Mask(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        _byte1 = byte1;
        _byte2 = byte2;
        _byte3 = byte3;
        _byte4 = byte4;
    }

    public uint AsUInt32()
    {
        return _mask;
    }

    public byte[] AsByteArray()
    {
        return [_byte1, _byte2, _byte3, _byte4];
    }

    public string ToFullString()
    {
        return $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";
    }
    public string ToCidrString()
    {
        return $"/{Cidr}";
    }

    public override string ToString() => ToCidrString();
}