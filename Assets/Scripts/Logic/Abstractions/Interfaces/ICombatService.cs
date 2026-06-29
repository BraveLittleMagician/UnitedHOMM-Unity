#nullable enable

public interface ICombatService
{
    IResult PerformMeleeAttack(IPiece attacker, IPiece target, IRoom room);

    IResult PerformRangedAttack(IPiece attacker, IPiece target, IRoom room);

    bool CanAttack(IPiece attacker, IPiece target, IRoom room, out string error);
}