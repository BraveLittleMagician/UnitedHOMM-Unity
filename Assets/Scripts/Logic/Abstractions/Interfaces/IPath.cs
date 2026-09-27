#nullable enable

using System.Collections.Generic;

public interface IPath<TPosition> where TPosition : struct
{
    IReadOnlyList<TPosition> Positions { get; }
}