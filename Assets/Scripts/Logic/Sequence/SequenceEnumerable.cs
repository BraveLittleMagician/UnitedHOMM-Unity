#nullable enable

using System.Collections.Generic;

public abstract class SequenceEnumerable<TPosition> : ISequenceEnumerator<TPosition> where TPosition : struct
{
    protected SequenceEnumerable(TPosition start, int maxDepth)
    {
        StartPosition = start;
        MaxDepth = maxDepth;
    }

    protected abstract List<TPosition> CreateOnlyAblesPositions { get; }
    protected abstract Dictionary<TPosition, bool> CreateAllPositions { get; }

    public bool SupportsExplicitAttackable { get; }
    public int MaxDepth { get; }
    public TPosition StartPosition { get; protected set; }
    public List<TPosition> PositionsAble => CreateOnlyAblesPositions;
    public Dictionary<TPosition, bool> PositionsAll => CreateAllPositions;

    protected abstract bool ProtectedCanMoveTo(IPath<TPosition> path);

    public abstract bool CanReach(TPosition target);
    public bool CanFollow(IPath<TPosition> path)
    {
        if (!path.Positions[0].Equals(StartPosition)) return false;
        return ProtectedCanMoveTo(path);
    }
    public abstract IEnumerable<TPosition> GetNeighbors(TPosition cell);
    public abstract IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);
}