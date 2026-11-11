#nullable enable

using System;

public sealed class MovementValidator : IMovementValidator
{
    private readonly ILogger _logger;

    public MovementValidator(ILogger logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error)
        where TPos : struct
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (room == null) throw new ArgumentNullException(nameof(room));

        foreach (var movement in piece.Movements)
        {
            if (!movement.IsApplicableToRoom(room)) continue;
            if (movement.CanMove(piece, path, room, out error))
            {
                _logger.LogDebug($"Движение {movement.GetType().Name} подходит для {piece} в {room.Name}");
                return true;
            }
        }
        error = $"У {piece} нет подходящего движения по пути {path} в комнате {room.Name}";
        _logger.LogWarning(error);
        return false;
    }

    public bool HasApplicableMovement(IPiece piece, IReadOnlyRoom room)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (room == null) throw new ArgumentNullException(nameof(room));
        foreach (var movement in piece.Movements)
            if (movement.IsApplicableToRoom(room)) return true;
        return false;
    }
}