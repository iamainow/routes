namespace routes.Test;

public class Ip4RangeSetTest
{
    [Fact]
    public void Union_Overlapping_MergesIntoSingleRange()
    {
        // Arrange
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set = new Ip4RangeSet(r1);

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
        var set = new Ip4RangeSet(r1);

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
    public void Union_WithIp4RangeSet_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Union((Ip4RangeSet)null!));
    }

    [Fact]
    public void Union_WithIp4RangeSet_CombinesTwoSets()
    {
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_MultipleRangesOverlapping()
    {
        // Arrange: set1 has [10-20] and [40-50], set2 has [15-35]
        var set1 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(15), new Ip4Address(35)));

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
    public void Union_WithIp4RangeSet_CompletelyOverlappingRanges()
    {
        // Arrange: set1 has [10-50], set2 has [15-25]
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(15), new Ip4Address(25)));

        // Act
        set1.Union(set2);

        // Assert: should remain [10-50]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_EmptySet_DoesNothing()
    {
        // Arrange
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet();

        // Act
        set1.Union(set2);

        // Assert: set1 should remain unchanged
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_EmptySetUnioningNonEmpty()
    {
        // Arrange
        var set1 = new Ip4RangeSet();
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Union(set2);

        // Assert: set1 should now contain set2's range
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_MultipleDisjointRanges()
    {
        // Arrange: set1 has [10-20], [40-50], set2 has [60-70], [80-90]
        var set1 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet(new[]
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
    public void Union_WithIp4RangeSet_PartialOverlap()
    {
        // Arrange: set1 has [10-30], set2 has [20-40]
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Act
        set1.Union(set2);

        // Assert: should merge into [10-40]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_AdjacentRangesBridgesTogetherAfterMultipleUnions()
    {
        // Arrange: set1 has [10-20], set2 has [21-30], set3 has [31-40]
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
        var set3 = new Ip4RangeSet(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));

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
    public void Union_WithIp4RangeSet_LargeAndSmallRanges()
    {
        // Arrange: set1 has [0-1000000], set2 has [100-200]
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(1000000)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));

        // Act
        set1.Union(set2);

        // Assert: should remain as single range [0-1000000]
        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(1000000), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_ConstructorWithMultipleRanges()
    {
        // Arrange: set1 constructed with multiple overlapping ranges, set2 with separate range
        var set1 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(15), new Ip4Address(25))
        });
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

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
        var set = new Ip4RangeSet();

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Constructor_FromSingleRange_CreatesSetWithOneRange()
    {
        var range = new Ip4Range(new Ip4Address(10), new Ip4Address(20));

        var set = new Ip4RangeSet(range);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(range, ranges[0]);
    }

    [Fact]
    public void Constructor_FromSingleSubnet_CreatesSetWithOneRange()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        var set = new Ip4RangeSet(subnet);

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

        var set = new Ip4RangeSet(ranges);

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
        var set = new Ip4RangeSet(subnets);

        // Assert: Should have 2 ranges
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
    }

    [Fact]
    public void Constructor_FromRangeEnumerable_NullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet((IEnumerable<Ip4Range>)null!));
    }

    [Fact]
    public void Constructor_FromSubnetEnumerable_NullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet((IEnumerable<Ip4Subnet>)null!));
    }


    #region Except(Ip4Range) Tests

    [Fact]
    public void Except_Ip4Range_CompletelyOverlapping_RemovesRange()
    {
        // Arrange: [10-30] except [10-30] should result in empty set
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4Range_PartialOverlapAtStart_TruncatesRange()
    {
        // Arrange: [10-30] except [5-20] should result in [21-30]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

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
        var set = new Ip4RangeSet(new[]
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
        var set = new Ip4RangeSet();

        // Act
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4Range_ExceedingRange_RemovesOverlappingPortion()
    {
        // Arrange: [10-20] except [0-100] should result in empty set
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    #endregion

    #region Except(Ip4RangeSet) Tests

    [Fact]
    public void Except_Ip4RangeSet_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Except((Ip4RangeSet)null!));
    }

    [Fact]
    public void Except_Ip4RangeSet_EmptySet_NoChange()
    {
        // Arrange
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var emptySet = new Ip4RangeSet();

        // Act
        set.Except(emptySet);

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4RangeSet_EmptySetExceptingNonEmpty_RemainsEmpty()
    {
        // Arrange
        var set = new Ip4RangeSet();
        var otherSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(otherSet);

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4RangeSet_CompleteOverlap_RemovesAll()
    {
        // Arrange: [10-20] except [10-20] should result in empty set
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var otherSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set.Except(otherSet);

        // Assert
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4RangeSet_MultipleRanges_RemovesOverlapping()
    {
        // Arrange: [10-20], [30-40], [50-60] except [15-35], [55-65]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });
        var otherSet = new Ip4RangeSet(new[]
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
    public void Except_Ip4RangeSet_DisjointRanges_NoChange()
    {
        // Arrange: [10-20], [30-40] except [50-60], [70-80]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });
        var otherSet = new Ip4RangeSet(new[]
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
    public void Except_Ip4RangeSet_PartialOverlaps_RemovesCorrectly()
    {
        // Arrange: [10-30] except [20-25]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var otherSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(20), new Ip4Address(25)));

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


    #region Static Properties Tests

    [Fact]
    public void Empty_ReturnsEmptySet()
    {
        // Act
        var emptySet = Ip4RangeSet.Empty;

        // Assert
        Assert.NotNull(emptySet);
        Assert.Empty(emptySet.ToIp4Ranges());
    }

    [Fact]
    public void All_ReturnsFullIpRange()
    {
        // Act
        var allSet = Ip4RangeSet.All;

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
        var empty1 = Ip4RangeSet.Empty;
        var empty2 = Ip4RangeSet.Empty;

        // Assert: should be different instances
        Assert.NotSame(empty1, empty2);
    }

    [Fact]
    public void All_MultipleCallsReturnDifferentInstances()
    {
        // Act
        var all1 = Ip4RangeSet.All;
        var all2 = Ip4RangeSet.All;

        // Assert: should be different instances
        Assert.NotSame(all1, all2);
    }

    #endregion

    #region Copy Constructor Tests

    [Fact]
    public void Constructor_CopyFromSet_CreatesDeepCopy()
    {
        // Arrange
        var original = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        // Act
        var copy = new Ip4RangeSet(original);

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
        var original = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var copy = new Ip4RangeSet(original);

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
        var original = new Ip4RangeSet();

        // Act
        var copy = new Ip4RangeSet(original);

        // Assert
        Assert.Empty(copy.ToIp4Ranges());
    }

    [Fact]
    public void Constructor_CopyFromNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet((Ip4RangeSet)null!));
    }

    #endregion

    #region ToIp4Subnets Tests

    [Fact]
    public void ToIp4Subnets_SingleRange_ReturnsSubnets()
    {
        // Arrange: 192.168.0.0 - 192.168.0.255 should convert to subnets
        var set = new Ip4RangeSet(new Ip4Range(
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
        var set = new Ip4RangeSet(new[]
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
        var set = new Ip4RangeSet();

        // Act
        var subnets = set.ToIp4Subnets();

        // Assert
        Assert.Empty(subnets);
    }

    [Fact]
    public void ToIp4Subnets_SmallRange_ReturnsCorrectSubnets()
    {
        // Arrange: small range that should produce specific subnets
        var set = new Ip4RangeSet(new Ip4Range(
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
        var set = new Ip4RangeSet(new[]
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
        var set = new Ip4RangeSet(new Ip4Range(
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
        var set = new Ip4RangeSet();

        // Act
        var minimized = set.MinimizeSubnets(10);

        // Assert
        Assert.Empty(minimized.ToIp4Ranges());
    }

    [Fact]
    public void MinimizeSubnets_HighDelta_RemovesAllSmallSubnets()
    {
        // Arrange
        var set = new Ip4RangeSet(new Ip4Range(
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
        var set = new Ip4RangeSet();

        // Act
        var result = set.ToString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToString_SingleRange_ReturnsRangeString()
    {
        // Arrange
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

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
        var set = new Ip4RangeSet(new[]
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
        var set = new Ip4RangeSet(new[]
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
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Union((Ip4Range[])null!));
    }

    [Fact]
    public void Union_WithNullEnumerable_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Union((IEnumerable<Ip4Range>)null!));
    }

    [Fact]
    public void Union_WithNullSubnetEnumerable_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Union((IEnumerable<Ip4Subnet>)null!));
    }

    [Fact]
    public void Operations_AtMinimumIpAddress_WorkCorrectly()
    {
        // Arrange: range starting at 0.0.0.0
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        // Act & Assert: should handle minimum address
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
    }

    [Fact]
    public void Operations_AtMaximumIpAddress_WorkCorrectly()
    {
        // Arrange: range ending at 255.255.255.255
        var set = new Ip4RangeSet(new Ip4Range(
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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        // Act: except entire IP range
        set.Except(Ip4Range.All);

        // Assert: should be empty
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void LargeRangeOperations_PerformCorrectly()
    {
        // Arrange: large ranges for performance testing
        var set = new Ip4RangeSet(new Ip4Range(
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
    public void MultipleOperations_MaintainsSortedOrder()
    {
        // Arrange
        var set = new Ip4RangeSet();

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

    [Fact]
    public void Union_AdjacentRangesAtMaxBoundary_MergesCorrectly()
    {
        // Arrange: ranges adjacent at uint.MaxValue boundary
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue - 5));
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue - 4), new Ip4Address(uint.MaxValue));
        var set = new Ip4RangeSet(r1);

        // Act
        set.Union(r2);

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_RangeStartingAtZero_NoUnderflow()
    {
        // Arrange: set [0, 20], except [0, 10] - tests underflow prevention at 0
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(20)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(10)));

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(11), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_RangeEndingAtMaxIp_HandlesCorrectly()
    {
        // Arrange: union with range ending at uint.MaxValue
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue - 5)));
        var otherSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(uint.MaxValue - 4), new Ip4Address(uint.MaxValue)));

        // Act
        set.Union(otherSet);

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_RangeEndingAtMaxIp_NoOverflow()
    {
        // Arrange: set [uint.MaxValue-10, uint.MaxValue], except [uint.MaxValue-5, uint.MaxValue] - tests overflow prevention at uint.MaxValue
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue)));

        // Act
        set.Except(new Ip4Range(new Ip4Address(uint.MaxValue - 5), new Ip4Address(uint.MaxValue)));

        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 6), ranges[0].LastAddress);
    }

    [Fact]
    public void IntersectableExcept_AtBoundaries_HandlesOverflowCorrectly()
    {
        // Test IntersectableExcept directly via Except operation at boundaries
        // Case 1: Excepting range ending at uint.MaxValue that covers the end
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(uint.MaxValue - 5), new Ip4Address(uint.MaxValue)));
        set1.Except(new Ip4Range(new Ip4Address(uint.MaxValue), new Ip4Address(uint.MaxValue)));
        var ranges1 = set1.ToIp4Ranges();
        Assert.Single(ranges1);
        Assert.Equal(new Ip4Address(uint.MaxValue - 5), ranges1[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 1), ranges1[0].LastAddress);

        // Case 2: Excepting range starting at 0 that covers the start
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(10)));
        set2.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(0)));
        var ranges2 = set2.ToIp4Ranges();
        Assert.Single(ranges2);
        Assert.Equal(new Ip4Address(1), ranges2[0].FirstAddress);
        Assert.Equal(new Ip4Address(10), ranges2[0].LastAddress);
    }

    #endregion

    #region Stress Tests for Large Range Sets

    [Fact]
    public void Constructor_WithLargeNumberOfRanges_HandlesCorrectly()
    {
        // Arrange: Create a large number of ranges (more than MAX_STACK_ALLOC)
        const int rangeCount = 2000;
        var ranges = new List<Ip4Range>();
        for (int i = 0; i < rangeCount; i++)
        {
            ranges.Add(new Ip4Range(new Ip4Address((uint)i * 100), new Ip4Address((uint)i * 100 + 50)));
        }

        // Act: Create set with large number of ranges
        var set = new Ip4RangeSet(ranges);

        // Assert: Should handle large input correctly
        var resultRanges = set.ToIp4Ranges().ToArray();
        Assert.Equal(rangeCount, resultRanges.Length);
        for (int i = 0; i < rangeCount; i++)
        {
            Assert.Equal(new Ip4Address((uint)i * 100), resultRanges[i].FirstAddress);
            Assert.Equal(new Ip4Address((uint)i * 100 + 50), resultRanges[i].LastAddress);
        }
    }

    [Fact]
    public void Union_WithLargeSets_HandlesCorrectly()
    {
        // Arrange: Create two large sets
        const int set1Size = 1500;
        var ranges1 = new List<Ip4Range>();
        for (int i = 0; i < set1Size; i++)
        {
            ranges1.Add(new Ip4Range(new Ip4Address((uint)i * 200), new Ip4Address((uint)i * 200 + 50)));
        }
        var set1 = new Ip4RangeSet(ranges1);

        const int set2Size = 1200;
        var ranges2 = new List<Ip4Range>();
        for (int i = 0; i < set2Size; i++)
        {
            ranges2.Add(new Ip4Range(new Ip4Address((uint)(i + 10000) * 200), new Ip4Address((uint)(i + 10000) * 200 + 50)));
        }
        var set2 = new Ip4RangeSet(ranges2);

        // Act: Union the large sets
        set1.Union(set2);

        // Assert: Should contain all ranges from both sets
        var resultRanges = set1.ToIp4Ranges().ToArray();
        Assert.Equal(set1Size + set2Size, resultRanges.Length);
    }

    [Fact]
    public void Except_WithLargeSets_HandlesCorrectly()
    {
        // Arrange: Create two large sets with some overlap
        const int set1Size = 1000;
        var ranges1 = new List<Ip4Range>();
        for (int i = 0; i < set1Size; i++)
        {
            ranges1.Add(new Ip4Range(new Ip4Address((uint)i * 100), new Ip4Address((uint)i * 100 + 50)));
        }
        var set1 = new Ip4RangeSet(ranges1);

        const int set2Size = 800;
        var ranges2 = new List<Ip4Range>();
        for (int i = 0; i < set2Size; i++)
        {
            // Create overlapping ranges
            ranges2.Add(new Ip4Range(new Ip4Address((uint)i * 100 + 25), new Ip4Address((uint)i * 100 + 75)));
        }
        var set2 = new Ip4RangeSet(ranges2);

        // Act: Except operation
        set1.Except(set2);

        // Assert: Should have performed the operation without throwing
        var resultRanges = set1.ToIp4Ranges().ToArray();
        // The exact count depends on merging, but should be reasonable
        Assert.True(resultRanges.Length > 0);
        Assert.True(resultRanges.Length <= set1Size);
    }

    [Fact]
    public void LargeSetOperations_MaintainCorrectness()
    {
        // Arrange: Create a large set and perform multiple operations
        const int initialSize = 500;
        var ranges = new List<Ip4Range>();
        for (int i = 0; i < initialSize; i++)
        {
            ranges.Add(new Ip4Range(new Ip4Address((uint)i * 1000), new Ip4Address((uint)i * 1000 + 500)));
        }
        var set = new Ip4RangeSet(ranges);

        // Act: Perform multiple operations
        var unionRange = new Ip4Range(new Ip4Address(250000), new Ip4Address(300000));
        set.Union(unionRange);

        var exceptRange = new Ip4Range(new Ip4Address(100000), new Ip4Address(150000));
        set.Except(exceptRange);

        // Assert: Operations completed successfully
        var resultRanges = set.ToIp4Ranges().ToArray();
        Assert.True(resultRanges.Length > 0);

        // Verify the union range is present
        bool unionRangeFound = resultRanges.Any(r =>
            r.FirstAddress <= unionRange.FirstAddress && r.LastAddress >= unionRange.LastAddress);
        Assert.True(unionRangeFound, "Union range should be present in the result");

        // Verify the except range is not present
        bool exceptRangeFound = resultRanges.Any(r =>
            r.FirstAddress <= exceptRange.LastAddress && r.LastAddress >= exceptRange.FirstAddress);
        Assert.False(exceptRangeFound, "Except range should not be present in the result");
    }

    [Fact]
    public void Ip4RangeSet_HandlesInputsExceedingStackAllocLimit_WithoutIssues()
    {
        // Arrange: Create input that exceeds MAX_STACK_ALLOC (1024) to verify heap allocation works
        const int rangeCount = 1500; // Well above 1024
        var ranges = new List<Ip4Range>();
        for (int i = 0; i < rangeCount; i++)
        {
            ranges.Add(new Ip4Range(new Ip4Address((uint)i * 10), new Ip4Address((uint)i * 10 + 5)));
        }

        // Act: Create set - should use heap allocation and work fine
        var set = new Ip4RangeSet(ranges);

        // Assert: Should handle large input correctly
        var resultRanges = set.ToIp4Ranges().ToArray();
        Assert.Equal(rangeCount, resultRanges.Length);

        // Verify ranges are sorted and correct
        for (int i = 0; i < rangeCount; i++)
        {
            Assert.Equal(new Ip4Address((uint)i * 10), resultRanges[i].FirstAddress);
            Assert.Equal(new Ip4Address((uint)i * 10 + 5), resultRanges[i].LastAddress);
        }
    }

    #endregion
}
