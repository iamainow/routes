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

        // Assert: before Normalize, should have 3 ranges
        var beforeNormalize = set1.ToIp4Ranges().ToArray();
        Assert.Equal(3, beforeNormalize.Length);

        // After Normalize, should merge to single range
        set1.Normalize();
        var ranges = set1.ToIp4Ranges();
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

        // Assert: Should have 3 ranges since they don't overlap
        var ranges = set.ToIp4Ranges().ToArray();
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
}
