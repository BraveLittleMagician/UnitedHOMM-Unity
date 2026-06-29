#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed record PieceDefinition(string Name, IndexOfPlayer Owner, int Health, IReadOnlyList<IMovement>? Movements = null, IReadOnlyList<IMeleeAttack>? MeleeAttacks = null, IReadOnlyList<IRangedAttack>? RangedAttacks = null, IReadOnlyList<IAbility>? Abilities = null)
{
    public PieceDefinition WithName(string newName) => this with { Name = newName };
    public PieceDefinition WithOwner(IndexOfPlayer newOwner) => this with { Owner = newOwner };
    public PieceDefinition WithHealth(int newHealth) => this with { Health = newHealth };
    public PieceDefinition AddMovements(params IMovement[] movements) => this with { Movements = Movements == null ? movements : new List<IMovement>(Movements).Concat(movements).ToList() };
}