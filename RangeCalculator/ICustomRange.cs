namespace RangeCalculator;

public interface ICustomRange<T>
{
    T First { get; }
    T Last { get; }
}
