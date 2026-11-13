#nullable enable
using System;
using System.Collections.Generic;

public sealed class FireballModifier : ITargetedModifier
{
    private const int _radius = 3;
    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "Fireball";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;
    public bool TriggersStartedAction => true;

    public void Attach(ModifierContext context)
    {
        _maxCharges = context.InitialCharges;
        _currentCharges = context.InitialCharges;
    }

    public void Detach() { }

    public bool TryConsumeCharge()
    {
        if (_currentCharges < 0) return true;
        if (_currentCharges == 0) return false;
        _currentCharges--;
        return true;
    }

    public void RestoreCharges(int charges)
    {
        if (_maxCharges < 0) { _currentCharges = -1; return; }
        if (charges < 0 || charges > _maxCharges)
            throw new ArgumentOutOfRangeException(nameof(charges), $"Charges must be in [0..{_maxCharges}].");
        _currentCharges = charges;
    }

    public void RestoreDuration(int duration) { }

    public bool CanActivate(ModifierActivationContext context) => !context.Owner.IsDead;

    public void Activate(ModifierActivationContext context)
    {
        throw new InvalidOperationException(
            "FireballModifier must be activated via Activate(context, targetCell).");
    }

    public IReadOnlyList<Square> GetTargetCells(IPiece owner, Square ownerPosition, Square gridSize)
    {
        var result = new List<Square>();
        for (int dx = -_radius; dx <= _radius; dx++)
            for (int dy = -_radius; dy <= _radius; dy++)
                for (int dz = -_radius; dz <= _radius; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    int cheb = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                    if (cheb > _radius) continue;

                    var cell = new Square
                    {
                        X = ownerPosition.X + dx,
                        Y = ownerPosition.Y + dy,
                        Z = ownerPosition.Z + dz,
                        W = ownerPosition.W,
                    };
                    if (cell.X < 0 || cell.X >= gridSize.X) continue;
                    if (cell.Y < 0 || cell.Y >= gridSize.Y) continue;
                    if (cell.Z < 0 || cell.Z >= gridSize.Z) continue;
                    result.Add(cell);
                }
        return result;
    }

    public void Activate(ModifierActivationContext context, Square targetCell)
    {
        if (!context.Registry.TryToGetRoom(context.Owner.IndexInHouse, out var room) || room == null)
            return;
        if (!room.TryToGetPiece(targetCell, out var target) || target == null)
            return;
        if (target.Owner.Equals(context.Owner.Owner))
            return;

        int damage = context.Owner.AttackDamage + context.Host.GetEffectiveDamageBonus(context.Owner);
        var effect = context.Owner.AttackEffect;
        effect.ApplyTo(target, context.Owner, room, damage);

        context.EventBus.Publish(new PieceAttackedEvent(
            context.Owner, target, room, AttackKind.Ranged, Depth: 1));
    }
}