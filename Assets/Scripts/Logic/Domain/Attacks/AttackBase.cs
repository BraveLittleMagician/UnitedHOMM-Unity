#nullable enable

using System;

public abstract class AttackBase : IAttack
{
    protected AttackBase(Operation operation)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
    }

    public Operation Operation { get; }

    public abstract void Execute(IPiece target, IReadOnlyRoom context);
}