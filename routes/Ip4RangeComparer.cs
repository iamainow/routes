namespace routes;

internal sealed class Ip4RangeComparer : IComparer<Ip4Range>
{
    public static readonly Ip4RangeComparer Instance = new();
    
    private Ip4RangeComparer() { }
    
    public int Compare(Ip4Range x, Ip4Range y)
    {
        return x.FirstAddress.CompareTo(y.FirstAddress);
    }
}
