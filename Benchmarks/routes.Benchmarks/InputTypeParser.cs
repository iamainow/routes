using routes.Generic;
using System.Numerics;

namespace routes.Benchmarks;

public static class InputTypeParser
{
    public static (InputTypeGeneral, double) Parse(InputType inputType)
    {
        return inputType switch
        {
            InputType.Normalized => (InputTypeGeneral.Normalized, default),
            InputType.Sorted_Overlapping_10 => (InputTypeGeneral.Sorted, 0.1),
            InputType.Sorted_Overlapping_20 => (InputTypeGeneral.Sorted, 0.2),

            InputType.Usorted_Overlapping_0 => (InputTypeGeneral.Unsorted, 0),
            InputType.Usorted_Overlapping_10 => (InputTypeGeneral.Unsorted, 0.1),
            InputType.Usorted_Overlapping_20 => (InputTypeGeneral.Unsorted, 0.2),
            _ => throw new NotImplementedException(),
        };
    }

    public static int Convert<T, TOne>(Span<CustomRange<T>> span, TOne one, InputTypeGeneral fromType, InputTypeGeneral toType)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, TOne, T>, ISubtractionOperators<T, TOne, T>
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    span.Sort(CustomRangeComparer<T>.Instance);
                    return span.Length;
                }
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromUnsorted(span, one);
                }
            case (InputTypeGeneral.Sorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromSorted(span, one);
                }
            default: return span.Length;
        }
    }
}
