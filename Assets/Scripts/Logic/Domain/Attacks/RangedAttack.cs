#nullable enable


public sealed class RangedAttack : AttackBase, IRangedAttack
{
    public RangedAttack(Operation operation, int range) : base(operation, range) { }

    public override void Execute(IPiece target, IRoom context)
    {
        target.ApplyOperation(Operation);
    }
}