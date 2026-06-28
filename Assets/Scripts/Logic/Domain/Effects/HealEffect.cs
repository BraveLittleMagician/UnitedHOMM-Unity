#nullable enable


public class HealEffect : OneFieldEffect
{
    public HealEffect(int amount) : base(new RecoverOperation(amount)) { }
}