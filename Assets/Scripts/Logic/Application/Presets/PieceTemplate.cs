#nullable enable

using System;
using System.Collections.Generic;

public sealed record PieceTemplate(
    string Name,
    int Health,
    int AttackDamage,
    int MaxAttackCharges,
    string MoveShapeName,
    string? AttackShapeName,
    IReadOnlyList<IMeleeAttack> MeleeAttacks,
    IReadOnlyList<IRangedAttack> RangedAttacks,
    IReadOnlyList<IAbility> Abilities,
    IReadOnlyList<ModifierDto> Modifiers);