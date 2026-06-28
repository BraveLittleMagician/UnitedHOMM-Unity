#nullable enable


public class DamageEffect : OneFieldEffect
{
    public DamageEffect(int amount) : base(new SubtractOperation(amount)) { }
}
