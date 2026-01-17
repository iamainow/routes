using routes.Generic;

namespace routes.Test;

public class ListStackAllocTest
{
    [Fact]
    public void Constructor_WithSpanElements_InitializesCorrectly()
    {
        Span<int> buffer = new int[10];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        Assert.Equal(3, list.Count);
        Assert.Equal(10, list.Capacity);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Constructor_WithReadOnlySpanElements_InitializesCorrectly()
    {
        Span<int> buffer = new int[10];
        ReadOnlySpan<int> elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        Assert.Equal(3, list.Count);
        Assert.Equal(10, list.Capacity);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Constructor_WithListStackAlloc_InitializesCorrectly()
    {
        Span<int> buffer1 = new int[10];
        int[] elements = [1, 2, 3];
        var original = new ListStackAlloc<int>(buffer1);
        original.AddRange(elements);

        Span<int> buffer2 = new int[10];
        var list = new ListStackAlloc<int>(buffer2);
        list.AddRange(original.AsReadOnlySpan());

        Assert.Equal(3, list.Count);
        Assert.Equal(10, list.Capacity);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Constructor_WithCount_InitializesCorrectly()
    {
        Span<int> buffer = new int[5];
        buffer[0] = 10;
        buffer[1] = 20;
        buffer[2] = 30;

        var list = new ListStackAlloc<int>(buffer, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(5, list.Capacity);
        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Indexer_Get_ValidIndex_ReturnsCorrectValue()
    {
        Span<int> buffer = new int[5];
        int[] elements = [10, 20, 30];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Indexer_Get_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        {
            Span<int> buffer = new int[5];
            int[] elements = [10, 20, 30];
            var list = new ListStackAlloc<int>(buffer);
            list.AddRange(elements);
            try
            {
                _ = list[-1];
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
        {
            Span<int> buffer = new int[5];
            int[] elements = [10, 20, 30];
            var list = new ListStackAlloc<int>(buffer);
            list.AddRange(elements);
            try
            {
                _ = list[3];
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }

    [Fact]
    public void Indexer_Get_Range_ReturnsCorrectSpan()
    {
        Span<int> buffer = new int[5];
        int[] elements = [10, 20, 30, 40];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        var range = list[1..3];
        Assert.Equal(2, range.Length);
        Assert.Equal(20, range[0]);
        Assert.Equal(30, range[1]);
    }

    [Fact]
    public void Add_SingleItem_IncreasesCount()
    {
        Span<int> buffer = new int[5];
        var list = new ListStackAlloc<int>(buffer);

        list.Add(100);
        Assert.Equal(1, list.Count);
        Assert.Equal(100, list[0]);
    }

    [Fact]
    public void Add_ExceedsCapacity_ThrowsInvalidOperationException()
    {
        Span<int> buffer = new int[2];
        var list = new ListStackAlloc<int>(buffer);
        list.Add(1);
        list.Add(2);
        try
        {
            list.Add(3);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [Fact]
    public void AddRange_Items_IncreasesCount()
    {
        Span<int> buffer = new int[5];
        var list = new ListStackAlloc<int>(buffer);

        int[] items = [1, 2];
        list.AddRange(items);

        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void AddRange_ExceedsCapacity_ThrowsInvalidOperationException()
    {
        Span<int> buffer = new int[3];
        var list = new ListStackAlloc<int>(buffer);
        list.Add(1);
        int[] items = [2, 3, 4];
        try
        {
            list.AddRange(items);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [Fact]
    public void RemoveLast_SingleItem_DecreasesCount()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        list.RemoveLast();
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void RemoveLast_EmptyList_ThrowsInvalidOperationException()
    {
        Span<int> buffer = new int[5];
        var list = new ListStackAlloc<int>(buffer);
        try
        {
            list.RemoveLast();
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [Fact]
    public void RemoveLast_MultipleItems_DecreasesCount()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3, 4];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        list.RemoveLast(2);
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void RemoveLast_MoreThanCount_ThrowsInvalidOperationException()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);
        try
        {
            list.RemoveLast(3);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [Fact]
    public void RemoveRegion_StartAndCount_RemovesCorrectly()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3, 4];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        list.RemoveRegion(1, 2);
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(4, list[1]);
    }

    [Fact]
    public void RemoveRegion_Range_RemovesCorrectly()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3, 4, 5];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        list.RemoveRegion(1..4);
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(5, list[1]);
    }

    [Fact]
    public void AsSpan_ReturnsCorrectSpan()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        var span = list.AsSpan();
        Assert.Equal(3, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(2, span[1]);
        Assert.Equal(3, span[2]);
    }

    [Fact]
    public void AsReadOnlySpan_ReturnsCorrectReadOnlySpan()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        var span = list.AsReadOnlySpan();
        Assert.Equal(3, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(2, span[1]);
        Assert.Equal(3, span[2]);
    }

    [Fact]
    public void ToArray_ReturnsCorrectArray()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        var array = list.ToArray();
        Assert.Equal(3, array.Length);
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
    }

    [Fact]
    public void Clear_ResetsCountToZero()
    {
        Span<int> buffer = new int[5];
        int[] elements = [1, 2, 3];
        var list = new ListStackAlloc<int>(buffer);
        list.AddRange(elements);

        Assert.Equal(3, list.Count);
        Assert.Equal(5, list.Capacity);

        list.Clear();

        Assert.Equal(0, list.Count);
        Assert.Equal(5, list.Capacity);
    }
}