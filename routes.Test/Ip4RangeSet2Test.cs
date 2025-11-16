namespace routes.Test;

public class Ip4RangeSet2Test
{
    [Fact]
    public void Union_Overlapping_MergesIntoSingleRange()
    {
        // Arrange
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set = new Ip4RangeSet2(r1);

        // Act
        set.Union(r2);

        // Assert
        Ip4Range[] ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_Disjoint_KeepsBothRanges()
    {
        // Arrange
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(22), new Ip4Address(30));
        var set = new Ip4RangeSet2(r1);

        // Act
        set.Union(r2);

        // Assert
        Ip4Range[] ranges = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(22), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    //[Fact]
    //public void Union_Adjacent_NotMergedUntilNormalize()
    //{
    //    // Arrange: [10-20] and [21-30] are adjacent with a 0-sized gap (delta=0 requires Normalize)
    //    var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
    //    var r2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
    //    var set = new Ip4RangeSet2(r1);

    //    // Act
    //    set.Union(r2);

    //    // Assert: still two ranges before Normalize()
    //    Ip4Range[] beforeNormalize = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
    //    Assert.Equal(2, beforeNormalize.Length);

    //    // Normalize should merge adjacency
    //    var normalized = set.Normalize();
    //    Ip4Range[] afterNormalize = normalized.ToIp4Ranges();
    //    Assert.Single(afterNormalize);
    //    Assert.Equal(new Ip4Address(10), afterNormalize[0].FirstAddress);
    //    Assert.Equal(new Ip4Address(30), afterNormalize[0].LastAddress);
    //}

    [Fact]
    public void Union_WithIp4RangeSet2_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2();

        Assert.Throws<ArgumentNullException>(() => set.Union((Ip4RangeSet2)null!));
    }

    [Fact]
    public void Union_WithIp4RangeSet2_CombinesTwoSets()
    {
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_MultipleRangesOverlapping()
    {
        // Arrange: set1 has [10-20] and [40-50], set2 has [15-35]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(15), new Ip4Address(35)));

        // Act
        set1.Union(set2);

        // Assert: should have [10-35] and [40-50]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(35), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_CompletelyOverlappingRanges()
    {
        // Arrange: set1 has [10-50], set2 has [15-25]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(15), new Ip4Address(25)));

        // Act
        set1.Union(set2);

        // Assert: should remain [10-50]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_EmptySet_DoesNothing()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2();

        // Act
        set1.Union(set2);

        // Assert: set1 should remain unchanged
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_EmptySetUnioningNonEmpty()
    {
        // Arrange
        var set1 = new Ip4RangeSet2();
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Union(set2);

        // Assert: set1 should now contain set2's range
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_MultipleDisjointRanges()
    {
        // Arrange: set1 has [10-20], [40-50], set2 has [60-70], [80-90]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(60), new Ip4Address(70)),
            new Ip4Range(new Ip4Address(80), new Ip4Address(90))
        });

        // Act
        set1.Union(set2);

        // Assert: should have all 4 ranges
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(4, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(60), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(70), ranges[2].LastAddress);
        Assert.Equal(new Ip4Address(80), ranges[3].FirstAddress);
        Assert.Equal(new Ip4Address(90), ranges[3].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_PartialOverlap()
    {
        // Arrange: set1 has [10-30], set2 has [20-40]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Act
        set1.Union(set2);

        // Assert: should merge into [10-40]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

    //[Fact]
    //public void Union_WithIp4RangeSet2_AdjacentRangesBridgesTogetherAfterMultipleUnions()
    //{
    //    // Arrange: set1 has [10-20], set2 has [21-30], set3 has [31-40]
    //    var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
    //    var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
    //    var set3 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));

    //    // Act
    //    set1.Union(set2);
    //    set1.Union(set3);

    //    // Assert: before Normalize, should have 3 ranges
    //    var beforeNormalize = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
    //    Assert.Equal(3, beforeNormalize.Length);

    //    // After Normalize, should merge to single range
    //    var normalized = set1.Normalize();
    //    var ranges = normalized.ToIp4Ranges();
    //    Assert.Single(ranges);
    //    Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
    //    Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    //}

    [Fact]
    public void Union_WithIp4RangeSet2_LargeAndSmallRanges()
    {
        // Arrange: set1 has [0-1000000], set2 has [100-200]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(0), new Ip4Address(1000000)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));

        // Act
        set1.Union(set2);

        // Assert: should remain as single range [0-1000000]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(1000000), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet2_ConstructorWithMultipleRanges()
    {
        // Arrange: set1 constructed with multiple overlapping ranges, set2 with separate range
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(15), new Ip4Address(25))
        });
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act
        set1.Union(set2);

        // Assert
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        // Constructor should have already merged [10-20] and [15-25] to [10-25]
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    [Fact]
    public void Constructor_Empty_CreatesEmptySet()
    {
        var set = new Ip4RangeSet2();

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Constructor_FromSingleRange_CreatesSetWithOneRange()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var set = new Ip4RangeSet2(range);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(range, ranges[0]);
    }

    [Fact]
    public void Constructor_FromSingleSubnet_CreatesSetWithOneRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        var set = new Ip4RangeSet2(subnet);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(subnet.FirstAddress, ranges[0].FirstAddress);
        Assert.Equal(subnet.LastAddress, ranges[0].LastAddress);
    }

    [Fact]
    public void Constructor_FromRangeEnumerable_MergesOverlappingRanges()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(15), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };

        var set = new Ip4RangeSet2(ranges);

        var result = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), result[0].LastAddress);
        Assert.Equal(new Ip4Address(30), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[1].LastAddress);
    }

    [Fact]
    public void Constructor_FromSubnetEnumerable_CreatesCorrectRanges()
    {
        // Arrange: Three non-overlapping subnets
        var subnets = new[]
        {
            Ip4Subnet.Parse("192.168.0.0/24"),
            Ip4Subnet.Parse("192.168.1.0/24"),
            Ip4Subnet.Parse("192.168.3.0/24")
        };

        // Act
        var set = new Ip4RangeSet2(subnets);

        // Assert: Should have 3 ranges since they don't overlap
        var ranges = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(3, ranges.Length);
    }

    [Fact]
    public void Constructor_FromRangeEnumerable_NullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet2((IEnumerable<Ip4Range>)null!));
    }

    [Fact]
    public void Constructor_FromSubnetEnumerable_NullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet2((IEnumerable<Ip4Subnet>)null!));
    }

    //[Fact]
    //public void Normalize_MergesAdjacentRanges()
    //{
    //    var ranges = new[]
    //    {
    //        new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
    //        new Ip4Range(new Ip4Address(21), new Ip4Address(30))
    //    };
    //    var set = new Ip4RangeSet2(ranges);

    //    var normalized = set.Normalize();

    //    var result = normalized.ToIp4Ranges();
    //    Assert.Single(result);
    //    Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
    //    Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    //}

    [Fact]
    public void ToIp4Ranges_ReturnsCorrectRanges()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var set = new Ip4RangeSet2(range);

        var ranges = set.ToIp4Ranges();

        Assert.Single(ranges);
        Assert.Equal(range, ranges[0]);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };
        var set = new Ip4RangeSet2(ranges);

        var result = set.ToString();

        Assert.Contains("0.0.0.10-0.0.0.20", result);
        Assert.Contains("0.0.0.30-0.0.0.40", result);
    }

    [Fact]
    public void StaticFields_HaveCorrectValues()
    {
        Assert.Empty(Ip4RangeSet2.Empty.ToIp4Ranges());

        var allRanges = Ip4RangeSet2.All.ToIp4Ranges();
        Assert.Single(allRanges);
        Assert.Equal(new Ip4Address(0), allRanges[0].FirstAddress);
        Assert.Equal(new Ip4Address(0xFFFFFFFF), allRanges[0].LastAddress);
    }

    [Fact]
    public void ToIp4Subnets_ConvertsRangesToSubnets()
    {
        var range = new Ip4Range(Ip4Address.Parse("192.168.0.0"), Ip4Address.Parse("192.168.0.255"));
        var set = new Ip4RangeSet2(range);

        var subnets = set.ToIp4Subnets();

        Assert.NotEmpty(subnets);
    }

    #region Except(Ip4RangeSet2 other) Tests

    [Fact]
    public void Except_WithIp4RangeSet2_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        Assert.Throws<ArgumentNullException>(() => set.Except((Ip4RangeSet2)null!));
    }

    [Fact]
    public void Except_WithIp4RangeSet2_EmptyOtherSet_LeavesSetUnchanged()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2();

        // Act
        set1.Except(set2);

        // Assert
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_EmptyFirstSet_RemainsEmpty()
    {
        // Arrange
        var set1 = new Ip4RangeSet2();
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Except(set2);

        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_WithIp4RangeSet2_NonOverlappingRanges_LeavesSetUnchanged()
    {
        // Arrange: set1 has [10-20], set2 has [30-40]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act
        set1.Except(set2);

        // Assert
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_CompleteOverlap_RemovesEntireRange()
    {
        // Arrange: set1 has [10-20], set2 has [5-30] (completely contains set1)
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(5), new Ip4Address(30)));

        // Act
        set1.Except(set2);

        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_WithIp4RangeSet2_PartialOverlap_Left()
    {
        // Arrange: set1 has [10-30], set2 has [20-40] (overlaps right side)
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Act
        set1.Except(set2);

        // Assert: Should leave [10-19]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_PartialOverlap_Right()
    {
        // Arrange: set1 has [20-40], set2 has [10-30] (overlaps left side)
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set1.Except(set2);

        // Assert: Should leave [31-40]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(31), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_HoleInMiddle_SplitsRange()
    {
        // Arrange: set1 has [10-40], set2 has [20-30] (creates hole in middle)
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(40)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));

        // Act
        set1.Except(set2);

        // Assert: Should have two ranges [10-19] and [31-40]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(31), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_MultipleRangesExceptSingle()
    {
        // Arrange: set1 has [10-20] and [40-50], set2 has [15-45]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(15), new Ip4Address(45)));

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14] and [46-50]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(46), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_MultipleRangesExceptMultiple()
    {
        // Arrange: set1 has [10-20] and [40-50], set2 has [15-25] and [35-45]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(35), new Ip4Address(45))
        });

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14] and [46-50]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(46), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_ExceptExactMatch_RemovesRange()
    {
        // Arrange: set1 has [10-20], set2 has [10-20]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Except(set2);

        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_WithIp4RangeSet2_ExceptSmallRangeFromLarge()
    {
        // Arrange: set1 has [0-1000], set2 has [100-200]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));

        // Act
        set1.Except(set2);

        // Assert: Should have [0-99] and [201-1000]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(99), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(201), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(1000), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_MultipleNonSequentialRanges_ExceptMultiple()
    {
        // Arrange: set1 has [10-20], [30-40], [50-60], [70-80]
        // set2 has [15-35], [55-65]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(55), new Ip4Address(65))
        });

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14], [50-54], [70-80]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(4, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(36), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(50), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(54), ranges[2].LastAddress);
        Assert.Equal(new Ip4Address(70), ranges[3].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[3].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_RemoveMultipleRangesCompletely()
    {
        // Arrange: set1 has [10-20], [30-40], [50-60]
        // set2 has [10-60] (covers all)
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(60)));

        // Act
        set1.Except(set2);

        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_WithIp4RangeSet2_PartiallyRemoveMultipleRanges()
    {
        // Arrange: set1 has [10-20], [30-40], [50-60]
        // set2 has [15-55]
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14] and [56-60]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(56), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_TouchingButNotOverlappingRanges()
    {
        // Arrange: set1 has [10-20], set2 has [21-30] (adjacent but not overlapping)
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        // Act
        set1.Except(set2);

        // Assert: set1 should remain unchanged
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_SingleAddressRange()
    {
        // Arrange: set1 has [10-10], set2 has [10-10]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));

        // Act
        set1.Except(set2);

        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_WithIp4RangeSet2_ExceptSingleAddressFromRange()
    {
        // Arrange: set1 has [10-20], set2 has [15-15]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(15), new Ip4Address(15)));

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14] and [16-20]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(16), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_WithIp4RangeSet2_ComplexScenarioWithMultipleOperations()
    {
        // Arrange: Build complex set through unions, then except multiple ranges
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(45), new Ip4Address(75))
        });

        // Act
        set1.Except(set2);

        // Assert: Should have [10-14] and [76-80]
        var ranges = set1.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(44), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(76), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[2].LastAddress);
    }

    #endregion
}
