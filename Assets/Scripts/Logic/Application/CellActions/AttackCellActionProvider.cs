#nullable enable
using System;
using System.Collections.Generic;

public sealed class AttackCellActionProvider : ICellActionProvider
{
    private readonly IFlow _flow;
    private readonly IAttackTargetProvider _attackTargetProvider;
    private readonly IRegistry _registry;
    private readonly ModifierHost _modifierHost;

    public AttackCellActionProvider(
        IFlow flow,
        IAttackTargetProvider attackTargetProvider,
        IRegistry registry,
        ModifierHost modifierHost)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _attackTargetProvider = attackTargetProvider ?? throw new ArgumentNullException(nameof(attackTargetProvider));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
    }

    public IReadOnlyList<CellActionOption> GetActionsForCell(IPiece actor, Square cell)
    {
        if (actor == null || actor.IsDead) return Array.Empty<CellActionOption>();
        if (!_registry.TryToGetRoom(actor.IndexInHouse, out var room)) return Array.Empty<CellActionOption>();
        if (!_attackTargetProvider.IsAttackTarget(actor, cell, room)) return Array.Empty<CellActionOption>();
        if (!room.TryToGetPiece(cell, out var target) || target == null) return Array.Empty<CellActionOption>();

        int damage = actor.AttackDamage + _modifierHost.GetEffectiveDamageBonus(actor);
        return new[]
        {
            new CellActionOption(
                "Attack",
                $"Attack {target.Name} for {damage} damage",
                actor.AttackCharges,
                actor.MaxAttackCharges,
                () => _flow.MeleeAttack(actor, cell)),
        };
    }

    public IReadOnlyList<Square> GetActionCells(IPiece actor)
    {
        if (actor == null || actor.IsDead) return Array.Empty<Square>();
        if (!_registry.TryToGetRoom(actor.IndexInHouse, out var room)) return Array.Empty<Square>();
        return _attackTargetProvider.GetAttackTargets(actor, room);
    }
}