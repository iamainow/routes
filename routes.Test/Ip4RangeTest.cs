namespace routes.Test;

public class Ip4RangeTest
{
    [Theory]
    [InlineData("12.23.34.0", "12.23.34.255", "12.23.34.0/24")]
    [InlineData("12.23.0.0", "12.23.255.255", "12.23.0.0/16")]
    [InlineData("12.0.0.0", "12.255.255.255", "12.0.0.0/8")]
    [InlineData("0.0.0.0", "127.255.255.255", "0.0.0.0/1")]
    [InlineData("0.0.0.0", "255.255.255.255", "0.0.0.0/0")]
    public void ToSubnets_SingleSubnet_ReturnsExpectedSubnet(string start, string end, string expectedResult)
    {
        Ip4Range range = new(Ip4Address.Parse(start), Ip4Address.Parse(end));

        Assert.Equal(expectedResult, string.Join(", ", range.ToSubnets().ToArray().OrderBy(x => x.FirstAddress)));
    }

    [Theory]
    [InlineData("0.0.0.0", "2.255.255.255", "0.0.0.0/7, 2.0.0.0/8")]
    [InlineData("0.0.0.10", "0.0.0.42", "0.0.0.10/31, 0.0.0.12/30, 0.0.0.16/28, 0.0.0.32/29, 0.0.0.40/31, 0.0.0.42/32")]
    [InlineData("81.3.192.0", "81.4.191.255", "81.3.192.0/18, 81.4.0.0/17, 81.4.128.0/18")]
    public void ToSubnets_MultipleSubnets_ReturnsExpectedSubnets(string start, string end, string expectedResult)
    {
        Ip4Range range = new(Ip4Address.Parse(start), Ip4Address.Parse(end));

        Assert.Equal(expectedResult, string.Join(", ", range.ToSubnets().ToArray().OrderBy(x => x.FirstAddress)));
    }

    [Fact]
    public void Constructor_ValidRange_CreatesRange()
    {
        var start = new Ip4Address(10);
        var end = new Ip4Address(20);

        var range = new Ip4Range(start, end);

        Assert.Equal(start, range.FirstAddress);
        Assert.Equal(end, range.LastAddress);
    }

    [Fact]
    public void Constructor_EndBeforeStart_ThrowsArgumentException()
    {
        var start = new Ip4Address(20);
        var end = new Ip4Address(10);

        Assert.Throws<ArgumentException>(() => new Ip4Range(start, end));
    }

    [Fact]
    public void Constructor_SameStartAndEnd_CreatesValidRange()
    {
        var address = new Ip4Address(10);

        var range = new Ip4Range(address, address);

        Assert.Equal(address, range.FirstAddress);
        Assert.Equal(address, range.LastAddress);
        Assert.Equal(1UL, range.Count);
    }

    [Theory]
    [InlineData(10, 20, 11)]
    [InlineData(0, 0, 1)]
    [InlineData(0, 255, 256)]
    [InlineData(100, 199, 100)]
    public void Count_ReturnsCorrectNumberOfAddresses(uint start, uint end, ulong expectedCount)
    {
        var range = new Ip4Range(new Ip4Address(start), new Ip4Address(end));

        Assert.Equal(expectedCount, range.Count);
    }

    [Theory]
    [InlineData(10, 20, 15, 25, true)]
    [InlineData(10, 20, 5, 15, true)]
    [InlineData(10, 20, 12, 18, true)]
    [InlineData(10, 20, 10, 20, true)]
    [InlineData(10, 20, 21, 30, false)]
    [InlineData(10, 20, 0, 9, false)]
    public void IsIntersects_VariousRanges_ReturnsExpectedResult(uint s1, uint e1, uint s2, uint e2, bool expected)
    {
        var range1 = new Ip4Range(new Ip4Address(s1), new Ip4Address(e1));
        var range2 = new Ip4Range(new Ip4Address(s2), new Ip4Address(e2));

        Assert.Equal(expected, range1.IsIntersects(range2));
    }

    [Fact]
    public void IntersectableUnion_OverlappingRanges_ReturnsMergedRange()
    {
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));

        var result = range1.IntersectableUnion(range2);

        Assert.Equal(new Ip4Address(10), result.FirstAddress);
        Assert.Equal(new Ip4Address(25), result.LastAddress);
    }

    [Fact]
    public void IntersectableIntersect_OverlappingRanges_ReturnsIntersection()
    {
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        var range2 = new Ip4Range(new Ip4Address(20), new Ip4Address(40));

        var result = range1.IntersectableIntersect(range2);

        Assert.Equal(new Ip4Address(20), result.FirstAddress);
        Assert.Equal(new Ip4Address(30), result.LastAddress);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var range = new Ip4Range(new Ip4Address(192, 168, 1, 1), new Ip4Address(192, 168, 1, 10));

        Assert.Equal("192.168.1.1-192.168.1.10", range.ToString());
    }

    [Fact]
    public void Equals_SameRange_ReturnsTrue()
    {
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
        Assert.False(range1 != range2);
    }

    [Fact]
    public void Equals_DifferentRange_ReturnsFalse()
    {
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(21));

        Assert.False(range1.Equals(range2));
        Assert.False(range1 == range2);
        Assert.True(range1 != range2);
    }

    [Fact]
    public void GetHashCode_SameRange_ReturnsSameHash()
    {
        var range1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var range2 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
    }

    [Fact]
    public void ImplicitCast_ToIp4RangeSet_CreatesSetWithSingleRange()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        Ip4RangeSet set = range;

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(range, ranges[0]);
    }

    [Fact]
    public void ToIp4RangeSet_CreatesSetWithSingleRange()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var set = range.ToIp4RangeSet();

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(range, ranges[0]);
    }

    [Fact]
    public void StaticField_All_CoversEntireAddressSpace()
    {
        Assert.Equal(new Ip4Address(0), Ip4Range.All.FirstAddress);
        Assert.Equal(new Ip4Address(0xFFFFFFFF), Ip4Range.All.LastAddress);
        Assert.Equal(4294967296UL, Ip4Range.All.Count);
    }

    [Fact]
    public void ToSubnets_SingleAddress_ReturnsSingleSubnet()
    {
        var range = new Ip4Range(new Ip4Address(192, 168, 1, 1), new Ip4Address(192, 168, 1, 1));

        var subnets = range.ToSubnets().ToArray();

        Assert.Single(subnets);
        Assert.Equal(32, subnets[0].Mask.Cidr);
    }

    [Fact]
    public void ToSubnets_ComplexRange_ReturnsMultipleSubnets()
    {
        var range = new Ip4Range(Ip4Address.Parse("192.168.1.0"), Ip4Address.Parse("192.168.1.127"));

        var subnets = range.ToSubnets().ToArray();

        Assert.NotEmpty(subnets);
        // Verify all subnets cover the range
        var reconstructed = new Ip4RangeSet(subnets);
        var reconstructedRanges = reconstructed.ToIp4Ranges();
        Assert.Single(reconstructedRanges);
        Assert.Equal(range.FirstAddress, reconstructedRanges[0].FirstAddress);
        Assert.Equal(range.LastAddress, reconstructedRanges[0].LastAddress);
    }

    #region GeneralComparison Tests

    [Theory]
    [InlineData(10, 20, 30, 40, -1)]  // first completely before second
    [InlineData(30, 40, 10, 20, 1)]   // first completely after second
    [InlineData(10, 30, 20, 40, 0)]   // overlapping
    [InlineData(10, 20, 10, 20, 0)]   // identical
    [InlineData(10, 30, 15, 25, 0)]   // second contained in first
    [InlineData(15, 25, 10, 30, 0)]   // first contained in second
    [InlineData(10, 20, 20, 30, 0)]   // touching at boundary
    [InlineData(10, 20, 21, 30, -1)]  // adjacent (not touching)
    public void GeneralComparison_Static_VariousRanges_ReturnsExpectedResult(
        uint s1, uint e1, uint s2, uint e2, int expected)
    {
        var range1 = new Ip4Range(new Ip4Address(s1), new Ip4Address(e1));
        var range2 = new Ip4Range(new Ip4Address(s2), new Ip4Address(e2));

        int result = Ip4Range.GeneralComparison(range1, range2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, 20, 30, 40, -1)]
    [InlineData(30, 40, 10, 20, 1)]
    [InlineData(10, 30, 20, 40, 0)]
    public void GeneralComparison_Instance_VariousRanges_ReturnsExpectedResult(
        uint s1, uint e1, uint s2, uint e2, int expected)
    {
        var range1 = new Ip4Range(new Ip4Address(s1), new Ip4Address(e1));
        var range2 = new Ip4Range(new Ip4Address(s2), new Ip4Address(e2));

        int result = range1.GeneralComparison(range2);

        Assert.Equal(expected, result);
    }

    #endregion

    #region OverlappingComparison Tests

    [Theory]
    [InlineData(10, 30, 20, 40, -1, -1)]  // first starts before, ends before
    [InlineData(20, 40, 10, 30, 1, 1)]    // first starts after, ends after
    [InlineData(10, 40, 20, 30, -1, 1)]   // first contains second
    [InlineData(20, 30, 10, 40, 1, -1)]   // second contains first
    [InlineData(10, 30, 10, 40, 0, -1)]   // same start, first ends before
    [InlineData(10, 40, 10, 30, 0, 1)]    // same start, first ends after
    [InlineData(10, 30, 10, 30, 0, 0)]    // identical
    public void OverlappingComparison_Static_VariousRanges_ReturnsExpectedTuple(
        uint s1, uint e1, uint s2, uint e2, int expectedFirst, int expectedLast)
    {
        var range1 = new Ip4Range(new Ip4Address(s1), new Ip4Address(e1));
        var range2 = new Ip4Range(new Ip4Address(s2), new Ip4Address(e2));

        var (firstCmp, lastCmp) = Ip4Range.OverlappingComparison(range1, range2);

        Assert.Equal(expectedFirst, Math.Sign(firstCmp));
        Assert.Equal(expectedLast, Math.Sign(lastCmp));
    }

    [Theory]
    [InlineData(10, 30, 20, 40, -1, -1)]
    [InlineData(10, 30, 10, 30, 0, 0)]
    public void OverlappingComparison_Instance_VariousRanges_ReturnsExpectedTuple(
        uint s1, uint e1, uint s2, uint e2, int expectedFirst, int expectedLast)
    {
        var range1 = new Ip4Range(new Ip4Address(s1), new Ip4Address(e1));
        var range2 = new Ip4Range(new Ip4Address(s2), new Ip4Address(e2));

        var (firstCmp, lastCmp) = range1.OverlappingComparison(range2);

        Assert.Equal(expectedFirst, Math.Sign(firstCmp));
        Assert.Equal(expectedLast, Math.Sign(lastCmp));
    }

    #endregion

    #region IntersectableExcept Tests

    [Fact]
    public void IntersectableExcept_CompletelyContained_ReturnsEmpty()
    {
        var range = new Ip4Range(new Ip4Address(20), new Ip4Address(30));
        var except = new Ip4Range(new Ip4Address(10), new Ip4Address(40));

        var result = range.IntersectableExcept(except);

        Assert.Empty(result);
    }

    [Fact]
    public void IntersectableExcept_LeftPartOnly_ReturnsLeftPart()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        var except = new Ip4Range(new Ip4Address(20), new Ip4Address(40));

        var result = range.IntersectableExcept(except);

        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), result[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_RightPartOnly_ReturnsRightPart()
    {
        var range = new Ip4Range(new Ip4Address(20), new Ip4Address(40));
        var except = new Ip4Range(new Ip4Address(10), new Ip4Address(30));

        var result = range.IntersectableExcept(except);

        Assert.Single(result);
        Assert.Equal(new Ip4Address(31), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_MiddleHole_ReturnsBothParts()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(40));
        var except = new Ip4Range(new Ip4Address(20), new Ip4Address(30));

        var result = range.IntersectableExcept(except);

        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), result[0].LastAddress);
        Assert.Equal(new Ip4Address(31), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[1].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_ExactMatch_ReturnsEmpty()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var except = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var result = range.IntersectableExcept(except);

        Assert.Empty(result);
    }

    [Fact]
    public void IntersectableExcept_AtMinimumBoundary_HandlesCorrectly()
    {
        var range = new Ip4Range(new Ip4Address(0), new Ip4Address(20));
        var except = new Ip4Range(new Ip4Address(0), new Ip4Address(10));

        var result = range.IntersectableExcept(except);

        Assert.Single(result);
        Assert.Equal(new Ip4Address(11), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_AtMaximumBoundary_HandlesCorrectly()
    {
        var range = new Ip4Range(new Ip4Address(uint.MaxValue - 20), new Ip4Address(uint.MaxValue));
        var except = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue));

        var result = range.IntersectableExcept(except);

        Assert.Single(result);
        Assert.Equal(new Ip4Address(uint.MaxValue - 20), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 11), result[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_SingleAddressExcept_CreatesTwoParts()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(30));
        var except = new Ip4Range(new Ip4Address(20), new Ip4Address(20));

        var result = range.IntersectableExcept(except);

        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), result[0].LastAddress);
        Assert.Equal(new Ip4Address(21), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[1].LastAddress);
    }

    #endregion
}