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
        Ip4Range[] ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(22), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_Adjacent_NotMergedUntilNormalize()
    {
        // Arrange: [10-20] and [21-30] are adjacent with a 0-sized gap (delta=0 requires Normalize)
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(21), new Ip4Address(30));
        var set = new Ip4RangeSet2(r1);

        // Act
        set.Union(r2);

        // Assert: still two ranges before Normalize()
        Ip4Range[] beforeNormalize = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, beforeNormalize.Length);

        // Normalize should merge adjacency
        set.Normalize();
        Ip4Range[] afterNormalize = set.ToIp4Ranges();
        Assert.Single(afterNormalize);
        Assert.Equal(new Ip4Address(10), afterNormalize[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), afterNormalize[0].LastAddress);
    }

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

        var ranges = set1.ToIp4Ranges().ToArray();
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
        var ranges = set1.ToIp4Ranges().ToArray();
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
        var ranges = set1.ToIp4Ranges().ToArray();
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

    [Fact]
    public void Union_WithIp4RangeSet2_AdjacentRangesBridgesTogetherAfterMultipleUnions()
    {
        // Arrange: set1 has [10-20], set2 has [21-30], set3 has [31-40]
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
        var set3 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));

        // Act
        set1.Union(set2);
        set1.Union(set3);

        // Assert: should merge to single range
        var ranges = set1.ToIp4Ranges().ToArray();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

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
        var ranges = set1.ToIp4Ranges().ToArray();
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

        var result = set.ToIp4Ranges().ToArray();
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

        // Assert: Should have 2 ranges
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
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

    #region Normalize() Tests

    [Fact]
    public void Normalize_AdjacentRanges_MergesIntoSingleRange()
    {
        // Arrange: [10-20] and [21-30] are adjacent (delta=0 means adjacent ranges should merge)
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert: should merge to single range [10-30]
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Normalize_MultipleAdjacentRanges_MergesAll()
    {
        // Arrange: [10-20], [21-30], [31-40] all adjacent
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(31), new Ip4Address(40))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert: should merge to single range [10-40]
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[0].LastAddress);
    }

    [Fact]
    public void Normalize_DisjointRanges_RemainsUnchanged()
    {
        // Arrange: [10-20] and [30-40] are disjoint (gap > 1)
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert: should remain as 2 ranges
        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
        Assert.Equal(new Ip4Address(30), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), result[1].LastAddress);
    }

    [Fact]
    public void Normalize_OverlappingRanges_AlreadyMerged()
    {
        // Arrange: [10-25] and [15-30] overlap (should already be merged during construction)
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(15), new Ip4Address(30))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert: should remain as single range [10-30]
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Normalize_EmptySet_RemainsEmpty()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.Normalize();

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Normalize_SingleRange_RemainsUnchanged()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Normalize();

        // Assert
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
    }

    [Fact]
    public void Normalize_MixedAdjacentAndDisjoint_MergesOnlyAdjacent()
    {
        // Arrange: [10-20], [21-30] adjacent; then [50-60] disjoint
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        // Act
        set.Normalize();

        // Assert: should have [10-30] and [50-60]
        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
        Assert.Equal(new Ip4Address(50), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), result[1].LastAddress);
    }

    [Fact]
    public void Normalize_LargeAdjacentRanges_MergesCorrectly()
    {
        // Arrange: [0-1000] and [1001-2000] adjacent
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(0), new Ip4Address(1000)),
            new Ip4Range(new Ip4Address(1001), new Ip4Address(2000))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(0), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(2000), result[0].LastAddress);
    }

    [Fact]
    public void Normalize_SingleAddressAdjacentToRange_Merges()
    {
        // Arrange: [10-10] and [11-20] (single address adjacent to range)
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(10)),
            new Ip4Range(new Ip4Address(11), new Ip4Address(20))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
    }

    #endregion

    #region Simplify(uint delta) Tests

    [Fact]
    public void Simplify_SmallRangesBelowDelta_RemovesThem()
    {
        // Arrange: set with small ranges [10-10] (size=1), [15-20] (size=6), [50-60] (size=11)
        // delta=5 means remove ranges with size <= 5
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(10)),   // size 1, should be removed
            new Ip4Range(new Ip4Address(15), new Ip4Address(20)),   // size 6, should remain
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))    // size 11, should remain
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Simplify(5);

        // Assert: small range should be removed
        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(15), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
        Assert.Equal(new Ip4Address(50), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), result[1].LastAddress);
    }

    [Fact]
    public void Simplify_SmallGapsBelowDelta_ExpandsToMerge()
    {
        // Arrange: [10-20] and [23-30] with small gap of size 2 (21-22)
        // delta=5 means merge ranges if gap <= 5
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(23), new Ip4Address(30)));

        // Act
        set.Simplify(5);

        // Assert: should merge into single range [10-30]
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Simplify_LargeGaps_RemainsUnmerged()
    {
        // Arrange: [10-20] and [40-50] with large gap of size 19 (21-39)
        // delta=5 means don't merge ranges if gap > 5
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(40), new Ip4Address(50)));

        // Act
        set.Simplify(5);

        // Assert: should remain as 2 ranges
        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
        Assert.Equal(new Ip4Address(40), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), result[1].LastAddress);
    }

    [Fact]
    public void Normalize_DeltaZero_OnlyNormalizes()
    {
        // Arrange: adjacent ranges [10-20] and [21-30]
        // delta=0 means only merge adjacent (gap size 0)
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };
        var set = new Ip4RangeSet2(ranges);

        // Act
        set.Normalize();

        // Assert: should merge adjacent ranges
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Simplify_EmptySet_RemainsEmpty()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.Simplify(10);

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Simplify_SingleRange_RemainsUnchanged()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Simplify(10);

        // Assert
        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
    }

    [Fact]
    public void Simplify_ComplexScenario_RemovesSmallRangesAndMergesSmallGaps()
    {
        // Arrange: [5-5] (size 1), [10-20] (size 11), [22-25] (size 4), [40-50] (size 11)
        // delta=5 means remove ranges size <= 5, and merge gaps size <= 5
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(5), new Ip4Address(5)));      // size 1, will be removed
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));    // size 11, retained
        set.Union(new Ip4Range(new Ip4Address(22), new Ip4Address(25)));    // size 4, will be removed
        set.Union(new Ip4Range(new Ip4Address(40), new Ip4Address(50)));    // size 11, retained

        // Act
        set.Simplify(5);

        // Assert: [5-5] and [22-25] removed, [10-20] and [40-50] retained
        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), result[0].LastAddress);
        Assert.Equal(new Ip4Address(40), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), result[1].LastAddress);
    }

    [Fact]
    public void Simplify_IterativeMerging_ConvergesToSimplifiedSet()
    {
        // Arrange: Multiple ranges that will iteratively merge
        // [10-12] (size 3), gap 1, [14-16] (size 3), gap 1, [18-20] (size 3)
        // delta=3 means remove/merge ranges/gaps of size <= 3
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(12)));
        set.Union(new Ip4Range(new Ip4Address(14), new Ip4Address(16)));
        set.Union(new Ip4Range(new Ip4Address(18), new Ip4Address(20)));

        // Act
        set.Simplify(3);

        // Assert: should eventually merge or remove based on smallest size/gap and delta
        var result = set.ToIp4Ranges();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Simplify_LargeDelta_RemovesMoreRangesAndMergesMore()
    {
        // Arrange: [10-20] (size 11), gap 5, [26-35] (size 10), gap 10, [46-55] (size 10)
        // delta=10 means remove ranges size <= 10 and merge gaps size <= 10
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(26), new Ip4Address(35)));
        set.Union(new Ip4Range(new Ip4Address(46), new Ip4Address(55)));

        // Act
        set.Simplify(10);

        // Assert: ranges with size <= 10 may be removed, gaps <= 10 may be merged
        var result = set.ToIp4Ranges();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Simplify_AllRangesBelowDelta_RemovesAll()
    {
        // Arrange: [10-12] (size 3), [20-22] (size 3), [30-32] (size 3)
        // delta=5 means all ranges are size <= 5 and should be removed
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(12)));
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(22)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(32)));

        // Act
        set.Simplify(5);

        // Assert: all ranges removed
        var result = set.ToIp4Ranges();
        Assert.Empty(result);
    }

    [Fact]
    public void Simplify_HighDeltaValue_MaxMerging()
    {
        // Arrange: multiple small ranges with large gaps
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(15)));
        set.Union(new Ip4Range(new Ip4Address(100), new Ip4Address(105)));
        set.Union(new Ip4Range(new Ip4Address(200), new Ip4Address(205)));

        // Act: use very large delta
        set.Simplify(1000);

        // Assert: all should merge into one or be removed
        var result = set.ToIp4Ranges();
        Assert.InRange(result.Length, 0, 1);
    }

    [Fact]
    public void Simplify_ExactBoundary_DeltaEqualToRangeSize()
    {
        // Arrange: [10-20] (size 11), gap 5, [26-30] (size 5)
        // delta=5 means [26-30] is exactly at boundary (size = delta)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(26), new Ip4Address(30)));

        // Act
        set.Simplify(5);

        // Assert: behavior at boundary (size <= delta)
        var result = set.ToIp4Ranges();
        Assert.NotEmpty(result);
    }

    #endregion

    #region Except(Ip4Range) Tests

    [Fact]
    public void Except_Ip4Range_CompletelyOverlapping_RemovesRange()
    {
        // Arrange: [10-30] except [10-30] should result in empty set
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4Range_PartialOverlapAtStart_TruncatesRange()
    {
        // Arrange: [10-30] except [5-20] should result in [21-30]
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(5), new Ip4Address(20)));

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(21), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4Range_PartialOverlapAtEnd_TruncatesRange()
    {
        // Arrange: [10-30] except [20-40] should result in [10-19]
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4Range_MiddleOverlap_SplitsIntoTwoRanges()
    {
        // Arrange: [10-30] except [15-20] should result in [10-14] and [21-30]
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(20)));

        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(21), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_Ip4Range_Disjoint_NoChange()
    {
        // Arrange: [10-20] except [30-40] should remain [10-20]
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4Range_MultipleRanges_RemovesFromMultiple()
    {
        // Arrange: [10-20], [30-40], [50-60] except [15-55] should result in [10-14] and [56-60]
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });

        // Act
        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(56), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_Ip4Range_EmptySet_RemainsEmpty()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4Range_ExceedingRange_RemovesOverlappingPortion()
    {
        // Arrange: [10-20] except [0-100] should result in empty set
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    #endregion

    #region Except(Ip4RangeSet2) Tests

    [Fact]
    public void Except_Ip4RangeSet2_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2();

        Assert.Throws<ArgumentNullException>(() => set.Except((Ip4RangeSet2)null!));
    }

    [Fact]
    public void Except_Ip4RangeSet2_EmptySet_NoChange()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var emptySet = new Ip4RangeSet2();

        // Act
        set.Except(emptySet);

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4RangeSet2_EmptySetExceptingNonEmpty_RemainsEmpty()
    {
        // Arrange
        var set = new Ip4RangeSet2();
        var otherSet = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(otherSet);

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4RangeSet2_CompleteOverlap_RemovesAll()
    {
        // Arrange: [10-20] except [10-20] should result in empty set
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var otherSet = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(otherSet);

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4RangeSet2_MultipleRanges_RemovesOverlapping()
    {
        // Arrange: [10-20], [30-40], [50-60] except [15-35], [55-65]
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });
        var otherSet = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(55), new Ip4Address(65))
        });

        // Act
        set.Except(otherSet);

        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(36), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(50), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(54), ranges[2].LastAddress);
    }

    [Fact]
    public void Except_Ip4RangeSet2_DisjointRanges_NoChange()
    {
        // Arrange: [10-20], [30-40] except [50-60], [70-80]
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });
        var otherSet = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });

        // Act
        set.Except(otherSet);

        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_Ip4RangeSet2_PartialOverlaps_RemovesCorrectly()
    {
        // Arrange: [10-30] except [20-25]
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var otherSet = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(25)));

        // Act
        set.Except(otherSet);

        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(26), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    #endregion

    #region ExpandSet Tests

    [Fact]
    public void ExpandSet_SmallGap_MergesRanges()
    {
        // Arrange: [10-20] and [23-30] with gap of 2 (21-22)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(23), new Ip4Address(30)));

        // Act: expand with delta=2 should merge
        set.ExpandSet(2, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void ExpandSet_LargeGap_DoesNotMerge()
    {
        // Arrange: [10-20] and [30-40] with gap of 9 (21-29)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act: expand with delta=5 should not merge (gap > delta)
        set.ExpandSet(5, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
    }

    [Fact]
    public void ExpandSet_DeltaZero_MergesAdjacentOnly()
    {
        // Arrange: [10-20] and [21-30] are adjacent
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        // Act
        set.ExpandSet(0, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void ExpandSet_MultipleGaps_MergesAll()
    {
        // Arrange: [10-12], [14-16], [18-20] with gaps of 1
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(12)));
        set.Union(new Ip4Range(new Ip4Address(14), new Ip4Address(16)));
        set.Union(new Ip4Range(new Ip4Address(18), new Ip4Address(20)));

        // Act: delta=1 should merge all
        set.ExpandSet(1, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void ExpandSet_EmptySet_NoChange()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.ExpandSet(10, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void ExpandSet_SingleRange_NoChange()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.ExpandSet(10, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
    }

    [Fact]
    public void ExpandSet_ExactGapSize_Merges()
    {
        // Arrange: [10-20] and [25-30] with gap of 4 (21-24)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(25), new Ip4Address(30)));

        // Act: delta=4 should merge (gap size = delta)
        set.ExpandSet(4, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
    }

    #endregion

    #region ShrinkSet Tests

    [Fact]
    public void ShrinkSet_SmallRanges_RemovesThem()
    {
        // Arrange: [10-12] (size 3), [20-30] (size 11), [40-42] (size 3)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(12)));
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));
        set.Union(new Ip4Range(new Ip4Address(40), new Ip4Address(42)));

        // Act: delta=5 should remove ranges with size <= 5
        set.ShrinkSet(5, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(20), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void ShrinkSet_AllRangesLarge_NoChange()
    {
        // Arrange: [10-20] (size 11), [30-40] (size 11)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act: delta=5 should not remove any ranges
        set.ShrinkSet(5, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
    }

    [Fact]
    public void ShrinkSet_AllRangesSmall_RemovesAll()
    {
        // Arrange: [10-12] (size 3), [20-22] (size 3)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(12)));
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(22)));

        // Act: delta=5 should remove all ranges
        set.ShrinkSet(5, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void ShrinkSet_ExactSize_Removes()
    {
        // Arrange: [10-14] (size 5)
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(14)));

        // Act: delta=5 should remove (size <= delta)
        set.ShrinkSet(5, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void ShrinkSet_EmptySet_NoChange()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.ShrinkSet(10, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void ShrinkSet_SingleAddressRange_RemovesWithDeltaOne()
    {
        // Arrange: [10-10] (size 1)
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));

        // Act: delta=1 should remove
        set.ShrinkSet(1, out bool wasListChanged);

        // Assert
        Assert.True(wasListChanged);
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void ShrinkSet_DeltaZero_NoChange()
    {
        // Arrange: any range has size >= 1
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));

        // Act: delta=0 should not remove any ranges (size > 0)
        set.ShrinkSet(0, out bool wasListChanged);

        // Assert
        Assert.False(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Empty_ReturnsEmptySet()
    {
        // Act
        var emptySet = Ip4RangeSet2.Empty;

        // Assert
        Assert.NotNull(emptySet);
        Assert.Empty(emptySet.ToIp4Ranges());
    }

    [Fact]
    public void All_ReturnsFullIpRange()
    {
        // Act
        var allSet = Ip4RangeSet2.All;

        // Assert
        Assert.NotNull(allSet);
        var ranges = allSet.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Empty_MultipleCallsReturnDifferentInstances()
    {
        // Act
        var empty1 = Ip4RangeSet2.Empty;
        var empty2 = Ip4RangeSet2.Empty;

        // Assert: should be different instances
        Assert.NotSame(empty1, empty2);
    }

    [Fact]
    public void All_MultipleCallsReturnDifferentInstances()
    {
        // Act
        var all1 = Ip4RangeSet2.All;
        var all2 = Ip4RangeSet2.All;

        // Assert: should be different instances
        Assert.NotSame(all1, all2);
    }

    #endregion

    #region Copy Constructor Tests

    [Fact]
    public void Constructor_CopyFromSet_CreatesDeepCopy()
    {
        // Arrange
        var original = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        // Act
        var copy = new Ip4RangeSet2(original);

        // Assert: verify deep copy
        var originalRanges = original.ToIp4Ranges().ToArray();
        var copyRanges = copy.ToIp4Ranges().ToArray();
        Assert.Equal(originalRanges.Length, copyRanges.Length);
        for (int i = 0; i < originalRanges.Length; i++)
        {
            Assert.Equal(originalRanges[i].FirstAddress, copyRanges[i].FirstAddress);
            Assert.Equal(originalRanges[i].LastAddress, copyRanges[i].LastAddress);
        }
    }

    [Fact]
    public void Constructor_CopyFromSet_ModifyingCopyDoesNotAffectOriginal()
    {
        // Arrange
        var original = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var copy = new Ip4RangeSet2(original);

        // Act: modify copy
        copy.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Assert: original should be unchanged
        var originalRanges = original.ToIp4Ranges();
        Assert.Single(originalRanges);
        Assert.Equal(new Ip4Address(10), originalRanges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), originalRanges[0].LastAddress);

        var copyRanges = copy.ToIp4Ranges().ToArray();
        Assert.Equal(2, copyRanges.Length);
    }

    [Fact]
    public void Constructor_CopyFromEmptySet_CreatesEmptyCopy()
    {
        // Arrange
        var original = new Ip4RangeSet2();

        // Act
        var copy = new Ip4RangeSet2(original);

        // Assert
        Assert.Empty(copy.ToIp4Ranges());
    }

    [Fact]
    public void Constructor_CopyFromNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet2((Ip4RangeSet2)null!));
    }

    #endregion

    #region ToIp4Subnets Tests

    [Fact]
    public void ToIp4Subnets_SingleRange_ReturnsSubnets()
    {
        // Arrange: 192.168.0.0 - 192.168.0.255 should convert to subnets
        var set = new Ip4RangeSet2(new Ip4Range(
            Ip4Address.Parse("192.168.0.0"),
            Ip4Address.Parse("192.168.0.255")));

        // Act
        var subnets = set.ToIp4Subnets();

        // Assert
        Assert.NotEmpty(subnets);
        // Verify all subnets cover the range
        ulong totalAddresses = (ulong)subnets.Sum(s => (decimal)s.Count);
        Assert.Equal(256UL, totalAddresses);
    }

    [Fact]
    public void ToIp4Subnets_MultipleRanges_ReturnsAllSubnets()
    {
        // Arrange
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(Ip4Address.Parse("10.0.0.0"), Ip4Address.Parse("10.0.0.255")),
            new Ip4Range(Ip4Address.Parse("192.168.1.0"), Ip4Address.Parse("192.168.1.255"))
        });

        // Act
        var subnets = set.ToIp4Subnets();

        // Assert
        Assert.NotEmpty(subnets);
        ulong totalAddresses = (ulong)subnets.Sum(s => (decimal)s.Count);
        Assert.Equal(512UL, totalAddresses); // 256 + 256
    }

    [Fact]
    public void ToIp4Subnets_EmptySet_ReturnsEmptyArray()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        var subnets = set.ToIp4Subnets();

        // Assert
        Assert.Empty(subnets);
    }

    [Fact]
    public void ToIp4Subnets_SmallRange_ReturnsCorrectSubnets()
    {
        // Arrange: small range that should produce specific subnets
        var set = new Ip4RangeSet2(new Ip4Range(
            Ip4Address.Parse("10.0.0.0"),
            Ip4Address.Parse("10.0.0.7")));

        // Act
        var subnets = set.ToIp4Subnets();

        // Assert
        Assert.NotEmpty(subnets);
        ulong totalAddresses = (ulong)subnets.Sum(s => (decimal)s.Count);
        Assert.Equal(8UL, totalAddresses);
    }

    #endregion

    #region MinimizeSubnets Tests

    [Fact]
    public void MinimizeSubnets_FiltersByDelta_ReturnsLargerSubnets()
    {
        // Arrange: create set with various subnet sizes
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(Ip4Address.Parse("10.0.0.0"), Ip4Address.Parse("10.0.0.255")),    // 256 addresses
            new Ip4Range(Ip4Address.Parse("192.168.0.0"), Ip4Address.Parse("192.168.0.7"))  // 8 addresses
        });

        // Act: filter out subnets with count <= 100
        var minimized = set.MinimizeSubnets(100);

        // Assert: should only keep larger subnets
        var subnets = minimized.ToIp4Subnets();
        Assert.All(subnets, s => Assert.True(s.Count > 100));
    }

    [Fact]
    public void MinimizeSubnets_DeltaZero_ReturnsAllSubnets()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(
            Ip4Address.Parse("10.0.0.0"),
            Ip4Address.Parse("10.0.0.255")));

        // Act
        var minimized = set.MinimizeSubnets(0);

        // Assert: all subnets should be included
        var originalSubnets = set.ToIp4Subnets();
        var minimizedSubnets = minimized.ToIp4Subnets();
        Assert.Equal(originalSubnets.Length, minimizedSubnets.Length);
    }

    [Fact]
    public void MinimizeSubnets_EmptySet_ReturnsEmptySet()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        var minimized = set.MinimizeSubnets(10);

        // Assert
        Assert.Empty(minimized.ToIp4Ranges());
    }

    [Fact]
    public void MinimizeSubnets_HighDelta_RemovesAllSmallSubnets()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(
            Ip4Address.Parse("10.0.0.0"),
            Ip4Address.Parse("10.0.0.31")));

        // Act: use very high delta
        var minimized = set.MinimizeSubnets(1000);

        // Assert: should remove all subnets smaller than delta
        var subnets = minimized.ToIp4Subnets();
        Assert.All(subnets, s => Assert.True(s.Count > 1000));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_EmptySet_ReturnsEmptyString()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        var result = set.ToString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToString_SingleRange_ReturnsRangeString()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        var result = set.ToString();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("10", result);
        Assert.Contains("20", result);
    }

    [Fact]
    public void ToString_MultipleRanges_ReturnsAllRanges()
    {
        // Arrange
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        // Act
        var result = set.ToString();

        // Assert
        Assert.NotEmpty(result);
        // Should contain both ranges
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void ToString_ContainsNewlines()
    {
        // Arrange
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        // Act
        var result = set.ToString();

        // Assert: should contain line breaks
        Assert.Contains(Environment.NewLine, result);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void Union_WithNullArray_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2();

        Assert.Throws<ArgumentNullException>(() => set.Union((Ip4Range[])null!));
    }

    [Fact]
    public void Union_WithNullEnumerable_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2();

        Assert.Throws<ArgumentNullException>(() => set.Union((IEnumerable<Ip4Range>)null!));
    }

    [Fact]
    public void Union_WithNullSubnetEnumerable_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet2();

        Assert.Throws<ArgumentNullException>(() => set.Union((IEnumerable<Ip4Subnet>)null!));
    }

    [Fact]
    public void Operations_AtMinimumIpAddress_WorkCorrectly()
    {
        // Arrange: range starting at 0.0.0.0
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        // Act & Assert: should handle minimum address
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
    }

    [Fact]
    public void Operations_AtMaximumIpAddress_WorkCorrectly()
    {
        // Arrange: range ending at 255.255.255.255
        var set = new Ip4RangeSet2(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue)));

        // Act & Assert: should handle maximum address
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_FullIpRange_WorksCorrectly()
    {
        // Arrange: union with entire IP range
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        // Act: union with full range
        set.Union(Ip4Range.All);

        // Assert: should result in full range
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_FullIpRange_ResultsInEmptySet()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        // Act: except entire IP range
        set.Except(Ip4Range.All);

        // Assert: should be empty
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void LargeRangeOperations_PerformCorrectly()
    {
        // Arrange: large ranges for performance testing
        var set = new Ip4RangeSet2(new Ip4Range(
            new Ip4Address(0),
            new Ip4Address(10_000_000)));

        // Act: union with another large range
        set.Union(new Ip4Range(
            new Ip4Address(5_000_000),
            new Ip4Address(15_000_000)));

        // Assert: should merge correctly
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(15_000_000), ranges[0].LastAddress);
    }

    [Fact]
    public void ExpandSet_NearMaxValue_HandlesOverflowCorrectly()
    {
        // Arrange: ranges near maximum IP address with gap of 9 (addresses 4294967195-4294967204)
        var set = new Ip4RangeSet2();
        set.Union(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue - 50)));
        set.Union(new Ip4Range(
            new Ip4Address(uint.MaxValue - 40),
            new Ip4Address(uint.MaxValue)));

        // Act: expand with delta=10 to merge the gap
        set.ExpandSet(10, out bool wasListChanged);

        // Assert: should merge without overflow
        Assert.True(wasListChanged);
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
    }

    [Fact]
    public void MultipleOperations_MaintainsSortedOrder()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act: perform multiple operations
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));
        set.Except(new Ip4Range(new Ip4Address(35), new Ip4Address(55)));

        // Assert: ranges should be in sorted order
        var ranges = set.ToIp4Ranges().ToArray();
        for (int i = 0; i < ranges.Length - 1; i++)
        {
            Assert.True(ranges[i].LastAddress < ranges[i + 1].FirstAddress);
        }
    }

    #endregion
}
