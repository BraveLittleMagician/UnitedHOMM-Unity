#nullable enable

using System;
using System.Collections.Generic;

public abstract class SequenceEnumerable<TPosition, TPurpose> : ISequenceEnumerator<TPosition, TPurpose> where TPosition : struct where TPurpose : struct, Enum
{
    protected static readonly TPurpose _activePurpose = (TPurpose)Enum.ToObject(typeof(TPurpose), Convert.ChangeType(1, Enum.GetUnderlyingType(typeof(TPurpose))));

    protected SequenceEnumerable(TPosition start, int maxDepth)
    {
        StartPosition = start;
        MaxDepth = maxDepth;
    }

    protected abstract List<TPosition> CreateOnlyAblesPositions { get; }
    protected abstract Dictionary<TPosition, TPurpose> CreateAllPositions { get; }

    public bool SupportsExplicitAttackable { get; }
    public int MaxDepth { get; }
    public TPosition StartPosition { get; protected set; }
    public List<TPosition> PositionsAble => CreateOnlyAblesPositions;
    public Dictionary<TPosition, TPurpose> PositionsAll => CreateAllPositions;

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