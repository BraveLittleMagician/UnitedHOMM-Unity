#nullable enable

using System;

public sealed class Flow : IGameController, IDisposable
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly ICombatService _combatService;
    private readonly IMovementValidator _movementValidator;
    private readonly IAbilityService _abilityService;
    private readonly IRegistry _registry;
    private bool _disposed;

    public Flow(IHouse house, IEventBus eventBus, ILogger logger, ICombatService combatService, IMovementValidator movementValidator, IAbilityService abilityService, IRegistry registry)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
        _movementValidator = movementValidator ?? throw new ArgumentNullException(nameof(movementValidator));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _registry = registry;
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);

    }

    private IResult<TRoom> GetRoom<TRoom>() where TRoom : IRoom
    {
        if (!_house.TryToGetRoom<TRoom>(out var room))
            return Result<TRoom>.Failure($"Комната {typeof(TRoom).Name} не найдена");
        return Result<TRoom>.Success(room);
    }
    private void OnPieceDied(PieceDiedEvent e)
    {
        _abilityService.DeactivateAbilities(e.Piece);
    }

    public IResult<IPiece> AddPiece<TRoom, TPos>(
    PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct
    {
        var roomResult = GetRoom<TRoom>();
        if (!roomResult.IsSuccess) return Result<IPiece>.Failure(roomResult.Error!);

        var room = roomResult.Value!;

        var piece = _house.CreatePiece(definition);

        try
        {
            if (definition.Movements != null) foreach (var m in definition.Movements) piece.AddMovement(m);
            if (definition.MeleeAttacks != null) foreach (var a in definition.MeleeAttacks) piece.AddMeleeAttack(a);
            if (definition.RangedAttacks != null) foreach (var a in definition.RangedAttacks) piece.AddRangedAttack(a);
            if (definition.Abilities != null) foreach (var a in definition.Abilities) piece.AddAbility(a);

            if (!room.Add(piece, position, false, out var error))
            {
                piece.Dispose();
                return Result<IPiece>.Failure($"Не удалось добавить фигуру в комнату: {error}");
            }

            _abilityService.ActivateAbilities(piece);
            _logger.Log($"Фигура {piece} добавлена в {room.Name} на позицию {position}");
            _eventBus.Publish(new PieceDeployedEvent(piece, room));

            return Result<IPiece>.Success(piece);
        }
        catch (Exception ex)
        {
            piece.Dispose();
            return Result<IPiece>.Failure($"Ошибка при добавлении фигуры: {ex.Message}");
        }
    }
    public IResult MovePiece<TRoom, TPos>(IndexOfPlayer owner, IPath<TPos> path) where TRoom : IRoom where TPos : struct
    {
        if (path == null) return Result.Failure("Путь не может быть null");
        var roomResult = GetRoom<TRoom>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);

        var room = roomResult.Value!;
        var start = path.Positions[0];
        if (!room.TryToGetPiece(start, owner, out var piece)) return Result.Failure($"Фигура игрока {owner} не найдена на позиции {start}");

        if (!_movementValidator.CanMove(piece, path, room, false, out var error)) return Result.Failure(error);
        if (!room.Displace(path, owner, out var displaceError)) return Result.Failure($"Перемещение не удалось: {displaceError}");
        _logger.Log($"Фигура {piece} перемещена с {start} на {path.Positions[^1]} в {room.Name}");
        return Result.Success();
    }
    public IResult MeleeAttack<TRoom, TPos>(IndexOfPlayer attackerOwner, IPath<TPos> path) where TRoom : IRoom where TPos : struct
    {
        {
            if (path == null) return Result.Failure("Путь не может быть null");

            var roomResult = GetRoom<TRoom>();
            if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);

            var room = roomResult.Value!;
            var start = path.Positions[0];
            var targetPos = path.Positions[^1];

            if (!room.TryToGetPiece(start, attackerOwner, out var attacker)) return Result.Failure($"Атакующий игрок {attackerOwner} не найден на позиции {start}");

            if (!room.TryToGetPiece(targetPos, new IndexOfPlayer(), out var target)) return Result.Failure($"На позиции {targetPos} нет фигуры для атаки");

            if (attacker.Owner.IndexOfSide == target.Owner.IndexOfSide) return Result.Failure("Нельзя атаковать союзника");

            if (!_movementValidator.CanMove(attacker, path, room, true, out var error)) return Result.Failure(error);

            if (attacker.MeleeAttacks.Count == 0) return Result.Failure($"У {attacker} нет ближних атак");

            var attackResult = _combatService.PerformMeleeAttack(attacker, target, room);
            if (!attackResult.IsSuccess) return attackResult;

            return Result.Success();
        }
    }
    public IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IndexOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct
    {
        var fromResult = GetRoom<TRoomFrom>();
        if (!fromResult.IsSuccess) return Result.Failure(fromResult.Error!);
        
        var fromRoom = fromResult.Value!;
        var toResult = GetRoom<TRoomTo>();
        
        if (!toResult.IsSuccess) return Result.Failure(toResult.Error!);
        
        var toRoom = toResult.Value!;

        if (ReferenceEquals(fromRoom, toRoom)) return Result.Failure("Начальная и конечная комнаты одинаковы");
        if (!fromRoom.TryToGetPiece(fromPosition, owner, out var piece)) return Result.Failure($"Фигура игрока {owner} не найдена на позиции {fromPosition}");
        
        _abilityService.DeactivateAbilities(piece);

        if (!toRoom.Add(piece, toPosition, true, out var error)) return Result.Failure($"Не удалось добавить фигуру в целевую комнату: {error}");

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
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _abilityService.Dispose();
        _logger.Log("Flow уничтожен");
    }
}