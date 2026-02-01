namespace routes.Generic;

internal sealed class CustomRangeComparer<T> : IComparer<CustomRange<T>>
    where T : struct, IEquatable<T>, IComparable<T>
{
    public static readonly CustomRangeComparer<T> Instance = new();

    private CustomRangeComparer() { }

    public int Compare(CustomRange<T> x, CustomRange<T> y)
    {
        return x.FirstAddress.CompareTo(y.FirstAddress);
    }
}

internal sealed class CustomRange2Comparer<T> : IComparer<CustomRange2<T>>
    where T : IComparable<T>
{
    public static readonly CustomRange2Comparer<T> Instance = new();

    private CustomRange2Comparer() { }

    public int Compare(CustomRange2<T> x, CustomRange2<T> y)
    {
        return x.FirstAddress.CompareTo(y.FirstAddress);
    }
}