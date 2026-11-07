#nullable enable
using System;
using UnityEngine;
public sealed class PieceModel
{
    private static long _nextPieceId = 1;

    public PieceModel(string name, IdOfPlayer player, Vector3Int positionInGrid, int attackDamage, int maxHp, int maxAttackCharges, FacingDirection facing = FacingDirection.PlusY, long? pieceId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Piece name must not be empty.", nameof(name));
        if (attackDamage < 0) throw new ArgumentOutOfRangeException(nameof(attackDamage), "AttackDamage must be non-negative.");
        if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp), "MaxHp must be positive.");
        if (maxAttackCharges < 0) throw new ArgumentOutOfRangeException(nameof(maxAttackCharges), "MaxAttackCharges must be non-negative.");
        if (pieceId.HasValue)
        {
            PieceId = pieceId.Value;
            if (PieceId >= _nextPieceId) _nextPieceId = PieceId + 1;
        }
        else
        {
            PieceId = _nextPieceId++;
        }
        Name = name;
        Player = player;
        PositionInGrid = positionInGrid;
        AttackDamage = attackDamage;
        MaxHp = maxHp;
        Hp = maxHp;
        MaxAttackCharges = maxAttackCharges;
        AttackCharges = maxAttackCharges;
        Facing = facing;
    }
    public long PieceId { get; }
    public string Name { get; }
    public IdOfPlayer Player { get; }
    public Vector3Int PositionInGrid { get; internal set; }
    public int AttackDamage { get; }
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public int MaxAttackCharges { get; }
    public int AttackCharges { get; private set; }
    public FacingDirection Facing { get; private set; }
    public bool HasTakenFirstActionOfGame { get; private set; }
    public bool HasMovedThisTurn { get; private set; }
    public int TotalRotationDegreesThisTurn { get; private set; }
    public bool IsDead => Hp <= 0;
    public bool HasPromotedThisTurn { get; private set; }
    
    public bool TryConsumeAttackCharge()
    {
        if (AttackCharges <= 0) return false;
        AttackCharges--;
        return true;
    }
    public bool TakeDamage(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
        if (amount == 0 || IsDead) return false;
        Hp = Math.Max(0, Hp - amount);
        if (IsDead) DeadStateChanged?.Invoke(this, true);
        return IsDead;
    }
    public void MarkFirstActionOfGameTaken(Activity activity)
    {
        if (HasTakenFirstActionOfGame) return;
        HasTakenFirstActionOfGame = true;
        FirstActionOfGameTaken?.Invoke(this, activity);
    }
    public void MarkMovedThisTurn()
    {
        HasMovedThisTurn = true;
    }
    public void AddRotationDegreesThisTurn(int degrees)
    {
        if (degrees < 0)
            throw new ArgumentOutOfRangeException(nameof(degrees), "Degrees must be non-negative.");
        TotalRotationDegreesThisTurn += degrees;
    }
    public void ResetTurnState()
    {
        HasMovedThisTurn = false;
        TotalRotationDegreesThisTurn = 0;
        HasPromotedThisTurn = false;
        // HasTakenFirstActionOfGame НЕ сбрасывается: это флаг «первое действие
        // за всю игру», а не «за ход». Сбрасывается только при промоушене
        // (см. ForkForPromotion) и восстанавливается из WorldSnapshot.
    }
    public void RestoreCombat(int hp, int attackCharges)
    {
        if (hp < 0 || hp > MaxHp)
            throw new ArgumentOutOfRangeException(nameof(hp), $"HP must be in [0..{MaxHp}].");
        if (attackCharges < 0 || attackCharges > MaxAttackCharges)
            throw new ArgumentOutOfRangeException(nameof(attackCharges), $"Charges must be in [0..{MaxAttackCharges}].");
        bool wasDead = IsDead;
        Hp = hp;
        AttackCharges = attackCharges;
        if (wasDead && !IsDead)
            DeadStateChanged?.Invoke(this, false);
    }
    public void RestoreFacing(FacingDirection facing)
    {
        if (Facing == facing) return;
        Facing = facing;
        FacingChanged?.Invoke(this);
    }
    public void RestoreFirstActionOfGameTaken(bool value)
    {
        HasTakenFirstActionOfGame = value;
    }
    public void RestoreMovedThisTurn(bool value)
    {
        HasMovedThisTurn = value;
    }
    public void RestoreTotalRotationDegrees(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
        TotalRotationDegreesThisTurn = value;
    }
    public PieceModel ForkForPromotion(string newName, int newAttackDamage, int newMaxHp, int newMaxAttackCharges)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Piece name must not be empty.", nameof(newName));
        if (newAttackDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(newAttackDamage), "AttackDamage must be non-negative.");
        if (newMaxHp <= 0)
            throw new ArgumentOutOfRangeException(nameof(newMaxHp), "MaxHp must be positive.");
        if (newMaxAttackCharges < 0)
            throw new ArgumentOutOfRangeException(nameof(newMaxAttackCharges), "MaxAttackCharges must be non-negative.");

        var newModel = new PieceModel(
            newName,
            Player,
            PositionInGrid,
            newAttackDamage,
            newMaxHp,
            newMaxAttackCharges,
            Facing,
            pieceId: PieceId);
        newModel.RestoreFirstActionOfGameTaken(HasTakenFirstActionOfGame);
        newModel.RestoreMovedThisTurn(HasMovedThisTurn);
        return newModel;
    }
    public void MarkPromotedThisTurn() => HasPromotedThisTurn = true;
    public void RestorePromotedThisTurn(bool value) => HasPromotedThisTurn = value;
    public override string ToString() =>
        $"{Name}#{PieceId} [{Player}] at {PositionInGrid} HP={Hp}/{MaxHp} Charges={AttackCharges}/{MaxAttackCharges} Dmg={AttackDamage} Facing={Facing} HasActed={HasTakenFirstActionOfGame} HasMovedThisTurn={HasMovedThisTurn} RotatedThisTurn={TotalRotationDegreesThisTurn}°";
    public event Action<PieceModel, bool>? DeadStateChanged;
    public event Action<PieceModel, Activity>? FirstActionOfGameTaken;
    public event Action<PieceModel>? FacingChanged;
}