namespace Ip4Parsers.Test;


public class Ip4SubnetParserTest
{
    [Fact]
    public void GetRanges2()
    {
        var result = Ip4SubnetParser.GetRanges2(" sdfsf 123.23.12.3-123.23.12.4, 123.23.12.3/32, 123.23.12.3 sdf ").ToArray();
        Assert.NotEmpty(result);
    }
}
