using System.Linq;
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
        var result = set.Union(r2);

        // Assert
        Ip4Range[] ranges = result.ToIp4Ranges();
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
        var result = set.Union(r2);

        // Assert
        Ip4Range[] ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
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
        var set = new Ip4RangeSet(r1);

        // Act
        var unionSet = set.Union(r2);

        // Assert: still two ranges before Normalize()
        Ip4Range[] beforeNormalize = unionSet.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, beforeNormalize.Length);

        // Normalize should merge adjacency
        var normalized = unionSet.Normalize();
        Ip4Range[] afterNormalize = normalized.ToIp4Ranges();
        Assert.Single(afterNormalize);
        Assert.Equal(new Ip4Address(10), afterNormalize[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), afterNormalize[0].LastAddress);
    }

    [Fact]
    public void Except_Range_RemovesIntersectionAndKeepsRest()
    {
        // Arrange: initial set has single range [10-30]
        var baseSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        // Remove middle [15-20]
        var toRemove = new Ip4Range(new Ip4Address(15), new Ip4Address(20));

        // Act
        var result = baseSet.Except(toRemove);

        // Assert: should produce two ranges [10-14] and [21-30]
        Ip4Range[] ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(21), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_Set_RemovesAllOverlaps()
    {
        // Arrange
        var start = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));
        var remove = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(0), new Ip4Address(15)),   // overlaps head
            new Ip4Range(new Ip4Address(20), new Ip4Address(25)),  // middle
            new Ip4Range(new Ip4Address(40), new Ip4Address(60))   // overlaps tail
        });

        // Act
        var result = start.Except(remove);

        // Assert: [16-19], [26-39]
        Ip4Range[] ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(16), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(26), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(39), ranges[1].LastAddress);
    }

    [Fact]
    public void Intersect_Range_ProducesIntersectionOnly()
    {
        // Arrange
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        var other = new Ip4Range(new Ip4Address(15), new Ip4Address(40));

        // Act
        var result = set.Intersect(other);

        // Assert: [15-30]
        Ip4Range[] ranges = result.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(15), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void Intersect_Set_AccumulatesIntersections()
    {
        // Arrange
        var left = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });
        var right = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(35)) // intersects both
        });

        // Act
        var result = left.Intersect(right);

        // Assert: resulting set should be [15-20] and [30-35]
        Ip4Range[] ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(15), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(35), ranges[1].LastAddress);
    }

    [Fact]
    public void ExpandSet_MergesWhenGapLessOrEqualDelta()
    {
        // Arrange: three ranges with small gaps between them
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(22), new Ip4Address(25)), // gap 1
            new Ip4Range(new Ip4Address(27), new Ip4Address(30))  // gap 1
        };
        var set = new Ip4RangeSet(ranges);

        // Act: delta=1 should merge all into [10-30]
        var expanded = Ip4RangeSet.ExpandSet(set, 1, out bool changed);

        // Assert
        Assert.True(changed);
        Ip4Range[] result = expanded.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void ShrinkSet_RemovesRangesSmallerOrEqualDelta()
    {
        // Arrange: include a tiny range [50-50] size=1 and a normal one [10-20]
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(50))
        };
        var set = new Ip4RangeSet(ranges);

        // Act: delta=1 will remove the single-address range
        var shrunk = Ip4RangeSet.ShrinkSet(set, 1, out bool changed);

        // Assert
        Assert.True(changed);
        Ip4Range[] result = shrunk.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), result[0].LastAddress);
    }

    [Fact]
    public void Simplify_PerformsExpandOrShrinkWithinDeltaBudget()
    {
        // Arrange: create two close ranges that can be expanded with delta
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(12)), // size=3
            new Ip4Range(new Ip4Address(15), new Ip4Address(18))  // size=4, gap=2
        };
        var set = new Ip4RangeSet(ranges);

        // Act: delta=2 allows expanding gap=2 into a single [10-18]
        var simplified = set.Simplify(2);

        // Assert
        Ip4Range[] result = simplified.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(18), result[0].LastAddress);
    }

    [Fact]
    public void MinimizeSubnets_FiltersOutSmallSubnets()
    {
        // Arrange: range spanning 0.0.0.0-0.0.0.7 -> decomposes to /29 (size 8)
        var range = new Ip4Range(Ip4Address.Parse("0.0.0.0"), Ip4Address.Parse("0.0.0.7"));
        var set = new Ip4RangeSet(range);

        // Act: delta=8 will filter out subnets where Count <= 8; since /29 Count=8, Count <= delta means removed
        var minimized = set.MinimizeSubnets(8);

        // Assert: result should be empty
        Ip4Subnet[] subnets = minimized.ToIp4Subnets();
        Assert.Empty(subnets);
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
        var set = new Ip4RangeSet(subnets);

        // Assert: Should have 3 ranges since they don't overlap
        var ranges = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(3, ranges.Length);
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

        var result = set1.Union(set2);

        var ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
    }

    [Fact]
    public void Except_WithIp4RangeSet_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Except((Ip4RangeSet)null!));
    }

    [Fact]
    public void Intersect_WithIp4RangeSet_NullSet_ThrowsArgumentNullException()
    {
        var set = new Ip4RangeSet();

        Assert.Throws<ArgumentNullException>(() => set.Intersect((Ip4RangeSet)null!));
    }

    [Fact]
    public void Intersect_NoOverlap_ReturnsEmptySet()
    {
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        var result = set1.Intersect(set2);

        Assert.Empty(result.ToIp4Ranges());
    }

    [Fact]
    public void Normalize_MergesAdjacentRanges()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };
        var set = new Ip4RangeSet(ranges);

        var normalized = set.Normalize();

        var result = normalized.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void ExpandSet_NullSet_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ip4RangeSet.ExpandSet(null!, 1, out _));
    }

    [Fact]
    public void ExpandSet_NoChangesNeeded_ReturnsFalse()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        var result = Ip4RangeSet.ExpandSet(set, 0, out bool changed);

        Assert.False(changed);
    }

    [Fact]
    public void ShrinkSet_NullSet_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ip4RangeSet.ShrinkSet(null!, 1, out _));
    }

    [Fact]
    public void ShrinkSet_NoChangesNeeded_ReturnsFalse()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(100)));

        var result = Ip4RangeSet.ShrinkSet(set, 1, out bool changed);

        Assert.False(changed);
    }

    [Fact]
    public void StaticFields_HaveCorrectValues()
    {
        Assert.Empty(Ip4RangeSet.Empty.ToIp4Ranges());
        
        var allRanges = Ip4RangeSet.All.ToIp4Ranges();
        Assert.Single(allRanges);
        Assert.Equal(new Ip4Address(0), allRanges[0].FirstAddress);
        Assert.Equal(new Ip4Address(0xFFFFFFFF), allRanges[0].LastAddress);
    }

    [Fact]
    public void ToIp4Subnets_ConvertsRangesToSubnets()
    {
        var range = new Ip4Range(Ip4Address.Parse("192.168.0.0"), Ip4Address.Parse("192.168.0.255"));
        var set = new Ip4RangeSet(range);

        var subnets = set.ToIp4Subnets();

        Assert.NotEmpty(subnets);
        // Verify subnets cover the original range
        var reconstructed = new Ip4RangeSet(subnets);
        var reconstructedRanges = reconstructed.ToIp4Ranges();
        Assert.Single(reconstructedRanges);
        Assert.Equal(range.FirstAddress, reconstructedRanges[0].FirstAddress);
        Assert.Equal(range.LastAddress, reconstructedRanges[0].LastAddress);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };
        var set = new Ip4RangeSet(ranges);

        var result = set.ToString();

        Assert.Contains("0.0.0.10-0.0.0.20", result);
        Assert.Contains("0.0.0.30-0.0.0.40", result);
    }

    [Fact]
    public void Union_MultipleOverlappingRanges_MergesAll()
    {
        var set = new Ip4RangeSet();
        set = set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set = set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(25)));
        set = set.Union(new Ip4Range(new Ip4Address(22), new Ip4Address(30)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_CompletelyRemovesRange_ReturnsEmpty()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var toRemove = new Ip4Range(new Ip4Address(5), new Ip4Address(25));

        var result = set.Except(toRemove);

        Assert.Empty(result.ToIp4Ranges());
    }

    [Fact]
    public void Simplify_WithZeroDelta_DoesNotModify()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };
        var set = new Ip4RangeSet(ranges);

        var simplified = set.Simplify(0);

        var result = simplified.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void MinimizeSubnets_WithZeroDelta_KeepsAllSubnets()
    {
        var range = new Ip4Range(Ip4Address.Parse("192.168.0.0"), Ip4Address.Parse("192.168.1.255"));
        var set = new Ip4RangeSet(range);

        var minimized = set.MinimizeSubnets(0);

        var subnets = minimized.ToIp4Subnets();
        Assert.NotEmpty(subnets);
    }

    [Fact]
    public void Union_WithSubnet_AddsSubnetToSet()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");

        var result = set.Union(subnet);

        var ranges = result.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        Assert.Equal(2, ranges.Length);
    }

    [Fact]
    public void Except_WithSubnet_RemovesSubnetFromSet()
    {
        var subnet = Ip4Subnet.Parse("192.168.1.0/24");
        var set = new Ip4RangeSet(subnet);
        var toRemove = Ip4Subnet.Parse("192.168.1.128/25");

        var result = set.Except(toRemove);

        var ranges = result.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(Ip4Address.Parse("192.168.1.0"), ranges[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse("192.168.1.127"), ranges[0].LastAddress);
    }

    [Fact]
    public void Intersect_WithSubnet_ReturnsIntersection()
    {
        var range = new Ip4Range(Ip4Address.Parse("192.168.1.0"), Ip4Address.Parse("192.168.1.255"));
        var set = new Ip4RangeSet(range);
        var subnet = Ip4Subnet.Parse("192.168.1.128/25");

        var result = set.Intersect(subnet);

        var ranges = result.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(Ip4Address.Parse("192.168.1.128"), ranges[0].FirstAddress);
        Assert.Equal(Ip4Address.Parse("192.168.1.255"), ranges[0].LastAddress);
    }

    [Fact]
    public void ComplexScenario_MultipleOperations_WorksCorrectly()
    {
        // Start with a base set
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));
        
        // Add another range
        set = set.Union(new Ip4Range(new Ip4Address(200), new Ip4Address(300)));
        
        // Remove a middle section
        set = set.Except(new Ip4Range(new Ip4Address(40), new Ip4Address(60)));
        
        // Intersect with a larger range
        set = set.Intersect(new Ip4Range(new Ip4Address(0), new Ip4Address(250)));

        var ranges = set.ToIp4Ranges().OrderBy(x => x.FirstAddress).ToArray();
        
        // Should have: [0-39], [61-100], [200-250]
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(39), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(61), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(200), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(250), ranges[2].LastAddress);
    }
}