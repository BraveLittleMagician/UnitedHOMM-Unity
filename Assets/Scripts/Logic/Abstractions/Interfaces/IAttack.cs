#nullable enable

public interface IAttack 
{
    void Execute(IPiece target, IReadOnlyRoom context);
}

public interface IMeleeAttack : IAttack { }
public interface IRangedAttack : IAttack { }