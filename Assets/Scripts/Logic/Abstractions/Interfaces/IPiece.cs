#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public interface IPiece : IDisposable
{
    bool HasMovedThisTurn { get; }
    bool HasPromotedThisTurn { get; }
    int Health { get; }
    int HealthNormal { get; }
    long PieceId { get; }
    string Name { get; }
    BigInteger IndexInHouse { get; }
    IdOfPlayer Owner { get; }
    Vector3Int PositionInGrid { get; }
    FacingDirection Facing { get; }

    IReadOnlyList<IMeleeAttack> MeleeAttacks { get; }
    IReadOnlyList<IRangedAttack> RangedAttacks { get; }
    IReadOnlyList<IAbility> Abilities { get; }

    void AddMeleeAttack(IMeleeAttack attack);
    void AddRangedAttack(IRangedAttack attack);
    void AddAbility(IAbility ability);
    void ApplyOperation(IOperation operation);

    void SetPosition(Vector3Int pos);
    void SetFacing(FacingDirection facing);
    void MarkMovedThisTurn();
    void MarkPromotedThisTurn();
    void ResetTurnState();

    void RestoreHealth(int hp);
    void RestoreMovedThisTurn(bool value);
    void RestorePromotedThisTurn(bool value);
}