#nullable enable

public interface IAttackEffect
{
    void ApplyTo(IPiece target, IPiece attacker, IRoom room, int effectiveDamage);
}