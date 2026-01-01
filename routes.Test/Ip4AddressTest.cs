using System.Net;

namespace routes.Test;

public class Ip4AddressTest
{
    [Theory]
    [InlineData(0x12345678, 0x12, 0x34, 0x56, 0x78)]
    [InlineData(0x87654321, 0x87, 0x65, 0x43, 0x21)]
    [InlineData(0x00000000, 0x00, 0x00, 0x00, 0x00)]
    [InlineData(0xFFFFFFFF, 0xFF, 0xFF, 0xFF, 0xFF)]
    public void Constructor_WithUInt_CreatesAddressWithCorrectBytes(uint address, byte part1, byte part2, byte part3, byte part4)
    {
        // Arrange & Act
        var ip = new Ip4Address(address);

        // Assert
        byte[] ipAsBytes = ip.ToByteArray();
        Assert.Equal(part1, ipAsBytes[0]);
        Assert.Equal(part2, ipAsBytes[1]);
        Assert.Equal(part3, ipAsBytes[2]);
        Assert.Equal(part4, ipAsBytes[3]);
    }

    [Theory]
    [InlineData(0x12345678, 0x12, 0x34, 0x56, 0x78)]
    [InlineData(0x87654321, 0x87, 0x65, 0x43, 0x21)]
    [InlineData(0x00000000, 0x00, 0x00, 0x00, 0x00)]
    [InlineData(0xFFFFFFFF, 0xFF, 0xFF, 0xFF, 0xFF)]
    public void Constructor_WithBytes_CreatesAddressWithCorrectUInt(uint expectedAddress, byte part1, byte part2, byte part3, byte part4)
    {
        // Arrange & Act
        var ip = new Ip4Address(part1, part2, part3, part4);

        // Assert
        Assert.Equal(expectedAddress, ip.ToUInt32());
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("255.255.255.255")]
    public void ImplicitCast_ToIPAddress_ConvertsCorrectly(string address)
    {
        // Arrange
        var ip4Address = Ip4Address.Parse(address);

        // Act
        IPAddress ipAddress = ip4Address;

        // Assert
        Assert.Equal(address, ipAddress.ToString());
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("255.255.255.255")]
    public void ImplicitCast_FromIPAddress_ConvertsCorrectly(string address)
    {
        // Arrange
        var ipAddress = IPAddress.Parse(address);

        // Act
        Ip4Address ip4Address = ipAddress;

        // Assert
        Assert.Equal(address, ip4Address.ToString());
    }

    [Theory]
    [InlineData("192.168.1.1", 192, 168, 1, 1)]
    [InlineData("0.0.0.0", 0, 0, 0, 0)]
    [InlineData("255.255.255.255", 255, 255, 255, 255)]
    [InlineData("10.20.30.40", 10, 20, 30, 40)]
    public void Parse_ValidString_ReturnsCorrectAddress(string input, byte b1, byte b2, byte b3, byte b4)
    {
        var result = Ip4Address.Parse(input);

        Assert.Equal(new Ip4Address(b1, b2, b3, b4), result);
    }

    [Theory]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    [InlineData("")]
    [InlineData("abc.def.ghi.jkl")]
    public void Parse_InvalidString_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => Ip4Address.Parse(input));
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("192.168.1", false)]
    [InlineData("256.1.1.1", false)]
    [InlineData("", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess)
    {
        bool result = Ip4Address.TryParse(input, out var address);

        Assert.Equal(expectedSuccess, result);
        if (expectedSuccess)
        {
            Assert.Equal(input, address.ToString());
        }
    }

    [Theory]
    [InlineData(0xC0A80101, "192.168.1.1")]
    [InlineData(0x00000000, "0.0.0.0")]
    [InlineData(0xFFFFFFFF, "255.255.255.255")]
    [InlineData(0x0A141E28, "10.20.30.40")]
    public void ToString_ReturnsCorrectFormat(uint address, string expected)
    {
        var ip = new Ip4Address(address);

        Assert.Equal(expected, ip.ToString());
    }

    [Fact]
    public void Constructor_WithValidByteArray_CreatesAddress()
    {
        // Arrange
        byte[] bytes = [192, 168, 1, 1];

        // Act
        var ip = new Ip4Address(bytes);

        // Assert
        Assert.Equal("192.168.1.1", ip.ToString());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(0)]
    public void Constructor_WithInvalidByteArrayLength_ThrowsArgumentException(int length)
    {
        // Arrange
        byte[] bytes = new byte[length];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Ip4Address(bytes));
    }

    [Theory]
    [InlineData(10, 20, -1)]
    [InlineData(10, 10, 0)]
    [InlineData(20, 10, 1)]
    [InlineData(0, 255, -1)]
    public void CompareTo_VariousAddresses_ReturnsExpectedResult(uint addr1, uint addr2, int expectedSign)
    {
        var ip1 = new Ip4Address(addr1);
        var ip2 = new Ip4Address(addr2);

        int result = ip1.CompareTo(ip2);

        Assert.Equal(expectedSign, Math.Sign(result));
    }

    [Fact]
    public void Min_WithTwoAddresses_ReturnsSmallerAddress()
    {
        // Arrange
        var ip1 = new Ip4Address(100);
        var ip2 = new Ip4Address(200);

        // Act
        var result = Ip4Address.Min(ip1, ip2);

        // Assert
        Assert.Equal(ip1, result);
    }

    [Fact]
    public void Max_WithTwoAddresses_ReturnsLargerAddress()
    {
        // Arrange
        var ip1 = new Ip4Address(100);
        var ip2 = new Ip4Address(200);

        // Act
        var result = Ip4Address.Max(ip1, ip2);

        // Assert
        Assert.Equal(ip2, result);
    }

    [Fact]
    public void Equals_SameAddress_ReturnsTrue()
    {
        var ip1 = new Ip4Address(192, 168, 1, 1);
        var ip2 = new Ip4Address(192, 168, 1, 1);

        Assert.True(ip1.Equals(ip2));
        Assert.True(ip1 == ip2);
        Assert.False(ip1 != ip2);
    }

    [Fact]
    public void Equals_DifferentAddress_ReturnsFalse()
    {
        var ip1 = new Ip4Address(192, 168, 1, 1);
        var ip2 = new Ip4Address(192, 168, 1, 2);

        Assert.False(ip1.Equals(ip2));
        Assert.False(ip1 == ip2);
        Assert.True(ip1 != ip2);
    }

    [Fact]
    public void GetHashCode_SameAddress_ReturnsSameHash()
    {
        var ip1 = new Ip4Address(192, 168, 1, 1);
        var ip2 = new Ip4Address(192, 168, 1, 1);

        Assert.Equal(ip1.GetHashCode(), ip2.GetHashCode());
    }

    [Fact]
    public void ComparisonOperators_WithDifferentAddresses_WorkCorrectly()
    {
        // Arrange
        var smaller = new Ip4Address(100);
        var larger = new Ip4Address(200);
        var equal = new Ip4Address(100);

        // Act & Assert
        Assert.True(smaller < larger);
        Assert.True(smaller <= larger);
        Assert.True(larger > smaller);
        Assert.True(larger >= smaller);
        Assert.True(smaller <= equal);
        Assert.True(smaller >= equal);
    }

    [Fact]
    public void ExplicitCast_ToUInt_ReturnsCorrectValue()
    {
        var ip = new Ip4Address(0x12345678);

        uint result = (uint)ip;

        Assert.Equal(0x12345678u, result);
    }

    [Fact]
    public void ImplicitCast_ToIp4Range_CreatesRangeWithSameStartEnd()
    {
        var ip = new Ip4Address(100);

        Ip4Range range = ip;

        Assert.Equal(ip, range.FirstAddress);
        Assert.Equal(ip, range.LastAddress);
    }

    [Fact]
    public void ImplicitCast_ToIp4Subnet_CreatesSubnetWithSingleAddress()
    {
        var ip = new Ip4Address(192, 168, 1, 1);

        Ip4Subnet subnet = ip;

        Assert.Equal(ip, subnet.FirstAddress);
        Assert.Equal(32, subnet.Mask.Cidr);
    }

    [Fact]
    public void ImplicitCast_ToIp4RangeSet_CreatesSetWithSingleAddress()
    {
        var ip = new Ip4Address(100);

        Ip4RangeSet set = ip;

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(ip, ranges[0].FirstAddress);
        Assert.Equal(ip, ranges[0].LastAddress);
    }

    [Fact]
    public void ToIp4Range_CreatesRangeWithSameStartEnd()
    {
        var ip = new Ip4Address(100);

        var range = ip.ToIp4Range();

        Assert.Equal(ip, range.FirstAddress);
        Assert.Equal(ip, range.LastAddress);
    }

    [Fact]
    public void ToIp4Subnet_CreatesSubnetWithSingleAddress()
    {
        var ip = new Ip4Address(192, 168, 1, 1);

        var subnet = ip.ToIp4Subnet();

        Assert.Equal(ip, subnet.FirstAddress);
        Assert.Equal(Ip4Mask.SingleAddress, subnet.Mask);
    }

    [Fact]
    public void ToIp4RangeSet_CreatesSetWithSingleAddress()
    {
        var ip = new Ip4Address(100);

        var set = ip.ToIp4RangeSet();

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(ip, ranges[0].FirstAddress);
    }

    [Fact]
    public void AsByteArray_WithValidAddress_ReturnsCorrectBytes()
    {
        // Arrange
        var ip = new Ip4Address(192, 168, 1, 1);

        // Act
        byte[] bytes = ip.ToByteArray();

        // Assert
        Assert.Equal([192, 168, 1, 1], bytes);
    }

    [Fact]
    public void FromIPAddress_ValidIPAddress_CreatesCorrectIp4Address()
    {
        var ipAddress = IPAddress.Parse("192.168.1.1");

        var ip4 = Ip4Address.FromIPAddress(ipAddress);

        Assert.Equal("192.168.1.1", ip4.ToString());
    }

    [Fact]
    public void FromIPAddress_NullIPAddress_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ip4Address.FromIPAddress(null!));
    }
}