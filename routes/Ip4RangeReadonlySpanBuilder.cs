namespace routes;

public readonly ref struct Ip4RangeReadonlySpanBuilder
{
    private readonly ReadOnlySpan<Ip4Range> _currentRanges;
    private readonly Span<Ip4Range> _rangeBuffer;
    private readonly Span<OperationData> _operations;
    private readonly int _operationCount;
    private readonly int _rangeOffset;

    public Ip4RangeReadonlySpanBuilder(ReadOnlySpan<Ip4Range> initialRanges, Span<Ip4Range> rangeBuffer, Span<OperationData> operationBuffer)
    {
        _currentRanges = initialRanges;
        _rangeBuffer = rangeBuffer;
        _operations = operationBuffer;
        _operationCount = 0;
        _rangeOffset = 0;
    }

    private Ip4RangeReadonlySpanBuilder(ReadOnlySpan<Ip4Range> currentRanges, Span<Ip4Range> rangeBuffer, Span<OperationData> operations, int operationCount, int rangeOffset)
    {
        _currentRanges = currentRanges;
        _rangeBuffer = rangeBuffer;
        _operations = operations;
        _operationCount = operationCount;
        _rangeOffset = rangeOffset;
    }

    public Ip4RangeReadonlySpanBuilder Union(ReadOnlySpan<Ip4Range> other)
    {
        if (_operationCount >= _operations.Length)
            throw new InvalidOperationException("Operation buffer exceeded. Increase the size of the operation buffer.");
        if (_rangeOffset + other.Length > _rangeBuffer.Length)
            throw new InvalidOperationException("Range buffer exceeded. Increase the size of the range buffer.");
        other.CopyTo(_rangeBuffer[_rangeOffset..]);
        _operations[_operationCount] = new OperationData(OperationType.Union, _rangeOffset, other.Length);
        return new Ip4RangeReadonlySpanBuilder(_currentRanges, _rangeBuffer, _operations, _operationCount + 1, _rangeOffset + other.Length);
    }

    public Ip4RangeReadonlySpanBuilder Except(ReadOnlySpan<Ip4Range> other)
    {
        if (_operationCount >= _operations.Length)
            throw new InvalidOperationException("Operation buffer exceeded. Increase the size of the operation buffer.");
        if (_rangeOffset + other.Length > _rangeBuffer.Length)
            throw new InvalidOperationException("Range buffer exceeded. Increase the size of the range buffer.");
        other.CopyTo(_rangeBuffer[_rangeOffset..]);
        _operations[_operationCount] = new OperationData(OperationType.Except, _rangeOffset, other.Length);
        return new Ip4RangeReadonlySpanBuilder(_currentRanges, _rangeBuffer, _operations, _operationCount + 1, _rangeOffset + other.Length);
    }

    public int CalcTotalBuffer()
    {
        int total = 0;
        int currentLen = _currentRanges.Length;
        for (int i = 0; i < _operationCount; i++)
        {
            var op = _operations[i];
            int needed = currentLen + op.SpanLength;
            total += needed;
            currentLen = needed; // overestimate for next step
        }
        return total;
    }

    public Ip4RangeReadonlySpan Execute(Span<Ip4Range> buffer)
    {
        int required = CalcTotalBuffer();
        if (buffer.Length < required)
            throw new ArgumentException($"Buffer too small. Required: {required}, provided: {buffer.Length}");

        int offset = 0;
        var current = _currentRanges;
        var tempSpan = new Ip4RangeReadonlySpan(current);
        for (int i = 0; i < _operationCount; i++)
        {
            var op = _operations[i];
            var opRanges = _rangeBuffer[op.SpanStart..(op.SpanStart + op.SpanLength)];
            int needed = current.Length + opRanges.Length;
            var subBuffer = buffer[offset..(offset + needed)];
            int count = op.Type switch
            {
                OperationType.Union => tempSpan.Union(opRanges, subBuffer),
                OperationType.Except => tempSpan.Except(opRanges, subBuffer),
                _ => throw new InvalidOperationException("Unknown operation type")
            };
            current = subBuffer[..count];
            tempSpan = new Ip4RangeReadonlySpan(current);
            offset += needed;
        }
        return tempSpan;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    public readonly struct OperationData : IEquatable<OperationData>
    {
        public OperationType Type { get; }
        public int SpanStart { get; }
        public int SpanLength { get; }

        public OperationData(OperationType type, int spanStart, int spanLength)
        {
            Type = type;
            SpanStart = spanStart;
            SpanLength = spanLength;
        }

        public override bool Equals(object? obj) => obj is OperationData other && Equals(other);

        public bool Equals(OperationData other) => Type == other.Type && SpanStart == other.SpanStart && SpanLength == other.SpanLength;

        public override int GetHashCode() => HashCode.Combine(Type, SpanStart, SpanLength);

        public static bool operator ==(OperationData left, OperationData right) => left.Equals(right);

        public static bool operator !=(OperationData left, OperationData right) => !left.Equals(right);
    }

    public enum OperationType
    {
        Union,
        Except
    }
}