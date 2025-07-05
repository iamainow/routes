using System.Runtime.InteropServices;

namespace routes.core;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Ip4Mask
{
    public static Ip4Mask ParseFullString(string text)
    {
        var step1 = text.Split('.');
        if (step1.Length != 4)
        {
            throw new ArgumentException("Invalid argument");
        }

        var step2 = step1.Select(byte.Parse).ToArray();

        return new Ip4Mask(step2[0], step2[1], step2[2], step2[3]);
    }
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

    public static Ip4Mask ParseCidrString(string text)
    {
        if (!text.StartsWith('/'))
        {
            throw new ArgumentException("Invalid argument");
        }

        int cidr = int.Parse(text[1..]);

        return new Ip4Mask(cidr);
    }
    public static bool TryParseCidrString(string text, out Ip4Mask result)
    {
        if (!text.StartsWith('/'))
        {
            result = default;
            return false;
        }

        if (!int.TryParse(text[1..], out int cidr))
        {
            result = default;
            return false;
        }

        result = new Ip4Mask(cidr);
        return true;
    }

    private static uint GetMaskByCidr(int cidr)
    {
        return 0xFFFFFFFF >> cidr << cidr;
    }
    private static int GetCidrByMask(uint mask)
    {
        return GetCidrByDifferentBits(~mask);
    }
    private static int GetCidrByDifferentBits(uint differentBits)
    {
        // TODO: optimize to binary search or asm command
        if (differentBits == 0x00000000) return 32;

        if (differentBits == 0x00000001) return 31;
        if (differentBits == 0x00000003) return 30;
        if (differentBits == 0x00000007) return 29;
        if (differentBits == 0x0000000F) return 28;

        if (differentBits == 0x0000001F) return 27;
        if (differentBits == 0x0000003F) return 26;
        if (differentBits == 0x0000007F) return 25;
        if (differentBits == 0x000000FF) return 24;

        if (differentBits == 0x000001FF) return 23;
        if (differentBits == 0x000003FF) return 22;
        if (differentBits == 0x000007FF) return 21;
        if (differentBits == 0x00000FFF) return 20;

        if (differentBits == 0x00001FFF) return 19;
        if (differentBits == 0x00003FFF) return 18;
        if (differentBits == 0x00007FFF) return 17;
        if (differentBits == 0x0000FFFF) return 16;

        if (differentBits == 0x0001FFFF) return 15;
        if (differentBits == 0x0003FFFF) return 14;
        if (differentBits == 0x0007FFFF) return 13;
        if (differentBits == 0x000FFFFF) return 12;

        if (differentBits == 0x001FFFFF) return 11;
        if (differentBits == 0x003FFFFF) return 10;
        if (differentBits == 0x007FFFFF) return 9;
        if (differentBits == 0x00FFFFFF) return 8;

        if (differentBits == 0x01FFFFFF) return 7;
        if (differentBits == 0x03FFFFFF) return 6;
        if (differentBits == 0x07FFFFFF) return 5;
        if (differentBits == 0x0FFFFFFF) return 4;

        if (differentBits == 0x1FFFFFFF) return 3;
        if (differentBits == 0x3FFFFFFF) return 2;
        if (differentBits == 0x7FFFFFFF) return 1;
        if (differentBits == 0xFFFFFFFF) return 0;

        throw new ArgumentException($"differentBits invalid value:  {differentBits:x2}", nameof(differentBits));
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

    public Ip4Mask(uint mask)
    {
        _mask = mask;
    }

    public Ip4Mask(int cidr)
    {
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

    public int AsCidr()
    {
        return GetCidrByMask(_mask);
    }

    public string ToFullString()
    {
        return $"{_byte1}.{_byte2}.{_byte3}.{_byte4}";
    }
    public string ToCidrString()
    {
        return $"/{AsCidr()}";
    }
    public override string ToString() => ToCidrString();

    public static Ip4Mask operator ~(Ip4Mask mask)
    {
        return new Ip4Mask(~mask._mask);
    }
}