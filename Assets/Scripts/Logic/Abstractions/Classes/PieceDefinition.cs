#nullable enable

using System.Collections.Generic;

public sealed record PieceDefinition(string Name, IndexOfPlayer Owner, int Health, IReadOnlyList<IMovement>? Movements = null, IReadOnlyList<IMeleeAttack>? MeleeAttacks = null, IReadOnlyList<IRangedAttack>? RangedAttacks = null, IReadOnlyList<IAbility>? Abilities = null)
{}