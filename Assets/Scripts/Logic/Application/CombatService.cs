#nullable enable

using System;

public sealed class CombatService : ICombatService
{
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;
    private readonly IAttackTargetProvider _targetProvider;
    private readonly ModifierHost _modifierHost;
    private int _attackDepth;

    public CombatService(
        ILogger logger,
        IEventBus eventBus,
        IAttackTargetProvider targetProvider,
        ModifierHost modifierHost)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _targetProvider = targetProvider ?? throw new ArgumentNullException(nameof(targetProvider));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
    }

    public IResult PerformAttack(IPiece attacker, IPiece target, IRoom room, AttackKind kind)
    {
        if (attacker == null) return Result.Failure("Attacker is null");
        if (target == null) return Result.Failure("Target is null");
        if (room == null) return Result.Failure("Room is null");

        if (!CanAttack(attacker, target, room, out var error))
            return Result.Failure(error);

        if (!room.TryToGetPosition<Square>(target.IndexInHouse, out var targetPos))
            return Result.Failure($"Target {target} has no position in room {room.Name}");

        if (!attacker.TryConsumeAttackCharge())
            return Result.Failure($"No attack charges left for {attacker}");

        attacker.MarkFirstActionOfGameTaken(new AttackActivity(targetPos));

        int effective = attacker.AttackDamage + _modifierHost.GetEffectiveDamageBonus(attacker);
        var effect = ResolveEffect(attacker);
        effect.ApplyTo(target, attacker, room, effective);

        _attackDepth++;
        try
        {
            _eventBus.Publish(new PieceAttackedEvent(attacker, target, room, kind, _attackDepth));
        }
        finally
        {
            _attackDepth--;
        }

        _logger.Log($"{kind} attack: {attacker} -> {target} (dmg={effective})");
        return Result.Success();
    }

    public bool CanAttack(IPiece attacker, IPiece target, IRoom room, out string error)
    {
        if (attacker == null) { error = "Attacker is null"; return false; }
        if (target == null) { error = "Target is null"; return false; }
        if (room == null) { error = "Room is null"; return false; }
        if (attacker.IsDead) { error = "Attacker is dead"; return false; }
        if (target.IsDead) { error = "Target is dead"; return false; }
        if (attacker.Owner.Equals(target.Owner)) { error = "Cannot attack ally"; return false; }
        if (attacker.AttackCharges <= 0) { error = "No attack charges"; return false; }
        if (attacker.HasPromotedThisTurn) { error = "Attacker promoted this turn"; return false; }
        if (_modifierHost.IsAttackBlocked(attacker)) { error = "Attack is blocked"; return false; }

        if (!room.TryToGetPosition<Square>(target.IndexInHouse, out var targetPos))
        {
            error = "Target has no position";
            return false;
        }

        if (!_targetProvider.IsAttackTarget(attacker, targetPos, room))
        {
            error = $"Cell {targetPos} is not a valid attack target for {attacker}";
            return false;
        }

        error = "";
        return true;
    }

    private IAttackEffect ResolveEffect(IPiece attacker) => attacker.AttackEffect;
}