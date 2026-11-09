#nullable enable
using System.Collections.Generic;

public sealed record PieceDefinition(
    string Name,
    IdOfPlayer Owner,
    int Health,
    string? MoveShapeName = null,
    string? AttackShapeName = null,
    int AttackDamage = 1,
    int MaxAttackCharges = 1,
    IReadOnlyList<IMeleeAttack>? MeleeAttacks = null,
    IReadOnlyList<IRangedAttack>? RangedAttacks = null,
    IReadOnlyList<IAbility>? Abilities = null,
    IReadOnlyList<ModifierDto>? Modifiers = null);