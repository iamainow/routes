namespace routes.Test;

public class Ip4SubnetTest
{
    [Theory]
    [InlineData("193.227.134.0/24", "193.227.134.0", "193.227.134.255")]
    [InlineData("193.228.161.128/25", "193.228.161.128", "193.228.161.255")]
    [InlineData("193.233.80.188/32", "193.233.80.188", "193.233.80.188")]
    [InlineData("217.197.2.16/31", "217.197.2.16", "217.197.2.17")]
    public void CreateByUInt_Check_FirstAddress_LastAddress(string subnetString, string firstIp, string lastIp)
    {
        var subnet = Ip4Subnet.Parse(subnetString);
        string actualResult = $"{subnet.FirstAddress}-{subnet.LastAddress}";

        Assert.Equal($"{firstIp}-{lastIp}", actualResult);
    }

    [Fact]
    public void Constructor_AddressAndMask_CreatesValidSubnet()
    {
        var address = new Ip4Address(192, 168, 1, 0);
        var mask = new Ip4Mask(24);

        var subnet = new Ip4Subnet(address, mask);

        Assert.Equal(address, subnet.FirstAddress);
        Assert.Equal(mask, subnet.Mask);
    }

    [Fact]
    public void Constructor_AddressAndCidr_CreatesValidSubnet()
    {
        var address = new Ip4Address(192, 168, 1, 0);

        var subnet = new Ip4Subnet(address, 24);

        Assert.Equal(address, subnet.FirstAddress);
        Assert.Equal(24, subnet.Mask.Cidr);
    }

    [Fact]
    public void Constructor_AddressAndUIntMask_CreatesValidSubnet()
    {
        var address = new Ip4Address(192, 168, 1, 0);

        var subnet = new Ip4Subnet(address, 0xFFFFFF00u);

        Assert.Equal(address, subnet.FirstAddress);
        Assert.Equal(24, subnet.Mask.Cidr);
    }

    [Fact]
    public void Constructor_InvalidFirstAddress_ThrowsArgumentException()
    {
        var address = new Ip4Address(192, 168, 1, 1); // Not network address
        var mask = new Ip4Mask(24);

        Assert.Throws<ArgumentException>(() => new Ip4Subnet(address, mask));
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0", 24)]
    [InlineData("10.0.0.0/8", "10.0.0.0", 8)]
    [InlineData("172.16.0.0/16", "172.16.0.0", 16)]
    [InlineData("192.168.1.128/25", "192.168.1.128", 25)]
    public void Parse_ValidCidrFormat_ReturnsCorrectSubnet(string input, string expectedAddress, int expectedCidr)
    {
        var subnet = Ip4Subnet.Parse(input);

        Assert.Equal(expectedAddress, subnet.FirstAddress.ToString());
        Assert.Equal(expectedCidr, subnet.Mask.Cidr);
    }

    [Theory]
    [InlineData("192.168.1.0 255.255.255.0", "192.168.1.0", 24)]
    [InlineData("10.0.0.0 255.0.0.0", "10.0.0.0", 8)]
    public void Parse_ValidFullMaskFormat_ReturnsCorrectSubnet(string input, string expectedAddress, int expectedCidr)
    {
        var subnet = Ip4Subnet.Parse(input);

        Assert.Equal(expectedAddress, subnet.FirstAddress.ToString());
        Assert.Equal(expectedCidr, subnet.Mask.Cidr);
    }

    [Theory]
    [InlineData("192.168.1.0")]
    [InlineData("192.168.1.0/")]
    [InlineData("192.168.1.0/33")]
    [InlineData("invalid")]
    public void Parse_InvalidFormat_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => Ip4Subnet.Parse(input));
    }

    [Theory]
    [InlineData("192.168.1.0/24", true)]
    [InlineData("10.0.0.0 255.0.0.0", true)]
    [InlineData("invalid", false)]
    [InlineData("192.168.1.0/33", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess)
    {
        bool result = Ip4Subnet.TryParse(input, out var subnet);

        Assert.Equal(expectedSuccess, result);
    }

    [Fact]
    public void TryParse_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ip4Subnet.TryParse(null!, out _));
    }

    [Theory]
    [InlineData("192.168.1.0", 24, true)]
    [InlineData("192.168.1.1", 24, false)]
    [InlineData("10.0.0.0", 8, true)]
    [InlineData("10.1.0.0", 8, false)]
    [InlineData("192.168.1.128", 25, true)]
    [InlineData("192.168.1.129", 25, false)]
    public void IsValid_VariousAddressesAndMasks_ReturnsExpectedResult(string address, int cidr, bool expected)
    {
        var addr = Ip4Address.Parse(address);
        var mask = new Ip4Mask(cidr);

        Assert.Equal(expected, Ip4Subnet.IsValid(mask, addr));
    }

    [Fact]
    public void Validate_ValidSubnet_DoesNotThrow()
    {
        var address = new Ip4Address(192, 168, 1, 0);
        var mask = new Ip4Mask(24);

        var exception = Record.Exception(() => Ip4Subnet.Validate(mask, address));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_InvalidSubnet_ThrowsArgumentException()
    {
        var address = new Ip4Address(192, 168, 1, 1);
        var mask = new Ip4Mask(24);

        Assert.Throws<ArgumentException>(() => Ip4Subnet.Validate(mask, address));
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.255")]
    [InlineData("10.0.0.0/8", "10.255.255.255")]
    [InlineData("172.16.0.0/16", "172.16.255.255")]
    [InlineData("192.168.1.128/25", "192.168.1.255")]
    [InlineData("192.168.1.0/32", "192.168.1.0")]
    public void LastAddress_ReturnsCorrectValue(string subnetString, string expectedLastAddress)
    {
        var subnet = Ip4Subnet.Parse(subnetString);

        Assert.Equal(expectedLastAddress, subnet.LastAddress.ToString());
    }

    [Theory]
    [InlineData(24, 256UL)]
    [InlineData(16, 65536UL)]
    [InlineData(8, 16777216UL)]
    [InlineData(32, 1UL)]
    [InlineData(0, 4294967296UL)]
    public void Count_ReturnsCorrectNumberOfAddresses(int cidr, ulong expectedCount)
    {
        var subnet = new Ip4Subnet(new Ip4Address(0), cidr);

        Assert.Equal(expectedCount, subnet.Count);
    }

    [Fact]
    public void ToIp4Range_CreatesCorrectRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        var range = subnet.ToIp4Range();

        Assert.Equal(subnet.FirstAddress, range.FirstAddress);
        Assert.Equal(subnet.LastAddress, range.LastAddress);
    }

    [Fact]
    public void ToIp4RangeSet_CreatesSetWithSingleRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        var set = subnet.ToIp4RangeSet();

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(subnet.FirstAddress, ranges[0].FirstAddress);
        Assert.Equal(subnet.LastAddress, ranges[0].LastAddress);
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0 255.255.255.0")]
    [InlineData("10.0.0.0/8", "10.0.0.0 255.0.0.0")]
    [InlineData("172.16.0.0/16", "172.16.0.0 255.255.0.0")]
    public void ToFullString_ReturnsCorrectFormat(string input, string expected)
    {
        var subnet = Ip4Subnet.Parse(input);

        Assert.Equal(expected, subnet.ToFullString());
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0/24")]
    [InlineData("10.0.0.0/8", "10.0.0.0/8")]
    [InlineData("172.16.0.0/16", "172.16.0.0/16")]
    public void ToCidrString_ReturnsCorrectFormat(string input, string expected)
    {
        var subnet = Ip4Subnet.Parse(input);

        Assert.Equal(expected, subnet.ToCidrString());
    }

    [Theory]
    [InlineData("192.168.1.0/24")]
    [InlineData("10.0.0.0/8")]
    public void ToString_ReturnsCidrFormat(string input)
    {
        var subnet = Ip4Subnet.Parse(input);

        Assert.Equal(input, subnet.ToString());
    }

    [Fact]
    public void Equals_SameSubnet_ReturnsTrue()
    {
        var subnet1 = Ip4Subnet.Parse("192.168.1.0/24");
        var subnet2 = Ip4Subnet.Parse("192.168.1.0/24");

        Assert.True(subnet1.Equals(subnet2));
        Assert.True(subnet1 == subnet2);
        Assert.False(subnet1 != subnet2);
    }

    [Fact]
    public void Equals_DifferentSubnet_ReturnsFalse()
    {
        var subnet1 = Ip4Subnet.Parse("192.168.1.0/24");
        var subnet2 = Ip4Subnet.Parse("192.168.2.0/24");

        Assert.False(subnet1.Equals(subnet2));
        Assert.False(subnet1 == subnet2);
        Assert.True(subnet1 != subnet2);
    }

    [Fact]
    public void Equals_DifferentMask_ReturnsFalse()
    {
        var subnet1 = Ip4Subnet.Parse("192.168.1.0/24");
        var subnet2 = Ip4Subnet.Parse("192.168.1.0/25");

        Assert.False(subnet1.Equals(subnet2));
    }

    [Fact]
    public void GetHashCode_SameSubnet_ReturnsSameHash()
    {
        var subnet1 = Ip4Subnet.Parse("192.168.1.0/24");
        var subnet2 = Ip4Subnet.Parse("192.168.1.0/24");

        Assert.Equal(subnet1.GetHashCode(), subnet2.GetHashCode());
    }

    [Fact]
    public void ImplicitCast_ToIp4Range_CreatesCorrectRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        Ip4Range range = subnet;

        Assert.Equal(subnet.FirstAddress, range.FirstAddress);
        Assert.Equal(subnet.LastAddress, range.LastAddress);
    }

    [Fact]
    public void ImplicitCast_ToIp4RangeSet_CreatesSetWithSingleRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        Ip4RangeSet set = subnet;

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
    }

    [Fact]
    public void StaticField_All_CoversEntireAddressSpace()
    {
        Assert.Equal(new Ip4Address(0), Ip4Subnet.All.FirstAddress);
        Assert.Equal(0, Ip4Subnet.All.Mask.Cidr);
        Assert.Equal(4294967296UL, Ip4Subnet.All.Count);
    }

    [Theory]
    [InlineData("192.168.0.0/24", "192.168.1.0/24", false)]
    [InlineData("192.168.1.0/24", "192.168.1.0/24", true)]
    [InlineData("192.168.1.0/24", "192.168.1.0/25", false)]
    public void Equality_VariousSubnets_WorksCorrectly(string subnet1Str, string subnet2Str, bool shouldBeEqual)
    {
        var subnet1 = Ip4Subnet.Parse(subnet1Str);
        var subnet2 = Ip4Subnet.Parse(subnet2Str);

        Assert.Equal(shouldBeEqual, subnet1 == subnet2);
        Assert.Equal(!shouldBeEqual, subnet1 != subnet2);
    }

    [Fact]
    public void Constructor_WithAllValidCombinations_CreatesValidSubnets()
    {
        var address = new Ip4Address(192, 168, 0, 0);

        // Test with Ip4Mask
        var subnet1 = new Ip4Subnet(address, new Ip4Mask(24));
        Assert.Equal(24, subnet1.Mask.Cidr);

        // Test with int cidr
        var subnet2 = new Ip4Subnet(address, 24);
        Assert.Equal(24, subnet2.Mask.Cidr);

        // Test with uint mask
        var subnet3 = new Ip4Subnet(address, 0xFFFFFF00u);
        Assert.Equal(24, subnet3.Mask.Cidr);

        // All should be equal
        Assert.Equal(subnet1, subnet2);
        Assert.Equal(subnet2, subnet3);
    }
}