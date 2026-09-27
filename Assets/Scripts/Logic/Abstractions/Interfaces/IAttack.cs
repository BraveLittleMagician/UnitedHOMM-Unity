#nullable enable

public interface IAttack 
{
    void Execute(IPiece target);
}

public interface IMeleeAttack : IAttack { }
public interface IRangedAttack : IAttack { }