#nullable enable

using System;
using System.Collections.Generic;

public interface IMovement
{
    void Dispose();
    bool CanAttack<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool IsApplicableToRoom(IReadOnlyRoom room);
    bool TryFindPath<TPos>(IPiece piece, TPos fromPosition, TPos target, IReadOnlyRoom room, out IPath<TPos>? path) where TPos : struct;
    ISequenceEnumerator<Square> CreateMoveEnumeratorFor(IPiece piece, Square fromPosition);
    ISequenceEnumerator<Square> CreateAttackEnumeratorFor(IPiece piece, Square fromPosition);
    ISequenceEnumerator<Square>? CreateEnumeratorFor(IPiece piece, Square fromPosition);
    IReadOnlyList<TPos> GetValidMoves<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room) where TPos : struct;
    IReadOnlyList<ShapeCell> GetShape<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room, bool includeOccupied) where TPos : struct;
    IReadOnlyDictionary<Square, SquareInfo> GetInfos(IPiece piece, Square fromPosition);

}