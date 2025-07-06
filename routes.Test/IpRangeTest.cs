using routes.core;

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
        Ip4Range range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var result = range.Except(new Ip4Range(new Ip4Address(start), new Ip4Address(end)));

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
        Ip4Range range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var result = range.Except(new Ip4Range(new Ip4Address(start), new Ip4Address(end)));

        string actualResult = string.Join(',', result);

        Assert.Equal(expectedResult, actualResult);
    }
}