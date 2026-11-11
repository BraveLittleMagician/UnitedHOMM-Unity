#nullable enable

using System;

public sealed class HealAttackEffect : IAttackEffect
{
    public void ApplyTo(IPiece target, IPiece attacker, IRoom room, int effectiveDamage)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (effectiveDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(effectiveDamage), "Heal must be non-negative.");
        if (effectiveDamage == 0) return;
        target.RecoverHp(effectiveDamage);
    }
}