#nullable enable

using System;

public sealed class DamageAttackEffect : IAttackEffect
{
    public void ApplyTo(IPiece target, IPiece attacker, IRoom room, int effectiveDamage)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (effectiveDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(effectiveDamage), "Damage must be non-negative.");
        if (effectiveDamage == 0 || target.IsDead) return;
        target.TakeDamage(effectiveDamage);
    }
}