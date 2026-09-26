#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    public static IEnumerable<Axis> GetSeparatedAxes(this MultipleAxes axes)
    {
        var axis = (Axis)axes;
        return Enum.GetValues(typeof(Axis)).Cast<Axis>().Where(a => a != Axis.None && axis.HasFlag(a)).OrderBy(a => a);
    }
    public static CountOfDimensions ToCountOfDimensions(this MultipleAxes axes) => axes switch
    {
        MultipleAxes.None => CountOfDimensions.Zero,
        MultipleAxes.One => CountOfDimensions.One,
        MultipleAxes.Two => CountOfDimensions.Two,
        MultipleAxes.Three => CountOfDimensions.Three,
        MultipleAxes.Four => CountOfDimensions.Four,
        _ => CountOfDimensions.Zero,
    };
    public static MultipleAxes ToMultipleAxes(this CountOfDimensions count)
    {
        return count switch
        {
            CountOfDimensions.Zero => MultipleAxes.None,
            CountOfDimensions.One => MultipleAxes.One,
            CountOfDimensions.Two => MultipleAxes.Two,
            CountOfDimensions.Three => MultipleAxes.Three,
            CountOfDimensions.Four => MultipleAxes.Four,
            _ => MultipleAxes.None
        };
    }
    public static Dictionary<int, Stayable> ToInt(this List<Stayable> stayables)
    {
        Dictionary<int, Stayable> indices = new();
        int i = 1;
        foreach (var s in stayables)
        {
            indices.Add(i, s);
            i++;
        }
        return indices;
    }
}