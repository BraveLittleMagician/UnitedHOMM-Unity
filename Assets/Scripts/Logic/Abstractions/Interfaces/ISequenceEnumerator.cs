#nullable enable

using System.Collections.Generic;

public interface ISequenceEnumerator<TPosition> where TPosition : struct
{
    public TPosition StartPosition { get; }
    public List<TPosition> PositionsStayables { get; }
    public Dictionary<TPosition, Stayable> PositionsAll { get; }

    bool CanMoveTo(TPosition target);
    bool CanMoveTo(IPath<TPosition> path);
}