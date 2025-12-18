namespace routes;

public readonly ref struct ListStackAlloc<T>
    where T : unmanaged
{
    private readonly ReadOnlySpan<T> items;
    private readonly int size;

    public List()
    {
    }

    public List(ReadOnlySpan<T> items)
    {
        this.items = items;
        this.size = this.items.Length;
    }

    public List(int capacity)

    {
        ReadOnlySpan<Ip4Range> temp = stackalloc Ip4Range[_ranges.Length + other._ranges.Length];
        this.items = stackalloc T[capacity];
        this.size = this.items.Length;
    }
}

public readonly ref struct Ip4RangeSetStackAlloc
{
    private readonly ReadOnlySpan<Ip4Range> _ranges;

    public Ip4RangeSetStackAlloc(ReadOnlySpan<Ip4Range> ranges) => _ranges = ranges;

    public Ip4RangeSetStackAlloc(int size)
    {
        Span<Ip4Range> ranges = stackalloc Ip4Range[size];
    }

    public Ip4RangeSetStackAlloc Union(Ip4RangeSetStackAlloc other)
    {
        ReadOnlySpan<Ip4Range> temp = stackalloc Ip4Range[_ranges.Length + other._ranges.Length];

        throw new NotImplementedException();
    }
}