namespace routes.Test;

/// <summary>
/// Comprehensive tests for Ip4RangeSet covering all operations including:
/// - Constructors
/// - Union operations (single range, arrays, sets)
/// - Except operations (single range, sets)
/// - Static properties
/// - Conversions (ToIp4Subnets, ToIp4Ranges, MinimizeSubnets)
/// - Edge cases and boundary conditions
/// - Normalization invariants
/// - Complex scenarios
/// </summary>
public class Ip4RangeSetTest
{
    #region Constructor Tests

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
        var subnets = new[]
        {
            Ip4Subnet.Parse("192.168.0.0/24"),
            Ip4Subnet.Parse("192.168.1.0/24"),
            Ip4Subnet.Parse("192.168.3.0/24")
        };

        var set = new Ip4RangeSet(subnets);

        // Adjacent subnets 192.168.0.0/24 and 192.168.1.0/24 should merge
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

    [Fact]
    public void Constructor_WithAdjacentElements_MergesThem()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };

        var set = new Ip4RangeSet(ranges);

        var result = set.ToIp4Ranges();
        Assert.Single(result);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[0].LastAddress);
    }

    [Fact]
    public void Constructor_WithUnsortedRanges_SortsAndNormalizes()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        };

        var set = new Ip4RangeSet(ranges);

        var result = set.ToIp4Ranges().ToArray();
        Assert.Equal(3, result.Length);
        Assert.Equal(new Ip4Address(10), result[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), result[1].FirstAddress);
        Assert.Equal(new Ip4Address(50), result[2].FirstAddress);
    }

    #endregion

    #region Copy Constructor Tests

    [Fact]
    public void Constructor_CopyFromSet_CreatesDeepCopy()
    {
        var original = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        var copy = new Ip4RangeSet(original);

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
        var original = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var copy = new Ip4RangeSet(original);

        copy.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

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
        var original = new Ip4RangeSet();

        var copy = new Ip4RangeSet(original);

        Assert.Empty(copy.ToIp4Ranges());
    }

    [Fact]
    public void Constructor_CopyFromNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ip4RangeSet((Ip4RangeSet)null!));
    }

    #endregion

    #region Union(Ip4Range) Tests

    [Fact]
    public void Union_Overlapping_MergesIntoSingleRange()
    {
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(15), new Ip4Address(25));
        var set = new Ip4RangeSet(r1);

        set.Union(r2);

        Ip4Range[] ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(25), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_Disjoint_KeepsBothRanges()
    {
        var r1 = new Ip4Range(new Ip4Address(10), new Ip4Address(20));
        var r2 = new Ip4Range(new Ip4Address(22), new Ip4Address(30));
        var set = new Ip4RangeSet(r1);

        set.Union(r2);

        Ip4Range[] ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(22), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_Adjacent_MergesIntoSingleRange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_RangeSpanningMultipleGaps_BridgesAllGaps()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(70), new Ip4Address(80))
        });

        set.Union(new Ip4Range(new Ip4Address(15), new Ip4Address(75)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_CompletelyContainedRange_NoChange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(100)));

        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_OnEmptySet_AddsRange()
    {
        var set = new Ip4RangeSet();

        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    #endregion

    #region Union(Ip4RangeSet) Tests

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
        var set1 = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(40), new Ip4Address(50))
        });
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(15), new Ip4Address(35)));

        set1.Union(set2);

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
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(15), new Ip4Address(25)));

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_EmptySet_DoesNothing()
    {
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet();

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_EmptySetUnioningNonEmpty()
    {
        var set1 = new Ip4RangeSet();
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_WithIp4RangeSet_AdjacentRangesBridge()
    {
        var set1 = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var set2 = new Ip4RangeSet(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
        var set3 = new Ip4RangeSet(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));

        set1.Union(set2);
        set1.Union(set3);

        var ranges = set1.ToIp4Ranges().ToArray();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[0].LastAddress);
    }

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

    #endregion

    #region Except(Ip4Range) Tests

    [Fact]
    public void Except_CompletelyOverlapping_RemovesRange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_PartialOverlapAtStart_TruncatesRange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Except(new Ip4Range(new Ip4Address(5), new Ip4Address(20)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(21), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_PartialOverlapAtEnd_TruncatesRange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_MiddleOverlap_SplitsIntoTwoRanges()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(20)));

        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(21), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_Disjoint_NoChange()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Except(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_MultipleRanges_RemovesFromMultiple()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });

        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(14), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(56), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].LastAddress);
    }

    [Fact]
    public void Except_EmptySet_RemainsEmpty()
    {
        var set = new Ip4RangeSet();

        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_SingleAddressFromMiddle_CreatesGap()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(15)));

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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Except(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(11), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_LastAddressOnly_TruncatesEnd()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(20)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(19), ranges[0].LastAddress);
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
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var emptySet = new Ip4RangeSet();

        set.Except(emptySet);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_Ip4RangeSet_CompleteOverlap_RemovesAll()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        var otherSet = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Except(otherSet);

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Except_Ip4RangeSet_MultipleRanges_RemovesOverlapping()
    {
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

        set.Except(otherSet);

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

        set.Except(otherSet);

        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(30), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(40), ranges[1].LastAddress);
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Empty_ReturnsEmptySet()
    {
        var emptySet = Ip4RangeSet.Empty;

        Assert.NotNull(emptySet);
        Assert.Empty(emptySet.ToIp4Ranges());
    }

    [Fact]
    public void All_ReturnsFullIpRange()
    {
        var allSet = Ip4RangeSet.All;

        Assert.NotNull(allSet);
        var ranges = allSet.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Empty_MultipleCallsReturnDifferentInstances()
    {
        var empty1 = Ip4RangeSet.Empty;
        var empty2 = Ip4RangeSet.Empty;

        Assert.NotSame(empty1, empty2);
    }

    [Fact]
    public void All_MultipleCallsReturnDifferentInstances()
    {
        var all1 = Ip4RangeSet.All;
        var all2 = Ip4RangeSet.All;

        Assert.NotSame(all1, all2);
    }

    #endregion

    #region ToIp4Subnets Tests

    [Fact]
    public void ToIp4Subnets_SingleRange_ReturnsSubnets()
    {
        var set = new Ip4RangeSet(new Ip4Range(
            Ip4Address.Parse("192.168.0.0"),
            Ip4Address.Parse("192.168.0.255")));

        var subnets = set.ToIp4Subnets().ToArray();

        Assert.NotEmpty(subnets);
        ulong totalAddresses = (ulong)subnets.Sum(s => (decimal)s.Count);
        Assert.Equal(256UL, totalAddresses);
    }

    [Fact]
    public void ToIp4Subnets_MultipleRanges_ReturnsAllSubnets()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(Ip4Address.Parse("10.0.0.0"), Ip4Address.Parse("10.0.0.255")),
            new Ip4Range(Ip4Address.Parse("192.168.1.0"), Ip4Address.Parse("192.168.1.255"))
        });

        var subnets = set.ToIp4Subnets().ToArray();

        Assert.NotEmpty(subnets);
        ulong totalAddresses = (ulong)subnets.Sum(s => (decimal)s.Count);
        Assert.Equal(512UL, totalAddresses);
    }

    [Fact]
    public void ToIp4Subnets_EmptySet_ReturnsEmptyArray()
    {
        var set = new Ip4RangeSet();

        var subnets = set.ToIp4Subnets().ToArray();

        Assert.Empty(subnets);
    }

    #endregion

    #region MinimizeSubnets Tests

    [Fact]
    public void MinimizeSubnets_FiltersByDelta_ReturnsLargerSubnets()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(Ip4Address.Parse("10.0.0.0"), Ip4Address.Parse("10.0.0.255")),
            new Ip4Range(Ip4Address.Parse("192.168.0.0"), Ip4Address.Parse("192.168.0.7"))
        });

        var minimized = set.MinimizeSubnets(100);

        var subnets = minimized.ToIp4Subnets().ToArray();
        Assert.All(subnets, s => Assert.True(s.Count > 100));
    }

    [Fact]
    public void MinimizeSubnets_DeltaZero_ReturnsAllSubnets()
    {
        var set = new Ip4RangeSet(new Ip4Range(
            Ip4Address.Parse("10.0.0.0"),
            Ip4Address.Parse("10.0.0.255")));

        var minimized = set.MinimizeSubnets(0);

        var originalSubnets = set.ToIp4Subnets();
        var minimizedSubnets = minimized.ToIp4Subnets();
        Assert.Equal(originalSubnets.Length, minimizedSubnets.Length);
    }

    [Fact]
    public void MinimizeSubnets_EmptySet_ReturnsEmptySet()
    {
        var set = new Ip4RangeSet();

        var minimized = set.MinimizeSubnets(10);

        Assert.Empty(minimized.ToIp4Ranges());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_EmptySet_ReturnsEmptyString()
    {
        var set = new Ip4RangeSet();

        var result = set.ToString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToString_SingleRange_ReturnsRangeString()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        var result = set.ToString();

        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToString_MultipleRanges_ReturnsAllRanges()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40))
        });

        var result = set.ToString();

        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    #endregion

    #region Edge Cases - Boundary Conditions

    [Fact]
    public void Operations_AtMinimumIpAddress_WorkCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(100)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
    }

    [Fact]
    public void Operations_AtMaximumIpAddress_WorkCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(
            new Ip4Address(uint.MaxValue - 100),
            new Ip4Address(uint.MaxValue)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_FullIpRange_WorksCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        set.Union(Ip4Range.All);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_FullIpRange_ResultsInEmptySet()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        set.Except(Ip4Range.All);

        Assert.Empty(set.ToIp4Ranges());
    }

    [Fact]
    public void Union_AdjacentRangesAtMaxBoundary_MergesCorrectly()
    {
        var r1 = new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue - 5));
        var r2 = new Ip4Range(new Ip4Address(uint.MaxValue - 4), new Ip4Address(uint.MaxValue));
        var set = new Ip4RangeSet(r1);

        set.Union(r2);

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_RangeStartingAtZero_NoUnderflow()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(20)));

        set.Except(new Ip4Range(new Ip4Address(0), new Ip4Address(10)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(11), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(20), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_RangeEndingAtMaxIp_NoOverflow()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(uint.MaxValue - 10), new Ip4Address(uint.MaxValue)));

        set.Except(new Ip4Range(new Ip4Address(uint.MaxValue - 5), new Ip4Address(uint.MaxValue)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(uint.MaxValue - 10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue - 6), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_AtBoundaries_HandlesOverflowCorrectly()
    {
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

    [Fact]
    public void MultipleOperations_MaintainsSortedOrder()
    {
        var set = new Ip4RangeSet();

        set.Union(new Ip4Range(new Ip4Address(50), new Ip4Address(60)));
        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(30), new Ip4Address(40)));
        set.Except(new Ip4Range(new Ip4Address(35), new Ip4Address(55)));

        var ranges = set.ToIp4Ranges().ToArray();
        for (int i = 0; i < ranges.Length - 1; i++)
        {
            Assert.True(ranges[i].LastAddress < ranges[i + 1].FirstAddress);
        }
    }

    #endregion

    #region Large Range Sets - Stress Tests

    [Fact]
    public void Constructor_WithLargeNumberOfRanges_HandlesCorrectly()
    {
        const int rangeCount = 2000;
        var ranges = new List<Ip4Range>();
        for (int i = 0; i < rangeCount; i++)
        {
            ranges.Add(new Ip4Range(new Ip4Address((uint)i * 100), new Ip4Address((uint)i * 100 + 50)));
        }

        var set = new Ip4RangeSet(ranges);

        var resultRanges = set.ToIp4Ranges().ToArray();
        Assert.Equal(rangeCount, resultRanges.Length);
    }

    [Fact]
    public void Union_WithLargeSets_HandlesCorrectly()
    {
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

        set1.Union(set2);

        var resultRanges = set1.ToIp4Ranges().ToArray();
        Assert.Equal(set1Size + set2Size, resultRanges.Length);
    }

    [Fact]
    public void LargeRangeOperations_PerformCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(
            new Ip4Address(0),
            new Ip4Address(10_000_000)));

        set.Union(new Ip4Range(
            new Ip4Address(5_000_000),
            new Ip4Address(15_000_000)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(15_000_000), ranges[0].LastAddress);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Union_MultipleOverlappingRanges_MergesCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));
        set.Union(new Ip4Range(new Ip4Address(35), new Ip4Address(50)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_ChainOfAdjacentRanges_MergesIntoSingleRange()
    {
        var set = new Ip4RangeSet();

        set.Union(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));
        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));
        set.Union(new Ip4Range(new Ip4Address(31), new Ip4Address(40)));
        set.Union(new Ip4Range(new Ip4Address(41), new Ip4Address(50)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
    }

    [Fact]
    public void Except_MultipleHolesInSingleRange_CreatesMultipleFragments()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(100)));

        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(25)));
        set.Except(new Ip4Range(new Ip4Address(40), new Ip4Address(45)));
        set.Except(new Ip4Range(new Ip4Address(60), new Ip4Address(65)));

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
    public void AlternatingUnionAndExcept_MaintainsCorrectState()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));

        set.Except(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));
        set.Union(new Ip4Range(new Ip4Address(150), new Ip4Address(250)));
        set.Except(new Ip4Range(new Ip4Address(300), new Ip4Address(400)));
        set.Union(new Ip4Range(new Ip4Address(350), new Ip4Address(450)));

        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(99), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(150), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(299), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(350), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(1000), ranges[2].LastAddress);
    }

    [Fact]
    public void Union_TwoSetsWithMultipleRanges_MergesComplexly()
    {
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

        set1.Union(set2);

        var ranges = set1.ToIp4Ranges().ToArray();
        Assert.Equal(3, ranges.Length);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(50), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(60), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(80), ranges[1].LastAddress);
        Assert.Equal(new Ip4Address(90), ranges[2].FirstAddress);
        Assert.Equal(new Ip4Address(100), ranges[2].LastAddress);
    }

    [Fact]
    public void Except_TwoSetsWithMultipleRanges_RemovesComplexly()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(70)),
            new Ip4Range(new Ip4Address(90), new Ip4Address(110))
        });
        var exceptSet = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(20), new Ip4Address(60)),
            new Ip4Range(new Ip4Address(100), new Ip4Address(120))
        });

        set.Except(exceptSet);

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
    public void Except_FullRangeWithPartialRange_CreatesComplement()
    {
        var set = new Ip4RangeSet(Ip4Range.All);

        set.Except(new Ip4Range(new Ip4Address(1000), new Ip4Address(2000)));

        var ranges = set.ToIp4Ranges().ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(999), ranges[0].LastAddress);
        Assert.Equal(new Ip4Address(2001), ranges[1].FirstAddress);
        Assert.Equal(new Ip4Address(uint.MaxValue), ranges[1].LastAddress);
    }

    [Fact]
    public void Union_SingleAddressRanges_HandlesCorrectly()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(10)));

        set.Union(new Ip4Range(new Ip4Address(11), new Ip4Address(11)));

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(10), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(11), ranges[0].LastAddress);
    }

    [Fact]
    public void Union_ManySmallRanges_MergesEfficiently()
    {
        var set = new Ip4RangeSet();

        for (uint i = 0; i < 100; i++)
        {
            set.Union(new Ip4Range(
                new Ip4Address(i * 10),
                new Ip4Address(i * 10 + 9)));
        }

        var ranges = set.ToIp4Ranges();
        Assert.Single(ranges);
        Assert.Equal(new Ip4Address(0), ranges[0].FirstAddress);
        Assert.Equal(new Ip4Address(999), ranges[0].LastAddress);
    }

    #endregion

    #region Normalization Invariant Tests

    /// <summary>
    /// Verifies that the IP range set is properly normalized:
    /// - Ranges are sorted by FirstAddress
    /// - No overlapping ranges exist
    /// - No adjacent ranges exist (LastAddress + 1 != NextFirstAddress)
    /// </summary>
    private static void AssertIsNormalized(Ip4RangeSet set)
    {
        var ranges = set.ToIp4Ranges();

        if (ranges.Length == 0)
            return;

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

    [Fact]
    public void Normalization_ConstructorWithOverlapping_MaintainsNormalized()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(30)),
            new Ip4Range(new Ip4Address(20), new Ip4Address(40))
        };

        var set = new Ip4RangeSet(ranges);

        AssertIsNormalized(set);
        var result = set.ToIp4Ranges();
        Assert.Single(result);
    }

    [Fact]
    public void Normalization_ConstructorWithAdjacent_MaintainsNormalized()
    {
        var ranges = new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(21), new Ip4Address(30))
        };

        var set = new Ip4RangeSet(ranges);

        AssertIsNormalized(set);
        var result = set.ToIp4Ranges();
        Assert.Single(result);
    }

    [Fact]
    public void Normalization_UnionWithOverlapping_MaintainsNormalized()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(30)));

        set.Union(new Ip4Range(new Ip4Address(20), new Ip4Address(40)));

        AssertIsNormalized(set);
    }

    [Fact]
    public void Normalization_UnionWithAdjacent_MaintainsNormalized()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(20)));

        set.Union(new Ip4Range(new Ip4Address(21), new Ip4Address(30)));

        AssertIsNormalized(set);
    }

    [Fact]
    public void Normalization_ExceptMiddle_MaintainsNormalized()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(10), new Ip4Address(50)));

        set.Except(new Ip4Range(new Ip4Address(20), new Ip4Address(30)));

        AssertIsNormalized(set);
    }

    [Fact]
    public void Normalization_ExceptSpanningMultiple_MaintainsNormalized()
    {
        var set = new Ip4RangeSet(new[]
        {
            new Ip4Range(new Ip4Address(10), new Ip4Address(20)),
            new Ip4Range(new Ip4Address(30), new Ip4Address(40)),
            new Ip4Range(new Ip4Address(50), new Ip4Address(60))
        });

        set.Except(new Ip4Range(new Ip4Address(15), new Ip4Address(55)));

        AssertIsNormalized(set);
    }

    [Fact]
    public void Normalization_LargeNumberOfOperations_MaintainsNormalized()
    {
        var ranges = new List<Ip4Range>();
        for (uint i = 0; i < 100; i++)
        {
            ranges.Add(new Ip4Range(
                new Ip4Address(i * 100),
                new Ip4Address(i * 100 + 50)));
        }

        var set = new Ip4RangeSet(ranges);

        AssertIsNormalized(set);
    }

    [Fact]
    public void Normalization_AlternatingOperations_MaintainsNormalized()
    {
        var set = new Ip4RangeSet(new Ip4Range(new Ip4Address(0), new Ip4Address(1000)));

        set.Except(new Ip4Range(new Ip4Address(100), new Ip4Address(200)));
        set.Union(new Ip4Range(new Ip4Address(150), new Ip4Address(250)));
        set.Except(new Ip4Range(new Ip4Address(300), new Ip4Address(400)));
        set.Union(new Ip4Range(new Ip4Address(350), new Ip4Address(450)));

        AssertIsNormalized(set);
    }

    #endregion
}
