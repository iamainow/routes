namespace routes.Test;

/// <summary>
/// Comprehensive complex test cases for Ip4RangeSet Union and Except methods.
/// These tests focus on edge cases, boundary conditions, and complex scenarios
/// to ensure the rewritten logic handles all possible situations correctly.
/// </summary>
public class Ip4RangeSetComplexTest
{
    #region Complex Union Tests

    [Fact]
    public void Union_MultipleOverlappingRanges_MergesCorrectly()
    {
        // Arrange: Create a set with multiple overlapping ranges
        // [10-30], [20-40], [35-50] should merge to [10-50]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        
        // Act
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));
        set.Union(new Ip4Range(new Ip4Address(35), new Ip4Address(50)));
        
        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_ChainOfAdjacentRanges_MergesIntoSingleRange()
    {
        // Arrange: Create adjacent ranges that should merge
        // [10-20], [21-30], [31-40], [41-50] should become [10-50]
        var set = new Ip4RangeSet();
        
        // Act
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
        set.Union(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));
        set.Union(new Ip4Range(new Ip4Address(41), new Ip4Address(50)));
        
        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_InterleavedRanges_MergesCorrectly()
    {
        // Arrange: Interleaved ranges [10-20], [30-40], [15-35], [50-60]
        // Should result in [10-40], [50-60]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });
        
        // Act
        set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(35)));
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        
        // Assert
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(50), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_RangeSpanningMultipleGaps_BridgesAllGaps()
    {
        // Arrange: Multiple disjoint ranges with a spanning range
        // [10-20], [30-40], [50-60], [70-80] + [15-75] should become [10-80]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });
        
        // Act: Add range that spans multiple gaps
        set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(75)));
        
        // Assert
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_SingleAddressRanges_HandlesCorrectly()
    {
        // Arrange: Single address ranges (where FirstAddress == LastAddress)
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));
        
        // Act: Add adjacent single address
        set.Union(new Ip4Range(new Ip4Address(11), new Ip4Address(11)));
        
        // Assert: Should merge to [10-11]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(11), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_BoundaryAtZero_HandlesMinimumAddress()
    {
        // Arrange: Range starting at 0.0.0.0
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));
        
        // Act: Union with adjacent range
        set.Union(new Ip4Range(new Ip4Address(101), new Ip4Address(200)));
        
        // Assert: Should merge to [0-200]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(200), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_BoundaryAtMaxValue_HandlesMaximumAddress()
    {
        // Arrange: Range ending at 255.255.255.255
        var set = new Ip4RangeSet(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue)));
        
        // Act: Union with adjacent range
        set.Union(new Ip4Range(
            new Ip4Address(uint.MaxValue - 200),
            new Ip4Address(uint.MaxValue - 101)));
        
        // Assert: Should merge to single range ending at max
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 200), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_CompletelyContainedRanges_AbsorbsSmaller()
    {
        // Arrange: Large range [10-100]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(100)));
        
        // Act: Union with completely contained ranges
        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));
        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        set.Union(new Ip4Range(new Ip4Address(80), new Ip4Address(90)));
        
        // Assert: Should remain [10-100]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_TwoSetsWithMultipleRanges_MergesComplexly()
    {
        // Arrange: Two sets with multiple ranges each
        // Set1: [10-20], [40-50], [70-80]
        // Set2: [15-45], [60-75], [90-100]
        var set1 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });
        var set2 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(15), new Ip4Address(45)),
            new Ip4Range(new Ip4Address(60), new Ip4Address(75)),
            new Ip4Range(new Ip4Address(90), new Ip4Address(100))
        });
        
        // Act
        set1.Union(set2);
        
        // Assert: Should result in [10-50], [60-80], [90-100]
        var ranges = set1.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(90), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[2].LastAddress);
    }

    #endregion

    #region Complex Except Tests

    [Fact]
    public void Except_MultipleHolesInSingleRange_CreatesMultipleFragments()
    {
        // Arrange: Large range [10-100]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(100)));
        
        // Act: Punch multiple holes
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(25)));
        set.Except(new Ip4Range(new Ip4Address(40), new Ip4Address(45)));
        set.Except(new Ip4Range(new Ip4Address(60), new Ip4Address(65)));
        
        // Assert: Should create 4 fragments
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(4, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(26), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(39), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(46), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(59), ranges[2].LastAddress);
        Assert.Equal(new Ip4Address(66), ranges[3].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[3].LastAddress);
    }

    [Fact]
    public void Except_RangeSpanningMultipleRanges_RemovesFromAll()
    {
        // Arrange: Multiple disjoint ranges [10-20], [30-40], [50-60], [70-80]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });
        
        // Act: Except range spanning from second to third range
        set.Except(new Ip4Range(new Ip4Address(35), new Ip4Address(55)));
        
        // Assert: Should result in [10-20], [30-34], [56-60], [70-80]
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(4, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(34), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(56), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(60), ranges[2].LastAddress);
        Assert.Equal(new Ip4Address(70), ranges[3].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[3].LastAddress);
    }

    [Fact]
    public void Except_ExactBoundaryMatches_RemovesCompletely()
    {
        // Arrange: Range [10-20]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Act: Except with exact same boundaries
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Assert: Should be empty
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_SingleAddressFromRange_CreatesGap()
    {
        // Arrange: Range [10-20]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Act: Except single address in the middle
        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(15)));
        
        // Assert: Should split into [10-14] and [16-20]
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(16), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_FirstAddressOnly_TruncatesStart()
    {
        // Arrange: Range [10-20]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Act: Except only the first address
        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));
        
        // Assert: Should result in [11-20]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(11), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_LastAddressOnly_TruncatesEnd()
    {
        // Arrange: Range [10-20]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Act: Except only the last address
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(20)));
        
        // Assert: Should result in [10-19]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_BoundaryAtZero_HandlesMinimumAddress()
    {
        // Arrange: Range starting at 0.0.0.0
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));
        
        // Act: Except first 50 addresses
        set.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(49)));
        
        // Assert: Should result in [50-100]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(50), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_BoundaryAtMaxValue_HandlesMaximumAddress()
    {
        // Arrange: Range ending at 255.255.255.255
        var set = new Ip4RangeSet(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue)));
        
        // Act: Except last 50 addresses
        set.Except(new Ip4Range(
            new Ip4Address(uint.MaxValue - 49),
            new Ip4Address(uint.MaxValue)));
        
        // Assert: Should result in range ending at MaxValue - 50
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 100), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 50), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_TwoSetsWithMultipleRanges_RemovesComplexly()
    {
        // Arrange: Set with multiple ranges [10-30], [50-70], [90-110]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(70)),
            new Ip4Range(new Ip4Address(90), new Ip4Address(110))
        });
        
        // Except set with overlapping ranges [20-60], [100-120]
        var exceptSet = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(20), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(100), new Ip4Address(120))
        });
        
        // Act
        set.Except(exceptSet);
        
        // Assert: Should result in [10-19], [61-70], [90-99]
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(61), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(70), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(90), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(99), ranges[2].LastAddress);
    }

    [Fact]
    public void Except_AlternatingOperations_MaintainsCorrectState()
    {
        // Arrange: Start with large range
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));
        
        // Act: Alternate except and union operations
        set.Except(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));  // Creates hole
        set.Union(new Ip4Range(new Ip4Address(150), new Ip4Address(250)));  // Partially fills hole
        set.Except(new Ip4Range(new Ip4Address(300), new Ip4Address(400)));  // Creates another hole
        set.Union(new Ip4Range(new Ip4Address(350), new Ip4Address(450)));  // Partially fills hole
        
        // Assert: Verify final state
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        // [0-99], [150-299], [350-1000]
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(99), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(150), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(299), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(350), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(1000), ranges[2].LastAddress);
    }

    #endregion

    #region Edge Cases - Empty and Full Sets

    [Fact]
    public void Union_EmptySetWithEmptySet_RemainsEmpty()
    {
        // Arrange
        var set1 = new Ip4RangeSet();
        var set2 = new Ip4RangeSet();
        
        // Act
        set1.Union(set2);
        
        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Except_EmptySetWithEmptySet_RemainsEmpty()
    {
        // Arrange
        var set1 = new Ip4RangeSet();
        var set2 = new Ip4RangeSet();
        
        // Act
        set1.Except(set2);
        
        // Assert
        Assert.Empty(set1.ToIp4Ranges());
    }

    [Fact]
    public void Union_FullRangeWithAnyRange_RemainsFullRange()
    {
        // Arrange: Full IP range
        var set = new Ip4RangeSet(Ip4Range.All);
        
        // Act: Union with any range
        set.Union(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));
        
        // Assert: Should remain full range
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_FullRangeWithFullRange_ResultsInEmpty()
    {
        // Arrange: Full IP range
        var set = new Ip4RangeSet(Ip4Range.All);
        
        // Act: Except full range
        set.Except(Ip4Range.All);
        
        // Assert: Should be empty
        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_FullRangeWithPartialRange_CreatesComplement()
    {
        // Arrange: Full IP range
        var set = new Ip4RangeSet(Ip4Range.All);
        
        // Act: Except middle portion
        set.Except(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));
        
        // Assert: Should create two ranges
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(999), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(2001), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[1].LastAddress);
    }

    #endregion

    #region Edge Cases - Adjacent Boundaries

    [Fact]
    public void Union_AdjacentRangesWithGapOfOne_RemainsDisjoint()
    {
        // Arrange: Ranges with gap of 1 [10-20], [22-30]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        
        // Act
        set.Union(new Ip4Range(new Ip4Address(22), new Ip4Address(30)));
        
        // Assert: Should remain as two ranges (not adjacent)
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(22), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_OverlappingWithExistingRanges_MergesCorrectly()
    {
        // Arrange: Two ranges with gap [10-20], [30-40]
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });
        
        // Act: Add overlapping range [15-35] that bridges the gap
        set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(35)));
        
        // Assert: Should merge to [10-40]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_CreatingAdjacentRanges_DoesNotMerge()
    {
        // Arrange: Range [10-30]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));
        
        // Act: Except middle address [20-20]
        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(20)));
        
        // Assert: Should create [10-19] and [21-30] (adjacent but not merged)
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(21), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    #endregion

    #region Stress Tests - Large Operations

    [Fact]
    public void Union_ManySmallRanges_MergesEfficiently()
    {
        // Arrange: Create 100 small adjacent ranges
        var set = new Ip4RangeSet();
        
        // Act: Add 100 adjacent ranges
        for (uint i = 0; i < 100; i++)
        {
            set.Union(new Ip4Range(
                new Ip4Address(i * 10),
                new Ip4Address(i * 10 + 9)));
        }
        
        // Assert: Should merge into single range [0-999]
        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(999), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_ManySmallHoles_CreatesFragmentation()
    {
        // Arrange: Large range [0-1000]
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));
        
        // Act: Punch 9 small holes (avoiding boundaries)
        for (uint i = 1; i <= 9; i++)
        {
            set.Except(new Ip4Range(
                new Ip4Address(i * 100),
                new Ip4Address(i * 100)));
        }
        
        // Assert: Should create 10 fragments
        // [0-99], [101-199], [201-299], ..., [801-899], [901-1000]
        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(10, ranges.Length);
        
        // Verify first and last fragments
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(99), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(901), ranges[9].FirstAddress);
        Assert.Equal(new Ip4Address(1000), ranges[9].LastAddress);
    }

    [Fact]
    public void Union_LargeNumberOfDisjointRanges_MaintainsAll()
    {
        // Arrange: Create set with many disjoint ranges
        var ranges = new List<Ip4Range>();
        for (uint i = 0; i < 50; i++)
        {
            ranges.Add(new Ip4Range(
                new Ip4Address(i * 100),
                new Ip4Address(i * 100 + 10)));
        }
        
        // Act
        var set = new Ip4RangeSet(ranges);
        
        // Assert: Should maintain all 50 ranges
        var result = set.ToIp4Ranges();
        Assert.Equal(50, result.Length);
    }

    #endregion
}