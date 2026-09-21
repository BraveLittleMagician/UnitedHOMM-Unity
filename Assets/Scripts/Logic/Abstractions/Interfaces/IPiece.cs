#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

public interface IPiece : IDisposable
{
    BigInteger IndexInHouse { get; }
    IndexOfPlayer Owner { get; }
    string Name { get; }
    int Health { get; }
    IReadOnlyList<IMovement> Movements { get; }
    IReadOnlyList<IMeleeAttack> MeleeAttacks { get; }
    IReadOnlyList<IRangedAttack> RangedAttacks { get; }
    IReadOnlyList<IAbility> Abilities { get; }

    void AddMovement(IMovement movement);
    void AddMeleeAttack(IMeleeAttack attack);
    void AddRangedAttack(IRangedAttack attack);
    void AddAbility(IAbility ability);
    void ApplyOperation(IOperation operation);
}