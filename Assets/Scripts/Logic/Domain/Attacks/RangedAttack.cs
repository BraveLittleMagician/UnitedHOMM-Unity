#nullable enable

public sealed class RangedAttack : AttackBase, IRangedAttack
{
    public RangedAttack(Operation operation) : base(operation) { }

    public override void Execute(IPiece target, IRoom context)
    {
        target.ApplyOperation(Operation);
    }
}