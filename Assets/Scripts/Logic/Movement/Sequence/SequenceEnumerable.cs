#nullable enable

using System.Collections.Generic;

public abstract class SequenceEnumerable<TPosition> : ISequenceEnumerator<TPosition> where TPosition : struct
{
    protected SequenceEnumerable(TPosition start)
    {
        StartPosition = start;
    }

    protected abstract List<TPosition> CreateOnlyStayablesPositions { get; }
    protected abstract Dictionary<TPosition, Stayable> CreateAllPossiblePositions { get; }

    public TPosition StartPosition { get; protected set; }
    public List<TPosition> PositionsStayables => CreateOnlyStayablesPositions;
    public Dictionary<TPosition, Stayable> PositionsAll => CreateAllPossiblePositions;

    protected abstract bool ProtectedCanMoveTo(IPath<TPosition> path);
    public abstract bool CanMoveTo(TPosition target);
    public bool CanMoveTo(IPath<TPosition> path)
    {
        if (!path.Positions[0].Equals(StartPosition)) return false;
        return ProtectedCanMoveTo(path);
    }
}