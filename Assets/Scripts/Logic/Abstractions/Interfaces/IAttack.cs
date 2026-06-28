#nullable enable


public interface IAttack 
{
    void Execute(IPiece target, IRoom context);
}

public interface IMeleeAttack : IAttack { }
public interface IRangedAttack : IAttack { }