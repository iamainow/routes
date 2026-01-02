using routes;

namespace Ip4Parsers.Test;

public class Ip4SubnetParserTest
{
    #region GetRanges - Single IP

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1", "192.168.1.1")]
    [InlineData("0.0.0.0", "0.0.0.0", "0.0.0.0")]
    [InlineData("255.255.255.255", "255.255.255.255", "255.255.255.255")]
    [InlineData("10.0.0.1", "10.0.0.1", "10.0.0.1")]
    public void GetRanges_SingleIp_ReturnsExpectedRange(string input, string expectedFirst, string expectedLast)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse(expectedFirst), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse(expectedLast), result[0].LastAddress);
    }

    #endregion

    #region GetRanges - CIDR Subnet

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0", "192.168.1.255")]
    [InlineData("10.0.0.0/8", "10.0.0.0", "10.255.255.255")]
    [InlineData("172.16.0.0/16", "172.16.0.0", "172.16.255.255")]
    [InlineData("192.168.1.128/25", "192.168.1.128", "192.168.1.255")]
    [InlineData("192.168.1.1/32", "192.168.1.1", "192.168.1.1")]
    [InlineData("0.0.0.0/0", "0.0.0.0", "255.255.255.255")]
    public void GetRanges_CidrSubnet_ReturnsExpectedRange(string input, string expectedFirst, string expectedLast)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse(expectedFirst), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse(expectedLast), result[0].LastAddress);
    }

    #endregion

    #region GetRanges - IP Range

    [Theory]
    [InlineData("192.168.1.1-192.168.1.10", "192.168.1.1", "192.168.1.10")]
    [InlineData("10.0.0.0-10.0.0.255", "10.0.0.0", "10.0.0.255")]
    [InlineData("0.0.0.0-255.255.255.255", "0.0.0.0", "255.255.255.255")]
    [InlineData("192.168.1.5-192.168.1.5", "192.168.1.5", "192.168.1.5")]
    public void GetRanges_IpRange_ReturnsExpectedRange(string input, string expectedFirst, string expectedLast)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse(expectedFirst), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse(expectedLast), result[0].LastAddress);
    }

    [Theory]
    [InlineData("192.168.1.1 - 192.168.1.10", "192.168.1.1", "192.168.1.10")]
    [InlineData("10.0.0.0  -  10.0.0.255", "10.0.0.0", "10.0.0.255")]
    public void GetRanges_IpRangeWithSpaces_ReturnsExpectedRange(string input, string expectedFirst, string expectedLast)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse(expectedFirst), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse(expectedLast), result[0].LastAddress);
    }

    #endregion

    #region GetRanges - Multiple Items

    [Theory]
    [InlineData("192.168.1.1\n192.168.2.2", 2)]
    [InlineData("192.168.1.1\r\n192.168.2.2", 2)]
    [InlineData("192.168.1.1\n192.168.2.2\n192.168.3.3", 3)]
    public void GetRanges_NewlineSeparatedIps_ReturnsAllRanges(string input, int expectedCount)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Equal(expectedCount, result.Length);
    }

    [Fact]
    public void GetRanges_MixedFormatsOnSameLine_ReturnsAllRanges()
    {
        var result = Ip4SubnetParser.GetRanges("192.168.1.1, 192.168.2.0/24, 192.168.3.5-192.168.3.10").ToArray();

        Assert.Equal(3, result.Length);
        Assert.Equal(Ip4Address.Parse("192.168.1.1"), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse("192.168.1.1"), result[0].LastAddress);
        Assert.Equal(Ip4Address.Parse("192.168.2.0"), result[1].FirstAddress);
        Assert.Equal(Ip4Address.Parse("192.168.2.255"), result[1].LastAddress);
        Assert.Equal(Ip4Address.Parse("192.168.3.5"), result[2].FirstAddress);
        Assert.Equal(Ip4Address.Parse("192.168.3.10"), result[2].LastAddress);
    }

    [Fact]
    public void GetRanges_MixedFormatsOnMultipleLines_ReturnsAllRanges()
    {
        var input = """
            192.168.1.1
            10.0.0.0/8
            172.16.0.1-172.16.0.100
            """;

        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Equal(3, result.Length);
        Assert.Equal(Ip4Address.Parse("192.168.1.1"), result[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse("10.0.0.0"), result[1].FirstAddress);
        Assert.Equal(Ip4Address.Parse("10.255.255.255"), result[1].LastAddress);
        Assert.Equal(Ip4Address.Parse("172.16.0.1"), result[2].FirstAddress);
        Assert.Equal(Ip4Address.Parse("172.16.0.100"), result[2].LastAddress);
    }

    #endregion

    #region GetRanges - Empty and Invalid Input

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    [InlineData("invalid")]
    [InlineData("not an ip address")]
    public void GetRanges_EmptyOrInvalidInput_ReturnsEmpty(string input)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("999.999.999.999")]
    [InlineData("256.1.1.1")]
    public void GetRanges_InvalidIpOctets_ReturnsEmpty(string input)
    {
        var result = Ip4SubnetParser.GetRanges(input).ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void GetRanges_IpWithExtraOctet_ExtractsValidPart()
    {
        // The regex matches "1.2.3.4" from "1.2.3.4.5" - this is expected behavior
        var result = Ip4SubnetParser.GetRanges("1.2.3.4.5").ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse("1.2.3.4"), result[0].FirstAddress);
    }

    [Fact]
    public void GetRanges_MixedValidAndInvalid_ReturnsOnlyValid()
    {
        var result = Ip4SubnetParser.GetRanges("invalid 192.168.1.1 garbage 999.999.999.999").ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse("192.168.1.1"), result[0].FirstAddress);
    }

    [Fact]
    public void GetRanges_IpEmbeddedInText_ExtractsIp()
    {
        var result = Ip4SubnetParser.GetRanges("Server at 192.168.1.100 is down").ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Address.Parse("192.168.1.100"), result[0].FirstAddress);
    }

    #endregion

    #region GetSubnets - Single IP

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1/32")]
    [InlineData("0.0.0.0", "0.0.0.0/32")]
    [InlineData("255.255.255.255", "255.255.255.255/32")]
    public void GetSubnets_SingleIp_ReturnsSubnet32(string input, string expectedSubnet)
    {
        var result = Ip4SubnetParser.GetSubnets(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Subnet.Parse(expectedSubnet), result[0]);
    }

    #endregion

    #region GetSubnets - CIDR Subnet

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0/24")]
    [InlineData("10.0.0.0/8", "10.0.0.0/8")]
    [InlineData("0.0.0.0/0", "0.0.0.0/0")]
    public void GetSubnets_CidrSubnet_ReturnsSameSubnet(string input, string expectedSubnet)
    {
        var result = Ip4SubnetParser.GetSubnets(input).ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Subnet.Parse(expectedSubnet), result[0]);
    }

    #endregion

    #region GetSubnets - IP Range Conversion

    [Fact]
    public void GetSubnets_IpRange_ReturnsMultipleSubnets()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.1-192.168.1.3").ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal(Ip4Subnet.Parse("192.168.1.1/32"), result[0]);
        Assert.Equal(Ip4Subnet.Parse("192.168.1.2/31"), result[1]);
    }

    [Fact]
    public void GetSubnets_AlignedRange_ReturnsSingleSubnet()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.0-192.168.1.255").ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Subnet.Parse("192.168.1.0/24"), result[0]);
    }

    #endregion

    #region GetSubnets - Multiple Items

    [Fact]
    public void GetSubnets_MultipleMixed_ReturnsAllSubnets()
    {
        var result = Ip4SubnetParser.GetSubnets("192.168.1.1, 192.168.2.0/24").ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal(Ip4Subnet.Parse("192.168.1.1/32"), result[0]);
        Assert.Equal(Ip4Subnet.Parse("192.168.2.0/24"), result[1]);
    }

    #endregion

    #region GetSubnets - Empty and Invalid Input

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("999.999.999.999")]
    public void GetSubnets_EmptyOrInvalidInput_ReturnsEmpty(string input)
    {
        var result = Ip4SubnetParser.GetSubnets(input).ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void GetSubnets_MixedValidAndInvalid_ReturnsOnlyValid()
    {
        var result = Ip4SubnetParser.GetSubnets("invalid 192.168.1.1 garbage").ToArray();

        Assert.Single(result);
        Assert.Equal(Ip4Subnet.Parse("192.168.1.1/32"), result[0]);
    }

    #endregion
}
