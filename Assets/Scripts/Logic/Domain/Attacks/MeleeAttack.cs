#nullable enable

public sealed class MeleeAttack : AttackBase, IMeleeAttack
{
    public MeleeAttack(Operation operation) : base(operation) { }

    public override void Execute(IPiece target, IReadOnlyRoom context)
    {
        target.ApplyOperation(Operation);
    }
}