using System.Net;

namespace routes.Test;

public class Ip4AddressTest
{
    [Theory]
    [InlineData(0x12345678, 0x12, 0x34, 0x56, 0x78)]
    [InlineData(0x87654321, 0x87, 0x65, 0x43, 0x21)]
    [InlineData(0x00000000, 0x00, 0x00, 0x00, 0x00)]
    [InlineData(0xFFFFFFFF, 0xFF, 0xFF, 0xFF, 0xFF)]
    public void CreateByUInt(uint address, byte part1, byte part2, byte part3, byte part4)
    {
        var ip = new Ip4Address(address);

        var ipAsBytes = ip.AsByteArray();

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
    public void CreateByBytes(uint address, byte part1, byte part2, byte part3, byte part4)
    {
        var ip = new Ip4Address(part1, part2, part3, part4);

        Assert.Equal(address, ip.AsUInt32());
    }

    [Theory]
    [InlineData("192.168.1.1")]
    public void ToIPAddress(string address)
    {
        var ip4Address = Ip4Address.Parse(address);
        var ipAddress = (IPAddress)ip4Address;

        string actualValue = ipAddress.ToString();

        Assert.Equal(address, actualValue);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    public void FromIPAddress(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        var ip4Address = (Ip4Address)ipAddress;

        string actualValue = ip4Address.ToString();

        Assert.Equal(address, actualValue);
    }
}