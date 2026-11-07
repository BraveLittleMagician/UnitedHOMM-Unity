using System.Collections.Generic;

public abstract class SequenceEnumerable<TPosition> : ISequenceEnumerator<TPosition>
    where TPosition : struct
{
    private static readonly IReadOnlyDictionary<TPosition, Attackable> _emptyAttackable
        = new Dictionary<TPosition, Attackable>();

    protected SequenceEnumerable(TPosition start)
    {
        StartPosition = start;
    }

    protected abstract List<TPosition> CreateOnlyStayablesPositions { get; }
    protected abstract Dictionary<TPosition, Stayable> CreateAllPossiblePositions { get; }

    public TPosition StartPosition { get; protected set; }
    public List<TPosition> PositionsStayables => CreateOnlyStayablesPositions;
    public Dictionary<TPosition, Stayable> PositionsAll => CreateAllPossiblePositions;

    public virtual bool SupportsExplicitAttackable => false;
    public virtual int MaxDepth => int.MaxValue;
    public virtual IReadOnlyDictionary<TPosition, Attackable> PositionsAttackable => _emptyAttackable;

    protected abstract bool ProtectedCanMoveTo(IPath<TPosition> path);

    public bool CanMoveTo(IPath<TPosition> path)
    {
        if (!path.Positions[0].Equals(StartPosition)) return false;
        return ProtectedCanMoveTo(path);
    }

    public abstract bool CanMoveTo(TPosition target);
    public abstract IEnumerable<TPosition> GetNeighbors(TPosition cell);
    public abstract IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);
}