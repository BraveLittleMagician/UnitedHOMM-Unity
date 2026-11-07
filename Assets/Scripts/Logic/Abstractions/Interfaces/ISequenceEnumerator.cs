#nullable enable

using System.Collections.Generic;

public interface ISequenceEnumerator<TPosition> where TPosition : struct
{
    bool SupportsExplicitAttackable { get; }
    int MaxDepth { get; }
    TPosition StartPosition { get; }
    List<TPosition> PositionsStayables { get; }
    Dictionary<TPosition, Stayable> PositionsAll { get; }
    IReadOnlyDictionary<TPosition, Attackable> PositionsAttackable { get; }
    bool CanMoveTo(TPosition target);
    bool CanMoveTo(IPath<TPosition> path);
    IEnumerable<TPosition> GetNeighbors(TPosition cell);
    IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);
}