#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;

public interface IPiece : IDisposable
{
    BigInteger IndexInHouse { get; }
    IdOfPlayer Owner { get; }
    string Name { get; }

    int Health { get; }
    int HealthNormal { get; }
    int AttackDamage { get; }
    int AttackCharges { get; }
    int MaxAttackCharges { get; }
    bool IsDead { get; }

    FacingDirection Facing { get; }
    IAttackEffect AttackEffect { get; }

    bool HasTakenFirstActionOfGame { get; }
    bool HasMovedThisTurn { get; }
    bool HasPromotedThisTurn { get; }
    int TotalRotationDegreesThisTurn { get; }

    IReadOnlyList<IMovement> Movements { get; }
    IReadOnlyList<IAbility> Abilities { get; }

    void AddMovement(IMovement movement);
    void AddAbility(IAbility ability);

    void ApplyOperation(IOperation operation);

    bool TakeDamage(int amount);
    void RecoverHp(int amount);
    bool TryConsumeAttackCharge();
    void RestoreCombat(int hp, int attackCharges);

    void RestoreFacing(FacingDirection facing);
    void MarkFirstActionOfGameTaken(Activity activity);
    void MarkMovedThisTurn();
    void AddRotationDegreesThisTurn(int degrees);
    void MarkPromotedThisTurn();
    void ResetTurnState();

    void RestoreFirstActionOfGameTaken(bool value);
    void RestoreMovedThisTurn(bool value);
    void RestoreTotalRotationDegrees(int value);
    void RestorePromotedThisTurn(bool value);
}