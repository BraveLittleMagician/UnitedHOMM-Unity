#nullable enable

public interface ICombatService
{
    bool CanAttack(IPiece attacker, IPiece target, IRoom room, out string error);
    IResult PerformAttack(IPiece attacker, IPiece target, IRoom room, AttackKind kind);
}