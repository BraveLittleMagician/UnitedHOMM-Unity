#nullable enable

using System;
using System.Collections.Generic;

public sealed class Flow : IFlow, IDisposable
{
    private readonly IHouse _house;
    private readonly IRegistry _registry;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly IMovementValidator _movementValidator;
    private readonly IAbilityService _abilityService;
    private readonly ICombatService _combatService;
    private readonly ModifierHost _modifierHost;
    private readonly Action? _onCommitted;

    private readonly Stack<IUndoableOperation> _undoStack = new();
    private readonly Stack<IUndoableOperation> _redoStack = new();
    private bool _disposed;

    private readonly EnPassantService? _enPassantService;
    private readonly AmbushService? _ambushService;

    public Flow(IHouse house, IRegistry registry, IEventBus eventBus, ILogger logger, IMovementValidator movementValidator, IAbilityService abilityService, ICombatService combatService, ModifierHost modifierHost, EnPassantService? enPassantService = null, AmbushService? ambushService = null, Action? onCommitted = null)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementValidator = movementValidator ?? throw new ArgumentNullException(nameof(movementValidator));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _enPassantService = enPassantService;
        _ambushService = ambushService;
        _onCommitted = onCommitted;
    }

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    private IResult<TRoom> GetRoom<TRoom>() where TRoom : IRoom
    {
        if (!_house.TryToGetRoom<TRoom>(out var room))
            return Result<TRoom>.Failure($"Комната {typeof(TRoom).Name} не найдена");
        return Result<TRoom>.Success(room);
    }

    public IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct
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
        if (piece.IsDead) return Result.Failure($"Piece {piece} is dead");
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

        var flagsBefore = new PieceActionFlags(piece.HasMovedThisTurn, piece.HasTakenFirstActionOfGame);

        if (!room.Displace(path, piece.Owner, out var displaceError))
            return Result.Failure($"Move failed: {displaceError}");

        piece.MarkMovedThisTurn();
        piece.MarkFirstActionOfGameTaken(new MoveActivity(fromPos, target));

        var flagsAfter = new PieceActionFlags(piece.HasMovedThisTurn, piece.HasTakenFirstActionOfGame);

        _undoStack.Push(new MoveUndo(piece, room, path, fromPos, target, flagsBefore, flagsAfter, _eventBus));
        _redoStack.Clear();

        _eventBus.Publish(new PieceMovedEvent<Square>(piece, fromPos, target, room));
        _logger.Log($"Piece {piece} moved from {fromPos} to {target}");
        return Result.Success();
    }
    public IResult MeleeAttack(IPiece attacker, Square target)
    {
        if (attacker == null) return Result.Failure("Attacker is null");
        if (attacker.IsDead) return Result.Failure("Attacker is dead");
        var roomResult = GetRoom<Board>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        if (!room.TryToGetPiece(target, out var targetPiece) || targetPiece == null)
            return Result.Failure($"No piece at {target}");

        var before = WorldState.Capture(_registry, _modifierHost);

        var result = _combatService.PerformAttack(attacker, targetPiece, room, AttackKind.Melee);
        if (!result.IsSuccess) return result;

        // Jump на клетку убитой цели (как в P2 AttackAction)
        if (targetPiece.IsDead && !room.TryToGetPiece(target, out _))
        {
            if (room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos))
            {
                if (room.Remove(fromPos, attacker.Owner))
                {
                    if (!room.Add(attacker, target, true, out var jumpErr))
                        _logger.LogError($"Jump failed: {jumpErr}");
                }
            }
        }

        var after = WorldState.Capture(_registry, _modifierHost);
        if (before.HasChanges(after))
        {
            _undoStack.Push(new WorldDelta(before, after, _registry, _modifierHost, _eventBus, _logger));
            _redoStack.Clear();
        }

        return Result.Success();
    }
    public IResult ActivateModifier(IPiece piece, IActiveModifier modifier)
    {
        if (piece == null || modifier == null) return Result.Failure("Null argument");
        var ctx = new ModifierActivationContext(piece, _eventBus, _modifierHost, _registry);

        if (!modifier.CanActivate(ctx))
            return Result.Failure($"Cannot activate '{modifier.ModifierName}'");
        if (modifier.CurrentCharges == 0)
            return Result.Failure($"No charges for '{modifier.ModifierName}'");

        var before = WorldState.Capture(_registry, _modifierHost);
        if (!modifier.TryConsumeCharge())
            return Result.Failure($"Failed to consume charge of '{modifier.ModifierName}'");
        modifier.Activate(ctx);
        if (modifier.TriggersStartedAction)
            piece.MarkFirstActionOfGameTaken(new ModifierActivity(modifier.ModifierName));

        var after = WorldState.Capture(_registry, _modifierHost);
        if (before.HasChanges(after))
        {
            _undoStack.Push(new WorldDelta(before, after, _registry, _modifierHost, _eventBus, _logger));
            _redoStack.Clear();
        }
        return Result.Success();
    }
    public IResult ActivateTargetedModifier(IPiece piece, ITargetedModifier modifier, Square targetCell)
    {
        if (piece == null || modifier == null) return Result.Failure("Null argument");
        var ctx = new ModifierActivationContext(piece, _eventBus, _modifierHost, _registry);

        if (!modifier.CanActivate(ctx))
            return Result.Failure($"Cannot activate '{modifier.ModifierName}'");
        if (modifier.CurrentCharges == 0)
            return Result.Failure($"No charges for '{modifier.ModifierName}'");

        if (!_registry.TryToGetRoom(piece.IndexInHouse, out var room) || !(room is ISquarePositionRoom sqRoom))
            return Result.Failure("Piece is not on a square-position room");
        if (!sqRoom.TryToGetPosition<Square>(piece.IndexInHouse, out var ownerPos))
            return Result.Failure("Piece has no position");

        var gridSize = sqRoom is Board b ? b.Size : new Square { X = int.MaxValue, Y = int.MaxValue, Z = int.MaxValue, W = int.MaxValue };

        var cells = modifier.GetTargetCells(piece, ownerPos, gridSize);
        bool valid = false;
        for (int i = 0; i < cells.Count; i++) if (cells[i].Equals(targetCell)) { valid = true; break; }
        if (!valid) return Result.Failure($"Cell {targetCell} is not a valid target for '{modifier.ModifierName}'");

        var before = WorldState.Capture(_registry, _modifierHost);
        if (!modifier.TryConsumeCharge())
            return Result.Failure($"Failed to consume charge of '{modifier.ModifierName}'");
        modifier.Activate(ctx, targetCell);
        if (modifier.TriggersStartedAction)
            piece.MarkFirstActionOfGameTaken(new ModifierActivity(modifier.ModifierName));

        var after = WorldState.Capture(_registry, _modifierHost);
        if (before.HasChanges(after))
        {
            _undoStack.Push(new WorldDelta(before, after, _registry, _modifierHost, _eventBus, _logger));
            _redoStack.Clear();
        }
        return Result.Success();
    }
    public IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct
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
    public IReadOnlyList<EnPassantTarget> GetAvailableEnPassantTargets(IPiece attacker)
    {
        if (_enPassantService == null || attacker == null || attacker.IsDead)
            return Array.Empty<EnPassantTarget>();
        return _enPassantService.GetAvailableTargets(attacker);
    }
    public IReadOnlyList<AmbushTarget> GetAvailableAmbushTargets(IPiece attacker)
    {
        if (_ambushService == null || attacker == null || attacker.IsDead)
            return Array.Empty<AmbushTarget>();
        return _ambushService.GetAvailableTargets(attacker);
    }
    public IResult EnPassantAttack(IPiece attacker, EnPassantTarget target)
    {
        if (attacker == null) return Result.Failure("Attacker is null");
        if (target.Victim == null) return Result.Failure("Victim is null");
        if (target.Victim.IsDead) return Result.Failure("Victim is already dead");

        var roomResult = GetRoom<Board>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        if (attacker.HasMovedThisTurn) return Result.Failure("Attacker already moved this turn");
        if (attacker.HasPromotedThisTurn) return Result.Failure("Attacker promoted this turn");
        if (!_modifierHost.TryGetModifier<EnPassantModifier>(attacker.IndexInHouse, out var ep))
            return Result.Failure("EnPassant modifier not found on attacker");
        if (ep.CurrentCharges <= 0) return Result.Failure("No EnPassant charges");
        if (!attacker.TryConsumeAttackCharge()) return Result.Failure("No attack charges");

        var before = WorldState.Capture(_registry, _modifierHost);

        if (!ep.TryConsumeCharge())
            return Result.Failure("Failed to consume EnPassant charge");

        int damage = attacker.AttackDamage + _modifierHost.GetEffectiveDamageBonus(attacker);
        target.Victim.TakeDamage(damage);
        _eventBus.Publish(new PieceAttackedEvent(attacker, target.Victim, room, AttackKind.Melee, Depth: 1));

        attacker.MarkMovedThisTurn();
        attacker.MarkFirstActionOfGameTaken(new AttackActivity(target.VictimCell));

        // Move attacker to via cell (если свободна)
        if (!room.TryToGetPiece(target.ViaCell, out _))
        {
            if (room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos))
            {
                if (room.Remove(fromPos, attacker.Owner))
                {
                    if (room.Add(attacker, target.ViaCell, true, out var err))
                    {
                        _eventBus.Publish(new PieceMovedEvent<Square>(attacker, fromPos, target.ViaCell, room));
                    }
                    else
                    {
                        _logger.LogError($"EnPassant move to via failed: {err}");
                    }
                }
            }
        }

        var after = WorldState.Capture(_registry, _modifierHost);
        if (before.HasChanges(after))
        {
            _undoStack.Push(new WorldDelta(before, after, _registry, _modifierHost, _eventBus, _logger));
            _redoStack.Clear();
        }

        _logger.Log($"EnPassant: {attacker} -> {target.Victim} at {target.VictimCell}");
        return Result.Success();
    }
    public IResult AmbushAttack(IPiece attacker, AmbushTarget target)
    {
        if (attacker == null) return Result.Failure("Attacker is null");
        if (target.Victim == null) return Result.Failure("Victim is null");
        if (target.Victim.IsDead) return Result.Failure("Victim is already dead");

        var roomResult = GetRoom<Board>();
        if (!roomResult.IsSuccess) return Result.Failure(roomResult.Error!);
        var room = roomResult.Value!;

        if (attacker.HasPromotedThisTurn) return Result.Failure("Attacker promoted this turn");
        if (!_modifierHost.TryGetModifier<AmbushModifier>(attacker.IndexInHouse, out var amb))
            return Result.Failure("Ambush modifier not found on attacker");
        if (amb.CurrentCharges <= 0) return Result.Failure("No Ambush charges");

        var before = WorldState.Capture(_registry, _modifierHost);

        if (!amb.TryConsumeCharge())
            return Result.Failure("Failed to consume Ambush charge");

        int damage = attacker.AttackDamage + _modifierHost.GetEffectiveDamageBonus(attacker);
        target.Victim.TakeDamage(damage);
        _eventBus.Publish(new PieceAttackedEvent(attacker, target.Victim, room, AttackKind.Melee, Depth: 1));
        attacker.MarkFirstActionOfGameTaken(new ModifierActivity("Ambush"));

        var after = WorldState.Capture(_registry, _modifierHost);
        if (before.HasChanges(after))
        {
            _undoStack.Push(new WorldDelta(before, after, _registry, _modifierHost, _eventBus, _logger));
            _redoStack.Clear();
        }

        _logger.Log($"Ambush: {attacker} -> {target.Victim} at {target.VictimCell}");
        return Result.Success();
    }

    public IResult Undo()
    {
        if (_undoStack.Count == 0) return Result.Failure("Nothing to undo");
        var top = _undoStack.Peek();
        if (!top.Undo()) return Result.Failure("Undo failed");
        _undoStack.Pop();
        _redoStack.Push(top);
        _logger.LogDebug($"Undo applied, undo={_undoStack.Count}, redo={_redoStack.Count}");
        return Result.Success();
    }
    public IResult Redo()
    {
        if (_redoStack.Count == 0) return Result.Failure("Nothing to redo");
        var top = _redoStack.Peek();
        if (!top.Redo()) return Result.Failure("Redo failed");
        _redoStack.Pop();
        _undoStack.Push(top);
        _logger.LogDebug($"Redo applied, undo={_undoStack.Count}, redo={_redoStack.Count}");
        return Result.Success();
    }
    public IResult UndoAll()
    {
        if (_undoStack.Count == 0) return Result.Failure("Nothing to undo");
        while (_undoStack.Count > 0)
        {
            var top = _undoStack.Pop();
            if (!top.Undo())
            {
                _undoStack.Push(top);
                return Result.Failure("UndoAll failed mid-way");
            }
            _redoStack.Push(top);
        }
        _logger.LogDebug($"UndoAll applied, undo=0, redo={_redoStack.Count}");
        return Result.Success();
    }
    public IResult Commit()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _logger.Log("Plan committed");
        _onCommitted?.Invoke();
        return Result.Success();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _undoStack.Clear();
        _redoStack.Clear();
        _logger.Log("Flow уничтожен");
    }
}