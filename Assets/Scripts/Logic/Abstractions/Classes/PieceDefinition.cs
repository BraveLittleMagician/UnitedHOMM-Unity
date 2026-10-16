#nullable enable

using System.Collections.Generic;

public sealed record PieceDefinition(string Name, IdOfPlayer Owner, int Health, IReadOnlyList<IMovementFactory>? MovementFactories = null, IReadOnlyList<IMeleeAttack>? MeleeAttacks = null, IReadOnlyList<IRangedAttack>? RangedAttacks = null, IReadOnlyList<IAbility>? Abilities = null)
{}