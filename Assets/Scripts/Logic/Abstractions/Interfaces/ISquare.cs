#nullable enable

using System;
using System.Collections.Generic;

public interface ISquare<TSelf> : IAdjacentable<TSelf>, IComparable<TSelf>, IComparable, IComparableByDistance<TSelf>, IComparableByDistance, IEquatable<TSelf> where TSelf : struct, ISquare<TSelf>
{
    public int X { get; init; }
    public bool IsZero { get; }
    public MultipleAxes ActiveAxes { get; }
    public bool TryGetValue(Axis axis, out int value);

    public int CountOfAxes { get; }
    public KeyValuePair<Axis, int> GetCoordinateAt(int index);
    public TSelf CopyWith(IReadOnlyDictionary<Axis, int> dictionary);
}