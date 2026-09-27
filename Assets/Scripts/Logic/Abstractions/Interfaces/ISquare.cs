#nullable enable

using System;
using System.Collections.Generic;

public interface ISquare<TSelf> : ISquarePosition, IAdjacentable<TSelf>, IComparable<TSelf>, IComparable, IComparableByDistance<TSelf>, IComparableByDistance, IEquatable<TSelf> where TSelf : struct, ISquare<TSelf>
{
    public int X { get; init; }
    public bool IsZero { get; }
    public int CountOfAxes { get; }
    public MultipleAxes ActiveAxes { get; }
    public bool TryGetValue(Axis axis, out int value);
    public KeyValuePair<Axis, int> GetCoordinateAt(int index);
    public TSelf WithValue(Axis axis, int value);
    public LineKey GetLineKey();

    TSelf Add(TSelf other);
    TSelf Subtract(TSelf other);
    TSelf Multiply(TSelf other);
    TSelf Divide(TSelf other);
    TSelf Module(TSelf other);
}