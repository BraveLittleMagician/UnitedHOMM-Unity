#nullable enable

using System.Collections.Generic;

public interface ISequenceEnumerator<TPosition> where TPosition : struct
{
    int MaxDepth { get; }
    TPosition StartPosition { get; }
    Dictionary<TPosition, bool> PositionsAll { get; }
    List<TPosition> PositionsAble { get; }

    bool CanReach(TPosition target);
    bool CanFollow(IPath<TPosition> path);
    IEnumerable<TPosition> GetNeighbors(TPosition cell);
    IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);
}