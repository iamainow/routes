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

            InputType.Sorted_Overlapping_25 => (InputTypeGeneral.Sorted, 0.25),
            InputType.Sorted_Overlapping_50 => (InputTypeGeneral.Sorted, 0.5),
            InputType.Sorted_Overlapping_75 => (InputTypeGeneral.Sorted, 0.75),

            InputType.Usorted_Overlapping_25 => (InputTypeGeneral.Unsorted, 0.25),
            InputType.Usorted_Overlapping_50 => (InputTypeGeneral.Unsorted, 0.5),
            InputType.Usorted_Overlapping_75 => (InputTypeGeneral.Unsorted, 0.75),
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
