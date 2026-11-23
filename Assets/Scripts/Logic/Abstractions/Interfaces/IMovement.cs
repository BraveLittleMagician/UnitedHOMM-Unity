#nullable enable

using System.Collections.Generic;

public interface IMovement
{
    void Dispose();
    bool TryFindPath<TPos>(IPiece piece, TPos fromPosition, TPos target, IReadOnlyRoom room, out IPath<TPos>? path) where TPos : struct;
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool CanAttack<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool IsApplicableToRoom(IReadOnlyRoom room);
    bool HasAtLeastOneStayableCell(IPiece piece, Square fromPosition) => false;
    ISequenceEnumerator<Square>? CreateEnumeratorFor(IPiece piece, Square fromPosition);
    IReadOnlyList<ShapeCell> GetShape<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room, bool includeOccupied) where TPos : struct;
    IReadOnlyList<TPos> GetValidMoves<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room) where TPos : struct;
    ISequenceEnumerator<Square> CreateMoveEnumeratorFor(IPiece piece, Square fromPosition);
    ISequenceEnumerator<Square> CreateAttackEnumeratorFor(IPiece piece, Square fromPosition);
    IReadOnlyDictionary<Square, SquareInfo> GetInfos(IPiece piece, Square fromPosition);
}