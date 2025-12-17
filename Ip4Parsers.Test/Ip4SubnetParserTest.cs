using routes;

namespace Ip4Parsers.Test;


public class Ip4SubnetParserTest
{
    [Fact]
    public void GetRanges_SingleIp_ReturnsSingleRange()
    {
        var result = Ip4SubnetParser.GetRanges("192.168.1.1").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].LastAddress);
    }

    [Fact]
    public void GetRanges_CidrSubnet_ReturnsRange()
    {
        var result = Ip4SubnetParser.GetRanges("192.168.1.0/24").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(192, 168, 1, 0), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 1, 255), result[0].LastAddress);
    }

    [Fact]
    public void GetRanges_IpRange_ReturnsRange()
    {
        var result = Ip4SubnetParser.GetRanges("192.168.1.1-192.168.1.10").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 1, 10), result[0].LastAddress);
    }

    [Fact]
    public void GetRanges_MultipleMixed_ReturnsMultipleRanges()
    {
        var result = Ip4SubnetParser.GetRanges("192.168.1.1, 192.168.2.0/24, 192.168.3.5-192.168.3.10").ToArray();
        Assert.Equal(3, result.Length);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].LastAddress);
        Assert.Equal(new Ip4Address(192, 168, 2, 0), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 2, 255), result[1].LastAddress);
        Assert.Equal(new Ip4Address(192, 168, 3, 5), result[2].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 3, 10), result[2].LastAddress);
    }

    [Fact]
    public void GetRanges_InvalidInputs_SkipsInvalid()
    {
        var result = Ip4SubnetParser.GetRanges("invalid 192.168.1.1 garbage 999.999.999.999").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(192, 168, 1, 1), result[0].LastAddress);
    }

    [Fact]
    public void GetSubnets_SingleIp_ReturnsSubnet32()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.1").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 1), 32), result[0]);
    }

    [Fact]
    public void GetSubnets_CidrSubnet_ReturnsSubnet()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.0/24").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 0), 24), result[0]);
    }

    [Fact]
    public void GetSubnets_IpRange_ReturnsMultipleSubnets()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.1-192.168.1.3").ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 1), 32), result[0]);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 2), 31), result[1]);
    }

    [Fact]
    public void GetSubnets_MultipleMixed_ReturnsSubnets()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.1, 192.168.2.0/24").ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 1), 32), result[0]);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 2, 0), 24), result[1]);
    }

    [Fact]
    public void GetSubnets_InvalidInputs_SkipsInvalid()
    {
        var result = Ip4SubnetParser.GetSubnets("invalid 192.168.1.1 garbage").ToArray();
        Assert.Single(result);
        Assert.Equal(new Ip4Subnet(new Ip4Address(192, 168, 1, 1), 32), result[0]);
    }
}
