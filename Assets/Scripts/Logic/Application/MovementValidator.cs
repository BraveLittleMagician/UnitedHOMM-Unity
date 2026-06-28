#nullable enable


using System;

public sealed class MovementValidator : IMovementValidator
{
    private readonly ILogger _logger;

    public MovementValidator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, bool isAttack, out string error) where TPos : struct
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (room == null) throw new ArgumentNullException(nameof(room));

        var roomType = room.GetType();

        foreach (var movement in piece.Movements)
        {
            if (movement.IsApplicableToRoomType(roomType) &&
                movement.CanMove(piece, path, room, isAttack))
            {
                error = "";
                _logger.LogDebug($"Движение {movement.GetType().Name} подходит для {piece} в {room.Name} (атака: {isAttack})");
                return true;
            }
        }

        error = $"У {piece} нет подходящего движения для {(isAttack ? "атаки" : "перемещения")} по пути {path} в комнате {room.Name}";
        _logger.LogWarning(error);
        return false;
    }

    public bool HasApplicableMovement(IPiece piece, Type roomType)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (roomType == null) throw new ArgumentNullException(nameof(roomType));

        foreach (var movement in piece.Movements)
            if (movement.IsApplicableToRoomType(roomType))
                return true;
        return false;
    }
}