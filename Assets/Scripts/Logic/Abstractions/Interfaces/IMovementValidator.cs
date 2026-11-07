#nullable enable

public interface IMovementValidator
{
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct;
    bool HasApplicableMovement(IPiece piece, IReadOnlyRoom room);
}