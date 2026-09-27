#nullable enable

using System;

public interface IMovementValidator
{
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, bool isAttack, out string error) where TPos : struct;

    bool HasApplicableMovement(IPiece piece, IReadOnlyRoom room);
}