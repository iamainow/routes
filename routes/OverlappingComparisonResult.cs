namespace routes;

public enum OverlappingComparisonResult
{
    /// <summary>
    ///   [___]
    /// [___]
    /// </summary>
    OverlapsLL,
    /// <summary>
    ///     [___]
    /// [_______]
    /// </summary>
    OverlapsLE,
    /// <summary>
    ///    [___]
    /// [_________]
    /// </summary>
    OverlapsLR,
    /// <summary>
    /// [_______]
    /// [___]
    /// </summary>
    OverlapsEL,
    /// <summary>
    /// [___]
    /// [___]
    /// </summary>
    OverlapsEE,
    /// <summary>
    /// [___]
    /// [_______]
    /// </summary>
    OverlapsER,
    /// <summary>
    /// [_________]
    ///    [___]
    /// </summary>
    OverlapsRL,
    /// <summary>
    /// [_______]
    ///     [___]
    /// </summary>
    OverlapsRE,
    /// <summary>
    /// [___]
    ///   [___]
    /// </summary>
    OverlapsRR,
}