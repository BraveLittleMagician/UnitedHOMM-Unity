#nullable enable

using System;

public sealed class Flow : IFlow, IDisposable
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly IMovementValidator _movementValidator;
    private readonly IAbilityService _abilityService;
    private readonly ICombatService _combatService;

    private bool _disposed;

    public Flow(IHouse house, IEventBus eventBus, ILogger logger, IMovementValidator movementValidator, IAbilityService abilityService, ICombatService combatService)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementValidator = movementValidator ?? throw new ArgumentNullException(nameof(movementValidator));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
    }

    private IResult<TRoom> GetRoom<TRoom>() where TRoom : IRoom
    {
        if (!_house.TryToGetRoom<TRoom>(out var room))
            return Result<TRoom>.Failure($"Комната {typeof(TRoom).Name} не найдена");
        return Result<TRoom>.Success(room);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _logger.Log("Flow уничтожен");
    }

    public IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position)
        where TRoom : IRoom where TPos : struct
    {
        var roomResult = GetRoom<TRoom>();
        if (!roomResult.IsSuccess) return Result<IPiece>.Failure(roomResult.Error!);

        var room = roomResult.Value!;
        var piece = _house.CreatePiece(definition);

        if (!room.Add(piece, position, false, out var error))
        {
            _house.RemovePiece(piece.IndexInHouse);
            piece.Dispose();
            return Result<IPiece>.Failure($"Не удалось добавить фигуру в комнату: {error}");
        }

        _abilityService.ActivateAbilities(piece);
        _eventBus.Publish(new PieceDeployedEvent<TPos>(piece, position, room));
        _logger.Log($"Фигура {piece} добавлена в {room.Name} на позицию {position}");

        return Result<IPiece>.Success(piece);
    }
    public IResult MovePiece(IPiece piece, Square target)
    {
        if (piece == null) return Result.Failure("Piece is null");
        var roomResult = GetRoom<Board>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        if (!room.TryToGetPosition<Square>(piece.IndexInHouse, out var fromPos))
            return Result.Failure($"Piece {piece} is not on the board");

        if (piece.HasMovedThisTurn)
            return Result.Failure($"Piece {piece} already moved this turn");

        IPath<Square>? path = null;
        foreach (var m in piece.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            if (m.TryFindPath(piece, fromPos, target, room, out path) && path != null) break;
        }
        if (path == null)
            return Result.Failure($"No valid path from {fromPos} to {target} for {piece}");

        if (!room.Displace(path, piece.Owner, out var displaceError))
            return Result.Failure($"Move failed: {displaceError}");

        piece.MarkMovedThisTurn();
        piece.MarkFirstActionOfGameTaken(new MoveActivity(fromPos, target));

        _eventBus.Publish(new PieceMovedEvent<Square>(piece, fromPos, target, room));
        _logger.Log($"Piece {piece} moved from {fromPos} to {target}");
        return Result.Success();
    }
    public IResult MeleeAttack(IPiece attacker, Square target)
    {
        if (attacker == null) return Result.Failure("Attacker is null");
        var roomResult = GetRoom<Board>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        if (!room.TryToGetPiece(target, out var targetPiece) || targetPiece == null)
            return Result.Failure($"No piece at {target}");

        var result = _combatService.PerformAttack(attacker, targetPiece, room, AttackKind.Melee);
        if (!result.IsSuccess) return result;

        // Если цель умерла — прыжок на её клетку (как в P2 AttackAction)
        if (targetPiece.IsDead && !room.TryToGetPiece(target, out _))
        {
            if (room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos))
            {
                // Простой Jump: удалить из from, поставить в target
                if (room.Remove(fromPos, attacker.Owner))
                {
                    if (room.Add(attacker, target, true, out var err))
                    {
                        _eventBus.Publish(new PieceMovedEvent<Square>(attacker, fromPos, target, room));
                    }
                    else
                    {
                        _logger.LogError($"Jump failed: {err}");
                    }
                }
            }
        }

        return Result.Success();
    }
    public IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition)
        where TRoomFrom : IRoom where TRoomTo : IRoom
        where TPosFrom : struct where TPosTo : struct
    {
        var fromResult = GetRoom<TRoomFrom>();
        if (!fromResult.IsSuccess) return Result.Failure(fromResult.Error!);

        var fromRoom = fromResult.Value!;
        var toResult = GetRoom<TRoomTo>();
        if (!toResult.IsSuccess) return Result.Failure(toResult.Error!);

        var toRoom = toResult.Value!;

        if (ReferenceEquals(fromRoom, toRoom))
            return Result.Failure("Начальная и конечная комнаты одинаковы");

        if (!fromRoom.TryToGetPiece(fromPosition, out var piece))
            return Result.Failure($"На позиции {fromPosition} нет фигуры");

        if (!piece.Owner.Equals(owner))
            return Result.Failure($"Фигура {piece} не принадлежит игроку {owner}");

        _abilityService.DeactivateAbilities(piece);

        if (!toRoom.Add(piece, toPosition, true, out var error))
        {
            _abilityService.ActivateAbilities(piece);
            return Result.Failure($"Не удалось добавить фигуру в целевую комнату: {error}");
        }

        if (!fromRoom.Remove(fromPosition, owner))
        {
            toRoom.Remove(piece.IndexInHouse);
            _abilityService.ActivateAbilities(piece);
            return Result.Failure("Не удалось удалить фигуру из исходной комнаты");
        }

        _abilityService.ActivateAbilities(piece);
        _logger.Log($"Фигура {piece} перемещена из {fromRoom.Name}:{fromPosition} в {toRoom.Name}:{toPosition}");
        return Result.Success();
    }
}