using System.Net;

namespace routes.Test;

public class Ip4MaskTest
{
    [Theory]
    [InlineData(32, "255.255.255.255")]
    [InlineData(31, "255.255.255.254")]
    [InlineData(30, "255.255.255.252")]
    [InlineData(29, "255.255.255.248")]
    [InlineData(28, "255.255.255.240")]
    [InlineData(27, "255.255.255.224")]
    [InlineData(26, "255.255.255.192")]
    [InlineData(25, "255.255.255.128")]
    [InlineData(24, "255.255.255.0")]
    [InlineData(23, "255.255.254.0")]
    [InlineData(22, "255.255.252.0")]
    [InlineData(21, "255.255.248.0")]
    [InlineData(20, "255.255.240.0")]
    [InlineData(19, "255.255.224.0")]
    [InlineData(18, "255.255.192.0")]
    [InlineData(17, "255.255.128.0")]
    [InlineData(16, "255.255.0.0")]
    [InlineData(15, "255.254.0.0")]
    [InlineData(14, "255.252.0.0")]
    [InlineData(13, "255.248.0.0")]
    [InlineData(12, "255.240.0.0")]
    [InlineData(11, "255.224.0.0")]
    [InlineData(10, "255.192.0.0")]
    [InlineData(9, "255.128.0.0")]
    [InlineData(8, "255.0.0.0")]
    [InlineData(7, "254.0.0.0")]
    [InlineData(6, "252.0.0.0")]
    [InlineData(5, "248.0.0.0")]
    [InlineData(4, "240.0.0.0")]
    [InlineData(3, "224.0.0.0")]
    [InlineData(2, "192.0.0.0")]
    [InlineData(1, "128.0.0.0")]
    [InlineData(0, "0.0.0.0")]

    public void Constructor_FromCidr_ReturnsCorrectFullString(int cidr, string subnet)
    {
        Ip4Mask mask = new(cidr);

        Assert.Equal(subnet, mask.ToFullString());
    }

    [Theory]
    [InlineData(32)]
    [InlineData(31)]
    [InlineData(30)]
    [InlineData(29)]
    [InlineData(28)]
    [InlineData(27)]
    [InlineData(26)]
    [InlineData(25)]
    [InlineData(24)]
    [InlineData(23)]
    [InlineData(22)]
    [InlineData(21)]
    [InlineData(20)]
    [InlineData(19)]
    [InlineData(18)]
    [InlineData(17)]
    [InlineData(16)]
    [InlineData(15)]
    [InlineData(14)]
    [InlineData(13)]
    [InlineData(12)]
    [InlineData(11)]
    [InlineData(10)]
    [InlineData(9)]
    [InlineData(8)]
    [InlineData(7)]
    [InlineData(6)]
    [InlineData(5)]
    [InlineData(4)]
    [InlineData(3)]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(0)]
    public void Constructor_RoundTrip_CidrToUInt32ToCidr_PreservesCidr(int cidr)
    {
        Assert.Equal(new Ip4Mask(new Ip4Mask(cidr).ToUInt32()).Cidr, cidr);
    }

    [Theory]
    [InlineData("255.192.0.0")]
    public void ImplicitCast_ToIPAddress_ReturnsCorrectAddress(string address)
    {
        var ip4mask = Ip4Mask.Parse(address);
        var ipAddress = (IPAddress)ip4mask;

        string actualValue = ipAddress.ToString();

        Assert.Equal(address, actualValue);
    }

    [Theory]
    [InlineData("255.192.0.0")]
    public void ImplicitCast_FromIPAddress_ReturnsCorrectMask(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        var ip4mask = (Ip4Mask)ipAddress;

        string actualValue = ip4mask.ToFullString();

        Assert.Equal(address, actualValue);
    }

    [Theory]
    [InlineData(0, 0x00000000u)]
    [InlineData(8, 0xFF000000u)]
    [InlineData(16, 0xFFFF0000u)]
    [InlineData(24, 0xFFFFFF00u)]
    [InlineData(32, 0xFFFFFFFFu)]
    public void Constructor_FromCidr_CreatesCorrectMask(int cidr, uint expectedMask)
    {
        var mask = new Ip4Mask(cidr);

        Assert.Equal(expectedMask, mask.ToUInt32());
        Assert.Equal(cidr, mask.Cidr);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    [InlineData(100)]
    public void Constructor_FromCidr_InvalidCidr_ThrowsArgumentOutOfRangeException(int invalidCidr)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ip4Mask(invalidCidr));
    }

    [Theory]
    [InlineData(0xFFFFFF00u, 24)]
    [InlineData(0xFFFFFFFFu, 32)]
    [InlineData(0x00000000u, 0)]
    [InlineData(0xFFFFFFF0u, 28)]
    public void Constructor_FromUInt_CreatesCorrectMask(uint maskValue, int expectedCidr)
    {
        var mask = new Ip4Mask(maskValue);

        Assert.Equal(expectedCidr, mask.Cidr);
        Assert.Equal(maskValue, mask.ToUInt32());
    }

    [Theory]
    [InlineData(0x12345678u)]
    [InlineData(0xFFFFFF01u)]
    [InlineData(0x80000001u)]
    public void Constructor_FromUInt_InvalidMask_ThrowsArgumentException(uint invalidMask)
    {
        Assert.Throws<ArgumentException>(() => new Ip4Mask(invalidMask));
    }

    [Theory]
    [InlineData(255, 255, 255, 0, 24)]
    [InlineData(255, 255, 0, 0, 16)]
    [InlineData(255, 0, 0, 0, 8)]
    [InlineData(0, 0, 0, 0, 0)]
    public void Constructor_FromBytes_CreatesCorrectMask(byte b1, byte b2, byte b3, byte b4, int expectedCidr)
    {
        var mask = new Ip4Mask(b1, b2, b3, b4);

        Assert.Equal(expectedCidr, mask.Cidr);
    }

    [Fact]
    public void Constructor_ByteArray_ValidArray_CreatesMask()
    {
        byte[] bytes = [255, 255, 255, 0];
        var mask = new Ip4Mask(bytes);

        Assert.Equal(24, mask.Cidr);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(0)]
    public void Constructor_ByteArray_InvalidLength_ThrowsArgumentException(int length)
    {
        byte[] bytes = new byte[length];
        Assert.Throws<ArgumentException>(() => new Ip4Mask(bytes));
    }

    [Theory]
    [InlineData("/24", 24)]
    [InlineData("24", 24)]
    [InlineData("/0", 0)]
    [InlineData("32", 32)]
    [InlineData("/16", 16)]
    public void ParseCidrString_ValidInput_ReturnsCorrectMask(string input, int expectedCidr)
    {
        var mask = Ip4Mask.ParseCidrString(input);

        Assert.Equal(expectedCidr, mask.Cidr);
    }

    [Theory]
    [InlineData("/24", true, 24)]
    [InlineData("24", true, 24)]
    [InlineData("/33", false, 0)]
    [InlineData("abc", false, 0)]
    [InlineData("/-1", false, 0)]
    public void TryParseCidrString_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess, int expectedCidr)
    {
        bool result = Ip4Mask.TryParseCidrString(input, out var mask);

        Assert.Equal(expectedSuccess, result);
        if (expectedSuccess)
        {
            Assert.Equal(expectedCidr, mask.Cidr);
        }
    }

    [Theory]
    [InlineData("255.255.255.0", 24)]
    [InlineData("255.255.0.0", 16)]
    [InlineData("255.0.0.0", 8)]
    [InlineData("0.0.0.0", 0)]
    public void ParseFullString_ValidInput_ReturnsCorrectMask(string input, int expectedCidr)
    {
        var mask = Ip4Mask.ParseFullString(input);

        Assert.Equal(expectedCidr, mask.Cidr);
        Assert.Equal(input, mask.ToFullString());
    }

    [Theory]
    [InlineData("255.255.255")]
    [InlineData("256.0.0.0")]
    [InlineData("abc.def.ghi.jkl")]
    public void ParseFullString_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => Ip4Mask.ParseFullString(input));
    }

    [Theory]
    [InlineData("255.255.255.0", true, 24)]
    [InlineData("255.255.0.0", true, 16)]
    [InlineData("255.255.255", false, 0)]
    [InlineData("256.0.0.0", false, 0)]
    public void TryParseFullString_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess, int expectedCidr)
    {
        bool result = Ip4Mask.TryParseFullString(input, out var mask);

        Assert.Equal(expectedSuccess, result);
        if (expectedSuccess)
        {
            Assert.Equal(expectedCidr, mask.Cidr);
        }
    }

    [Theory]
    [InlineData("/24", 24)]
    [InlineData("24", 24)]
    [InlineData("255.255.255.0", 24)]
    [InlineData("/16", 16)]
    [InlineData("255.255.0.0", 16)]
    public void Parse_VariousFormats_ReturnsCorrectMask(string input, int expectedCidr)
    {
        var mask = Ip4Mask.Parse(input);

        Assert.Equal(expectedCidr, mask.Cidr);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("/33")]
    [InlineData("256.0.0.0")]
    public void Parse_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => Ip4Mask.Parse(input));
    }

    [Theory]
    [InlineData("/24", true, 24)]
    [InlineData("255.255.255.0", true, 24)]
    [InlineData("invalid", false, 0)]
    [InlineData("/33", false, 0)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess, int expectedCidr)
    {
        bool result = Ip4Mask.TryParse(input, out var mask);

        Assert.Equal(expectedSuccess, result);
        if (expectedSuccess)
        {
            Assert.Equal(expectedCidr, mask.Cidr);
        }
    }

    [Theory]
    [InlineData(0, 4294967296UL)]
    [InlineData(8, 16777216UL)]
    [InlineData(16, 65536UL)]
    [InlineData(24, 256UL)]
    [InlineData(32, 1UL)]
    public void Count_ReturnsCorrectNumberOfAddresses(int cidr, ulong expectedCount)
    {
        var mask = new Ip4Mask(cidr);

        Assert.Equal(expectedCount, mask.Count);
    }

    [Theory]
    [InlineData(24, "/24")]
    [InlineData(16, "/16")]
    [InlineData(0, "/0")]
    [InlineData(32, "/32")]
    public void ToCidrString_ReturnsCorrectFormat(int cidr, string expected)
    {
        var mask = new Ip4Mask(cidr);

        Assert.Equal(expected, mask.ToCidrString());
    }

    [Theory]
    [InlineData(24, "/24")]
    [InlineData(16, "/16")]
    [InlineData(32, "/32")]
    public void ToString_ReturnsCidrFormat(int cidr, string expected)
    {
        var mask = new Ip4Mask(cidr);

        Assert.Equal(expected, mask.ToString());
    }

    [Fact]
    public void Equals_SameMask_ReturnsTrue()
    {
        var mask1 = new Ip4Mask(24);
        var mask2 = new Ip4Mask(24);

        Assert.True(mask1.Equals(mask2));
        Assert.True(mask1 == mask2);
        Assert.False(mask1 != mask2);
    }

    [Fact]
    public void Equals_DifferentMask_ReturnsFalse()
    {
        var mask1 = new Ip4Mask(24);
        var mask2 = new Ip4Mask(16);

        Assert.False(mask1.Equals(mask2));
        Assert.False(mask1 == mask2);
        Assert.True(mask1 != mask2);
    }

    [Fact]
    public void GetHashCode_SameMask_ReturnsSameHash()
    {
        var mask1 = new Ip4Mask(24);
        var mask2 = new Ip4Mask(24);

        Assert.Equal(mask1.GetHashCode(), mask2.GetHashCode());
    }

    [Fact]
    public void StaticFields_HaveCorrectValues()
    {
        Assert.Equal(0, Ip4Mask.All.Cidr);
        Assert.Equal(32, Ip4Mask.SingleAddress.Cidr);
    }

    [Fact]
    public void AsByteArray_ReturnsCorrectBytes()
    {
        var mask = new Ip4Mask(24);

        byte[] bytes = mask.ToByteArray();

        Assert.Equal([255, 255, 255, 0], bytes);
    }

    [Fact]
    public void FromIPAddress_ValidIPAddress_CreatesCorrectMask()
    {
        var ipAddress = IPAddress.Parse("255.255.255.0");

        var mask = Ip4Mask.FromIPAddress(ipAddress);

        Assert.Equal(24, mask.Cidr);
    }

    [Fact]
    public void FromIPAddress_NullIPAddress_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ip4Mask.FromIPAddress(null!));
    }

    [Fact]
    public void ImplicitCast_ToIPAddress_WorksCorrectly()
    {
        var mask = new Ip4Mask(24);

        IPAddress ipAddress = mask;

        Assert.Equal("255.255.255.0", ipAddress.ToString());
    }

    [Fact]
    public void ImplicitCast_FromIPAddress_WorksCorrectly()
    {
        var ipAddress = IPAddress.Parse("255.255.255.0");

        Ip4Mask mask = ipAddress;

        Assert.Equal(24, mask.Cidr);
    }
}