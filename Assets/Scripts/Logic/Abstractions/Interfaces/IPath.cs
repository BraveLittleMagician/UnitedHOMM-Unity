#nullable enable

using System.Collections.Generic;

public interface IPath<out TPosition> where TPosition : struct
{
    IReadOnlyList<TPosition> Positions { get; }
}