#nullable enable

public interface IAttackEffectProvider
{
    IAttackEffect GetOverride(IPiece piece);
}