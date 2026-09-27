#nullable enable

using System;

public sealed class CombatService : ICombatService
{
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;

    public CombatService(ILogger logger, IEventBus eventBus)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public IResult PerformMeleeAttack(IPiece attacker, IPiece target, IRoom room)
    {
        if (attacker == null) return Result.Failure("Атакующий не может быть null");
        if (target == null) return Result.Failure("Цель не может быть null");
        if (room == null) return Result.Failure("Комната не может быть null");

        if (!CanAttack(attacker, target, out var error))
            return Result.Failure(error);

        if (attacker.MeleeAttacks.Count == 0)
            return Result.Failure($"У {attacker} нет ближних атак");

        var attack = attacker.MeleeAttacks[0];

        attack.Execute(target);

        _logger.Log($"Ближняя атака: {attacker} → {target} в {room.Name}");
        _eventBus.Publish(new PieceAttackedEvent(attacker, target, room, AttackKind.Melee));

        return Result.Success();
    }
    public IResult PerformRangedAttack(IPiece attacker, IPiece target, IRoom room)
    {
        if (attacker == null) return Result.Failure("Атакующий не может быть null");
        if (target == null) return Result.Failure("Цель не может быть null");
        if (room == null) return Result.Failure("Комната не может быть null");

        if (!CanAttack(attacker, target, out var error))
            return Result.Failure(error);

        if (attacker.RangedAttacks.Count == 0)
            return Result.Failure($"У {attacker} нет дальних атак");

        var attack = attacker.RangedAttacks[0];
        attack.Execute(target);

        _logger.Log($"Дальняя атака: {attacker} → {target} в {room.Name}");
        _eventBus.Publish(new PieceAttackedEvent(attacker, target, room, AttackKind.Ranged));

        return Result.Success();
    }

    public bool CanAttack(IPiece attacker, IPiece target, out string error)
    {
        if (ReferenceEquals(attacker, target))
        {
            error = "Нельзя атаковать самого себя";
            return false;
        }

        if (target.Health <= 0)
        {
            error = "Цель уже мертва";
            return false;
        }

        error = "";
        return true;
    }
}