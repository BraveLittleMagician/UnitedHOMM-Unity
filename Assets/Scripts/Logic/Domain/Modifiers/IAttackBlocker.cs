#nullable enable
public interface IAttackBlocker
{
    bool CanAttack(PieceModel piece);
}