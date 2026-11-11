#nullable enable

using System;
using System.Collections.Generic;

public interface IMovement : IDisposable
{
    bool IsApplicableToRoom(IReadOnlyRoom room);
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool TryFindPath<TPos>(IPiece piece, TPos fromPosition, TPos target, IReadOnlyRoom room, out IPath<TPos>? path) where TPos : struct;
    IReadOnlyList<TPos> GetValidMoves<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room) where TPos : struct;
    IReadOnlyList<ShapeCell> GetShape<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room, bool includeOccupied) where TPos : struct;
    ISequenceEnumerator<Square>? CreateEnumeratorFor(IPiece piece, Square fromPosition);
}