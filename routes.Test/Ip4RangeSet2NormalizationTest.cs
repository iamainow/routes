namespace routes.Test;

/// <summary>
/// Comprehensive tests to verify that all public methods in Ip4RangeSet2 maintain normalized IP ranges.
/// Normalized means: sorted by FirstAddress, no overlapping ranges, no adjacent ranges (they should be merged).
/// </summary>
public class Ip4RangeSet2NormalizationTest
{
    #region Helper Methods

    /// <summary>
    /// Verifies that the IP range set is properly normalized:
    /// - Ranges are sorted by FirstAddress
    /// - No overlapping ranges exist
    /// - No adjacent ranges exist (LastAddress + 1 != NextFirstAddress)
    /// </summary>
    private static void AssertIsNormalized(Ip4RangeSet2 set)
    {
        var ranges = set.ToIp4Ranges();

        // Empty set is normalized
        if (ranges.Length == 0)
        {
            return;
        }

        // Check sorted order and no overlaps/adjacency
        for (int i = 1; i < ranges.Length; i++)
        {
            var prev = ranges[i - 1];
            var curr = ranges[i];

            // Verify sorted order
            Assert.True(prev.FirstAddress < curr.FirstAddress,
                $"Ranges not sorted: range[{i - 1}].FirstAddress ({prev.FirstAddress}) >= range[{i}].FirstAddress ({curr.FirstAddress})");

            // Verify no overlap
            Assert.True(prev.LastAddress < curr.FirstAddress,
                $"Ranges overlap: range[{i - 1}].LastAddress ({prev.LastAddress}) >= range[{i}].FirstAddress ({curr.FirstAddress})");

            // Verify no adjacency (should be merged)
            Assert.True(prev.LastAddress.ToUInt32() + 1 < curr.FirstAddress.ToUInt32(),
                $"Adjacent ranges not merged: range[{i - 1}].LastAddress ({prev.LastAddress}) + 1 == range[{i}].FirstAddress ({curr.FirstAddress})");
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_Empty_MaintainsNormalizedState()
    {
        // Act
        var set = new Ip4RangeSet2();

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_SingleRange_MaintainsNormalizedState()
    {
        // Arrange
        var range = new Ip4Range(new Ip4Address(100), new Ip4Address(200));

        // Act
        var set = new Ip4RangeSet2(range);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_SingleSubnet_MaintainsNormalizedState()
    {
        // Arrange
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        // Act
        var set = new Ip4RangeSet2(subnet);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_ArrayWithOverlappingRanges_MaintainsNormalizedState()
    {
        // Arrange: overlapping ranges [10-30] and [20-40]
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(20), new Ip4Address(40))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
        var result = set.ToIp4Ranges();
        Assert.Single(result); // Should be merged
    }

    [Fact]
    public void Constructor_ArrayWithAdjacentRanges_MaintainsNormalizedState()
    {
        // Arrange: adjacent ranges [10-20] and [21-30]
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
        var result = set.ToIp4Ranges();
        Assert.Single(result); // Should be merged
    }

    [Fact]
    public void Constructor_ArrayWithDisjointRanges_MaintainsNormalizedState()
    {
        // Arrange: disjoint ranges [10-20], [30-40], [50-60]
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_ArrayWithUnsortedRanges_MaintainsNormalizedState()
    {
        // Arrange: unsorted ranges
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_EnumerableWithOverlappingRanges_MaintainsNormalizedState()
    {
        // Arrange: overlapping ranges
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(25), new Ip4Address(45)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(60))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_EnumerableWithAdjacentRanges_MaintainsNormalizedState()
    {
        // Arrange: adjacent ranges
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(31), new Ip4Address(40))
        };

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_SubnetEnumerableWithOverlapping_MaintainsNormalizedState()
    {
        // Arrange: overlapping subnets
        IEnumerable<Ip4Subnet> subnets = new List<Ip4Subnet>
        {
            Ip4Subnet.Parse("192.168.0.0/24"),
            Ip4Subnet.Parse("192.168.1.0/24"),
            Ip4Subnet.Parse("192.168.2.0/24")
        };

        // Act
        var set = new Ip4RangeSet2(subnets);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Constructor_CopyConstructor_MaintainsNormalizedState()
    {
        // Arrange: create a normalized set
        var original = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        // Act
        var copy = new Ip4RangeSet2(original);

        // Assert
        AssertIsNormalized(copy);
    }

    [Fact]
    public void Constructor_CopyFromSetWithSingleRange_MaintainsNormalizedState()
    {
        // Arrange
        var original = new Ip4RangeSet2(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));

        // Act
        var copy = new Ip4RangeSet2(original);

        // Assert
        AssertIsNormalized(copy);
    }

    [Fact]
    public void Constructor_CopyFromEmptySet_MaintainsNormalizedState()
    {
        // Arrange
        var original = new Ip4RangeSet2();

        // Act
        var copy = new Ip4RangeSet2(original);

        // Assert
        AssertIsNormalized(copy);
    }

    #endregion

    #region Union(Ip4Range) Tests

    [Fact]
    public void UnionRange_WithOverlappingRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act: union with overlapping range
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_WithAdjacentRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act: union with adjacent range
        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_WithDisjointRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act: union with disjoint range
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_WithRangeBefore_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));

        // Act: union with range before existing
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_WithRangeAfter_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act: union with range after existing
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_SpanningMultipleRanges_MaintainsNormalizedState()
    {
        // Arrange: multiple disjoint ranges
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });

        // Act: union with range spanning all
        set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionRange_OnEmptySet_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        AssertIsNormalized(set);
    }

    #endregion

    #region Union(Ip4RangeSet2) Tests

    [Fact]
    public void UnionSet_WithOverlappingSet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void UnionSet_WithAdjacentSet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void UnionSet_WithDisjointSet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void UnionSet_WithMultipleRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(45), new Ip4Address(60))
        });

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void UnionSet_WithEmptySet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2();

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void UnionSet_EmptySetWithNonEmpty_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2();
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Union(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    #endregion

    #region Union(Ip4Range[]) Tests

    [Fact]
    public void UnionArray_WithOverlappingRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(20), new Ip4Address(30))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionArray_WithAdjacentRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(21), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(31), new Ip4Address(40))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionArray_WithDisjointRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionArray_WithUnsortedRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2();
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionArray_WithEmptyArray_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var ranges = Array.Empty<Ip4Range>();

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    #endregion

    #region Union(IEnumerable<Ip4Range>) Tests

    [Fact]
    public void UnionEnumerable_WithOverlappingRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(25)),
            new Ip4Range(new Ip4Address(20), new Ip4Address(30))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionEnumerable_WithAdjacentRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>
        {
            new Ip4Range(new Ip4Address(21), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(31), new Ip4Address(40))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionEnumerable_WithDisjointRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>
        {
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        };

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionEnumerable_WithEmptyEnumerable_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        IEnumerable<Ip4Range> ranges = new List<Ip4Range>();

        // Act
        set.Union(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    #endregion

    #region Union(IEnumerable<Ip4Subnet>) Tests

    [Fact]
    public void UnionSubnets_WithOverlappingSubnets_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(Ip4Subnet.Parse("192.168.0.0/24"));
        IEnumerable<Ip4Subnet> subnets = new List<Ip4Subnet>
        {
            Ip4Subnet.Parse("192.168.0.128/25"),
            Ip4Subnet.Parse("192.168.1.0/24")
        };

        // Act
        set.Union(subnets);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionSubnets_WithAdjacentSubnets_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(Ip4Subnet.Parse("192.168.0.0/24"));
        IEnumerable<Ip4Subnet> subnets = new List<Ip4Subnet>
        {
            Ip4Subnet.Parse("192.168.1.0/24"),
            Ip4Subnet.Parse("192.168.2.0/24")
        };

        // Act
        set.Union(subnets);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionSubnets_WithDisjointSubnets_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(Ip4Subnet.Parse("192.168.0.0/24"));
        IEnumerable<Ip4Subnet> subnets = new List<Ip4Subnet>
        {
            Ip4Subnet.Parse("10.0.0.0/24"),
            Ip4Subnet.Parse("172.16.0.0/24")
        };

        // Act
        set.Union(subnets);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void UnionSubnets_WithEmptyEnumerable_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(Ip4Subnet.Parse("192.168.0.0/24"));
        IEnumerable<Ip4Subnet> subnets = new List<Ip4Subnet>();

        // Act
        set.Union(subnets);

        // Assert
        AssertIsNormalized(set);
    }

    #endregion

    #region Except(Ip4Range) Tests

    [Fact]
    public void ExceptRange_WithOverlappingRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        // Act: except overlapping range
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void ExceptRange_WithMiddleRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));

        // Act: except middle portion, creating two ranges
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void ExceptRange_WithDisjointRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act: except disjoint range
        set.Except(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void ExceptRange_SpanningMultipleRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });

        // Act: except range spanning multiple ranges
        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void ExceptRange_CompletelyRemovingRange_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act: except entire range
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void ExceptRange_OnEmptySet_MaintainsNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Assert
        AssertIsNormalized(set);
    }

    #endregion

    #region Except(Ip4RangeSet2) Tests

    [Fact]
    public void ExceptSet_WithOverlappingSet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void ExceptSet_WithDisjointSet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void ExceptSet_WithMultipleRanges_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });
        var set2 = new Ip4RangeSet2(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)),
            new Ip4Range(new Ip4Address(55), new Ip4Address(65))
        });

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void ExceptSet_WithEmptySet_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2();

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void ExceptSet_EmptySetExceptingNonEmpty_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2();
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    [Fact]
    public void ExceptSet_CompleteOverlap_MaintainsNormalizedState()
    {
        // Arrange
        var set1 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet2(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        // Act
        set1.Except(set2);

        // Assert
        AssertIsNormalized(set1);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void Operations_AtMinimumAddress_MaintainNormalizedState()
    {
        // Arrange: range starting at 0.0.0.0
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        // Act: union with adjacent range
        set.Union(new Ip4Range(new Ip4Address(101), new Ip4Address(200)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void Operations_AtMaximumAddress_MaintainNormalizedState()
    {
        // Arrange: range ending at 255.255.255.255
        var set = new Ip4RangeSet2(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue)));

        // Act: union with adjacent range
        set.Union(new Ip4Range(
            new Ip4Address(uint.MaxValue - 200),
            new Ip4Address(uint.MaxValue - 101)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void MultipleOperations_MaintainNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act: perform multiple operations
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));
        set.Except(new Ip4Range(new Ip4Address(35), new Ip4Address(55)));

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void LargeNumberOfRanges_MaintainNormalizedState()
    {
        // Arrange: create many ranges
        var ranges = new List<Ip4Range>();
        for (uint i = 0; i < 100; i++)
        {
            ranges.Add(new Ip4Range(
                new Ip4Address(i * 100),
                new Ip4Address(i * 100 + 50)));
        }

        // Act
        var set = new Ip4RangeSet2(ranges);

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void SequentialOperations_MaintainNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2();

        // Act: add ranges sequentially
        for (uint i = 0; i < 10; i++)
        {
            set.Union(new Ip4Range(
                new Ip4Address(i * 20),
                new Ip4Address(i * 20 + 10)));
        }

        // Assert
        AssertIsNormalized(set);
    }

    [Fact]
    public void AlternatingUnionAndExcept_MaintainNormalizedState()
    {
        // Arrange
        var set = new Ip4RangeSet2(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));

        // Act: alternate union and except operations
        set.Except(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));
        set.Union(new Ip4Range(new Ip4Address(150), new Ip4Address(250)));
        set.Except(new Ip4Range(new Ip4Address(300), new Ip4Address(400)));
        set.Union(new Ip4Range(new Ip4Address(350), new Ip4Address(450)));

        // Assert
        AssertIsNormalized(set);
    }

    #endregion
}