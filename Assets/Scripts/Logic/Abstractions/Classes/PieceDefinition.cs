#nullable enable

using System.Collections.Generic;

public sealed record PieceDefinition(
    string Name,
    IdOfPlayer Owner,
    int Health,
    int AttackDamage,
    int AttackCharges,
    FacingDirection InitialFacing,
    IReadOnlyList<IMovementFactory>? MovementFactories = null,
    IReadOnlyList<IMeleeAttack>? MeleeAttacks = null,
    IReadOnlyList<IRangedAttack>? RangedAttacks = null,
    IReadOnlyList<IAbility>? Abilities = null,
    IAttackEffect? AttackEffect = null);