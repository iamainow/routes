namespace routes.Test;

public class Ip4RangeTest
{
    [Theory]
    [InlineData(0, 10, "0.0.0.11-0.0.0.20")]
    [InlineData(0, 20, "")]
    [InlineData(0, 30, "")]
    [InlineData(10, 15, "0.0.0.16-0.0.0.20")]
    [InlineData(10, 20, "")]
    [InlineData(10, 30, "")]
    [InlineData(15, 20, "0.0.0.10-0.0.0.14")]
    [InlineData(15, 30, "0.0.0.10-0.0.0.14")]
    [InlineData(20, 30, "0.0.0.10-0.0.0.19")]
    public void IntersectableExcept_Intersectable_BorderConditions(uint start, uint end, string expectedResult)
    {
        Ip4Range range = new(new Ip4Address(10), new Ip4Address(20));

        Ip4Range[] result = range.Except(new Ip4Range(new Ip4Address(start), new Ip4Address(end)));

        string actualResult = string.Join(',', result);

        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(0, 15, "0.0.0.16-0.0.0.20")]
    [InlineData(0, 30, "")]
    [InlineData(12, 18, "0.0.0.10-0.0.0.11,0.0.0.19-0.0.0.20")]
    [InlineData(15, 30, "0.0.0.10-0.0.0.14")]
    public void IntersectableExcept_Intersectable_NormalConditions(uint start, uint end, string expectedResult)
    {
        Ip4Range range = new(new Ip4Address(10), new Ip4Address(20));

        Ip4Range[] result = range.Except(new Ip4Range(new Ip4Address(start), new Ip4Address(end)));

        string actualResult = string.Join(',', result);

        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("12.23.34.0", "12.23.34.255", "12.23.34.0/24")]
    [InlineData("12.23.0.0", "12.23.255.255", "12.23.0.0/16")]
    [InlineData("12.0.0.0", "12.255.255.255", "12.0.0.0/8")]
    [InlineData("0.0.0.0", "127.255.255.255", "0.0.0.0/1")]
    [InlineData("0.0.0.0", "255.255.255.255", "0.0.0.0/0")]
    public void ToSubsets_SingularSubset(string start, string end, string expectedResult)
    {
        Ip4Range range = new(Ip4Address.Parse(start), Ip4Address.Parse(end));

        Assert.Equal(expectedResult, string.Join(", ", range.ToSubnets().OrderBy(x => x.FirstAddress)));
    }

    [Theory]
    [InlineData("0.0.0.0", "2.255.255.255", "0.0.0.0/7, 2.0.0.0/8")]
    [InlineData("0.0.0.10", "0.0.0.42", "0.0.0.10/31, 0.0.0.12/30, 0.0.0.16/28, 0.0.0.32/29, 0.0.0.40/31, 0.0.0.42/32")]
    [InlineData("81.3.192.0", "81.4.191.255", "81.3.192.0/18, 81.4.0.0/17, 81.4.128.0/18")]
    public void ToSubsets_MultipleSubsets(string start, string end, string expectedResult)
    {
        Ip4Range range = new(Ip4Address.Parse(start), Ip4Address.Parse(end));

        Assert.Equal(expectedResult, string.Join(", ", range.ToSubnets().OrderBy(x => x.FirstAddress)));
    }
}