#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Flow : IFlow, IDisposable
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly IMovementResolver _movementValidator;
    private readonly IAbilityService _abilityService;
    private readonly IAttackTargetProvider _attackTargetProvider;
    private readonly PieceRegistry _pieceRegistry;
    private readonly Func<Vector3Int> _gridSizeProvider;
    private bool _disposed;

    public Flow(
        IHouse house,
        IEventBus eventBus,
        ILogger logger,
        IMovementResolver movementValidator,
        IAbilityService abilityService,
        PieceRegistry pieceRegistry,
        IAttackTargetProvider attackTargetProvider,
        Func<Vector3Int> gridSizeProvider)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementValidator = movementValidator ?? throw new ArgumentNullException(nameof(movementValidator));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _pieceRegistry = pieceRegistry ?? throw new ArgumentNullException(nameof(pieceRegistry));
        _gridSizeProvider = gridSizeProvider ?? throw new ArgumentNullException(nameof(gridSizeProvider));
        _attackTargetProvider = attackTargetProvider ?? throw new ArgumentNullException(nameof(attackTargetProvider));
    }

    private IResult<TRoom> GetRoom<TRoom>() where TRoom : IRoom
    {
        if (!_house.TryToGetRoom<TRoom>(out var room))
            return Result<TRoom>.Failure($"Комната {typeof(TRoom).Name} не найдена");
        return Result<TRoom>.Success(room);
    }
    private void SyncHp(IPiece piece)
    {
        var model = _pieceRegistry.GetByPieceId(piece.PieceId);
        model?.SyncHp(piece.Health);
    }
    private static Vector3Int ToV3(Square s) => new(s.X, s.Y, s.Z);

    private static bool PathsEqual(IReadOnlyList<Square> a, IReadOnlyList<Square> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (!a[i].Equals(b[i])) return false;
        return true;
    }

    public IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct
    {
        if (definition == null)
            return Result<IPiece>.Failure("PieceDefinition не может быть null");

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

        if (position is Square sq) piece.SetPosition(new Vector3Int(sq.X, sq.Y, sq.Z));

        var cell = ToV3(position is Square s ? s : throw new InvalidOperationException(
            $"AddPiece поддерживает только Square-позиции, получен {typeof(TPos).Name}"));
        var model = new PieceModel(
            piece.Name,
            piece.Owner,
            cell,
            attackDamage: definition.AttackDamage,
            maxHp: definition.Health,
            maxAttackCharges: definition.MaxAttackCharges,
            facing: FacingDirection.PlusY,
            pieceId: piece.PieceId);

        try
        {
            _pieceRegistry.Register(model);
        }
        catch (InvalidOperationException ex)
        {
            room.Remove(piece.IndexInHouse);
            _house.RemovePiece(piece.IndexInHouse);
            piece.Dispose();
            return Result<IPiece>.Failure($"Не удалось зарегистрировать фигуру в PieceRegistry: {ex.Message}");
        }

        // TODO(Phase 8): _modifierHost.AttachModifiers(model, definition.Modifiers, _eventBus);

        _abilityService.ActivateAbilities(piece);
        _eventBus.Publish(new PieceDeployedEvent<TPos>(piece, position, room));
        _logger.Log($"Фигура {piece} добавлена в {room.Name} на позицию {position}");
        return Result<IPiece>.Success(piece);
    }

    public IResult MovePiece<TRoom, TPos>(IdOfPlayer owner, IPath<TPos> path)
        where TRoom : IRoom where TPos : struct
    {
        if (path == null) return Result.Failure("Путь не может быть null");
        if (path is not IPath<Square> typedPath)
            return Result.Failure($"Путь должен быть в координатах Square (получен {typeof(TPos).Name})");
        if (typedPath.Positions.Count < 2)
            return Result.Failure("Путь должен содержать хотя бы 2 позиции");

        var roomResult = GetRoom<TRoom>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        var start = typedPath.Positions[0];
        var end = typedPath.Positions[^1];

        if (!room.TryToGetPiece(start, out var piece))
            return Result.Failure($"На позиции {start} нет фигуры");
        if (!piece.Owner.Equals(owner))
            return Result.Failure($"Фигура {piece} не принадлежит игроку {owner}");

        if (_pieceRegistry.GetByPieceId(piece.PieceId) is not { } model)
            return Result.Failure($"PieceModel для {piece} не найден в PieceRegistry");

        if (!_movementValidator.TryFindPath(model, ToV3(end), _gridSizeProvider(), out var builtPath))
            return Result.Failure(
                $"У {piece} нет подходящего движения в {end} в комнате {room.Name}");

        if (!PathsEqual(builtPath.Positions, typedPath.Positions))
            return Result.Failure(
                "Переданный путь не совпадает с найденным путём движения " +
                $"(caller: {typedPath.Positions.Count} шагов, resolver: {builtPath.Positions.Count} шагов)");

        if (!room.Displace(typedPath, owner, out var displaceError))
            return Result.Failure($"Перемещение не удалось: {displaceError}");

        piece.SetPosition(ToV3(end));
        _pieceRegistry.Move(model, ToV3(end));

        _eventBus.Publish(new PieceMovedEvent<Square>(piece, start, end, room));

        _logger.Log($"Фигура {piece} перемещена с {start} на {end} в {room.Name}");
        return Result.Success();
    }

    public IResult MeleeAttack<TRoom, TPos>(IdOfPlayer attackerOwner, IPath<TPos> path)
        where TRoom : IRoom where TPos : struct
        => PerformAttack<TRoom, TPos>(attackerOwner, path, AttackKind.Melee);

    public IResult RangedAttack<TRoom, TPos>(IdOfPlayer attackerOwner, IPath<TPos> path)
        where TRoom : IRoom where TPos : struct
        => PerformAttack<TRoom, TPos>(attackerOwner, path, AttackKind.Ranged);

    private IResult PerformAttack<TRoom, TPos>(IdOfPlayer attackerOwner, IPath<TPos> path, AttackKind kind) where TRoom : IRoom where TPos : struct
    {
        if (path == null) return Result.Failure("Путь не может быть null");
        if (path is not IPath<Square> typedPath)
            return Result.Failure($"Путь должен быть в координатах Square (получен {typeof(TPos).Name})");
        if (typedPath.Positions.Count < 2)
            return Result.Failure("Путь должен содержать хотя бы 2 позиции");

        var roomResult = GetRoom<TRoom>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        var start = typedPath.Positions[0];
        var end = typedPath.Positions[^1];

        if (!room.TryToGetPiece(start, out var attacker))
            return Result.Failure($"На позиции {start} нет фигуры");
        if (!attacker.Owner.Equals(attackerOwner))
            return Result.Failure($"Фигура {attacker} не принадлежит игроку {attackerOwner}");
        if (!room.TryToGetPiece(end, out var target))
            return Result.Failure($"На позиции {end} нет фигуры-цели");
        if (target.Owner.Equals(attackerOwner))
            return Result.Failure($"Фигура {target} — союзник, нельзя атаковать");

        var attackerModel = _pieceRegistry.GetByPieceId(attacker.PieceId);
        if (attackerModel is null)
            return Result.Failure($"PieceModel для {attacker} не найден в PieceRegistry");
        var targetModel = _pieceRegistry.GetByPieceId(target.PieceId);
        if (targetModel is null)
            return Result.Failure($"PieceModel для {target} не найден в PieceRegistry");

        if (!_attackTargetProvider.IsAttackTarget(attackerModel, ToV3(end), _gridSizeProvider()))
            return Result.Failure($"Фигура {attacker} не может атаковать клетку {end}");

        if (!attackerModel.TryConsumeAttackCharge())
            return Result.Failure(
                $"У {attacker} закончились атаки " +
                $"(Charges={attackerModel.AttackCharges}/{attackerModel.MaxAttackCharges})");

        attackerModel.MarkFirstActionOfGameTaken(new AttackActivity(ToV3(end)));

        target.ApplyOperation(new SubtractOperation(attackerModel.AttackDamage));

        SyncHp(target);
        SyncHp(attacker);

        _eventBus.Publish(new PieceAttackedEvent(attacker, target, room, kind));
        _logger.Log(
            $"{kind} атака: {attacker} → {target} в {room.Name} " +
            $"(урон {attackerModel.AttackDamage}, у цели HP={target.Health})");

        return Result.Success();
    }

    public IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(
        IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition)
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

        if (!toRoom.Add(piece, toPosition, true, out var addError))
        {
            _abilityService.ActivateAbilities(piece);
            return Result.Failure($"Не удалось добавить фигуру в целевую комнату: {addError}");
        }

        if (!fromRoom.Remove(fromPosition, owner))
        {
            toRoom.Remove(piece.IndexInHouse);
            _abilityService.ActivateAbilities(piece);
            return Result.Failure("Не удалось удалить фигуру из исходной комнаты");
        }

        if (toPosition is Square sqTo)
        {
            piece.SetPosition(new Vector3Int(sqTo.X, sqTo.Y, sqTo.Z));
            if (_pieceRegistry.GetByPieceId(piece.PieceId) is { } model)
                _pieceRegistry.Move(model, new Vector3Int(sqTo.X, sqTo.Y, sqTo.Z));
        }

        _abilityService.ActivateAbilities(piece);
        _logger.Log(
            $"Фигура {piece} перемещена из {fromRoom.Name}:{fromPosition} в {toRoom.Name}:{toPosition}");
        return Result.Success();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _logger.Log("Flow уничтожен");
    }
}