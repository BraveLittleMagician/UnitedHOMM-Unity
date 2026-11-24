#nullable enable

using System;
using System.Collections.Generic;

public interface ISequenceEnumerator<TPosition, TPurpose> where TPosition : struct where TPurpose : struct, Enum
{
    int MaxDepth { get; }
    TPosition StartPosition { get; }
    Dictionary<TPosition, TPurpose> PositionsAll { get; }
    List<TPosition> PositionsAble { get; }

    bool CanReach(TPosition target);
    bool CanFollow(IPath<TPosition> path);
    IEnumerable<TPosition> GetNeighbors(TPosition cell);
    IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);
}