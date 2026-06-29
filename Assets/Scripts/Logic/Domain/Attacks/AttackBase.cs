#nullable enable

using System;

public abstract class AttackBase : IAttack
{
    protected AttackBase(Operation operation, int range)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Range = Math.Max(range, 1);
    }

    public Operation Operation { get; }
    public int Range { get; }

    public abstract void Execute(IPiece target, IRoom context);

    public virtual void Dispose() { }
}
